using AutoMapper;
using Fan.Blog.Categories;
using Fan.Blog.Data;
using Fan.Blog.Enums;
using Fan.Blog.Helpers;
using Fan.Blog.Models;
using Fan.Blog.Posts;
using Fan.Blog.Tags;
using Fan.Blog.Validators;
using Fan.Exceptions;
using Fan.Helpers;
using Fan.Medias;
using Fan.Settings;
using Fan.Shortcodes;
using FluentValidation.Results;
using Humanizer;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("Fan.Blog.IntegrationTests")]
[assembly: InternalsVisibleTo("Fan.Blog.UnitTests")]

namespace Fan.Blog.Services
{
    public partial class BlogService : IBlogService
    {
        private readonly ISettingService _settingSvc;
        private readonly ICategoryRepository _catRepo;
        private readonly IPostRepository _postRepo;
        private readonly IDistributedCache _cache;
        private readonly ILogger<BlogService> _logger;
        private readonly IMapper _mapper;
        private readonly IShortcodeService _shortcodeSvc;
        private readonly IMediator _mediator;
        private readonly IMediaService _mediaSvc;
        private readonly IStorageProvider _storageProvider;
        private readonly AppSettings _appSettings;

        public BlogService(
            ISettingService settingService,
            ICategoryRepository catRepo,
            IPostRepository postRepo,
            IMediaService mediaSvc,
            IStorageProvider storageProvider,
            IOptionsSnapshot<AppSettings> appSettings,
            IDistributedCache cache,
            ILogger<BlogService> logger,
            IMapper mapper,
            IShortcodeService shortcodeService,
            IMediator mediator)
        {
            _settingSvc = settingService;
            _catRepo = catRepo;
            _postRepo = postRepo;
            _mediaSvc = mediaSvc;
            _storageProvider = storageProvider;
            _appSettings = appSettings.Value;
            _cache = cache;
            _mapper = mapper;
            _logger = logger;
            _shortcodeSvc = shortcodeService;
            _mediator = mediator;
        }

        // -------------------------------------------------------------------- Archives

        /// <summary>
        /// Returns archive information.
        /// </summary>
        /// <returns></returns>
        public async Task<Dictionary<int, List<MonthItem>>> GetArchivesAsync()
        {
            return await _cache.GetAsync(BlogCache.KEY_ALL_ARCHIVES, BlogCache.Time_Archives, async () =>
            {
                var months = new Dictionary<DateTime, int>();
                var years = new Dictionary<int, List<MonthItem>>();

                var dates = await _postRepo.GetPostDateTimesAsync();
                foreach (var month in dates)
                {
                    months.TryGetValue(month, out int count);
                    ++count;
                    months[month] = count;
                }

                foreach (var month in months)
                {
                    int year = month.Key.Year;
                    if (!years.Keys.Contains(year))
                    {
                        years.Add(year, new List<MonthItem>());
                    }

                    years[year].Add(new MonthItem
                    {
                        Title = month.Key.ToString("MMMM"),
                        Url = BlogRoutes.GetArchiveRelativeLink(year, month.Key.Month),
                        Count = month.Value,
                    });
                }

                return years;
            });
        }

        // -------------------------------------------------------------------- BlogPosts 

        /// <summary>
        /// Creates a <see cref="BlogPost"/>.
        /// </summary>
        /// <param name="blogPost">Contains incoming blog post data to create.</param>
        /// <returns></returns>
        /// <remarks>
        /// It creates tags, post and invalidates cache for posts on index page.
        /// </remarks>
        public async Task<BlogPost> CreatePostAsync(BlogPost blogPost)
        {
            if (blogPost == null) return blogPost;

            // prep
            var post = await PrepPostAsync(blogPost, ECreateOrUpdate.Create);

            // before create
            await _mediator.Publish(new BlogPostBeforeCreate
            {
                CategoryTitle = blogPost.CategoryTitle,
                TagTitles = blogPost.TagTitles
            });

            // create
            await _postRepo.CreateAsync(post, blogPost.CategoryId, blogPost.CategoryTitle, blogPost.TagTitles);

            // invalidate cache only when published
            if (blogPost.Status == EPostStatus.Published)
            {
                await RemoveAllCacheAsync();
            }

            // after create
            await _mediator.Publish(new BlogPostCreated { BlogPost = blogPost });

            return await GetPostAsync(post.Id);
        }

        /// <summary>
        /// Updates a <see cref="BlogPost"/>.
        /// </summary>
        /// <param name="blogPost">Contains incoming blog post data to update.</param>
        public async Task<BlogPost> UpdatePostAsync(BlogPost blogPost)
        {
            if (blogPost == null) return blogPost;

            // prep
            var post = await PrepPostAsync(blogPost, ECreateOrUpdate.Update);

            // before update
            await _mediator.Publish(new BlogPostBeforeUpdate
            {
                CategoryTitle = blogPost.CategoryTitle,
                TagTitles = blogPost.TagTitles,
                CurrentPost = await QueryPostAsync(blogPost.Id, EPostType.BlogPost),
            });

            // update
            await _postRepo.UpdateAsync(post, blogPost.CategoryId, blogPost.CategoryTitle, blogPost.TagTitles);

            // invalidate cache 
            await RemoveAllCacheAsync();

            // after update
            await _mediator.Publish(new BlogPostUpdated { BlogPost = blogPost });

            return await GetPostAsync(post.Id);
        }

        /// <summary>
        /// Deletes a blog post and invalidates cache for posts on index page.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task DeletePostAsync(int id)
        {
            await _postRepo.DeleteAsync(id);
            await RemoveAllCacheAsync();
        }

        /// <summary>
        /// Returns a <see cref="BlogPost"/> by id with its <see cref="Category"/> and <see cref="Tag"/>
        /// and throws <see cref="FanException"/> if not found.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <exception cref="FanException">if post is not found.</exception>
        /// <remarks>
        /// This is used when you want to get a post from db.
        /// </remarks>
        public async Task<BlogPost> GetPostAsync(int id)
        {
            var post = await QueryPostAsync(id, EPostType.BlogPost);
            if (post == null) throw new FanException("Blog post not found.");
            return await GetBlogPostAsync(post, parseShortcode: false);
        }

        /// <summary>
        /// Returns a <see cref="BlogPost"/> by slug and date time, throws <see cref="FanException"/> if not found.
        /// </summary>
        /// <param name="slug"></param>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="day"></param>
        /// <exception cref="FanException">if post is not found.</exception>
        /// <remarks>
        /// This is used by controller. The dates are stored as UTC time in database, thus the 
        /// params year, month and day should be the UTC time.
        /// </remarks>
        public async Task<BlogPost> GetPostAsync(string slug, int year, int month, int day)
        {
            // todo caching
            var post = await _postRepo.GetAsync(slug, year, month, day);
            if (post == null) throw new FanException("Blog post not found.");
            return await GetBlogPostAsync(post, parseShortcode: true);
        }

        /// <summary>
        /// Returns a list of blog posts for the blog index page.
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public async Task<BlogPostList> GetPostsAsync(int pageIndex, int pageSize)
        {
            PostListQuery query = new PostListQuery(EPostListQueryType.BlogPosts)
            {
                PageIndex = (pageIndex <= 0) ? 1 : pageIndex,
                PageSize = pageSize,
            };

            // TODO cache only first page of the public site not admin
            //if (query.PageIndex == 1)
            //{
            //    return await _cache.GetAsync(CACHE_KEY_POSTS_INDEX, CacheTime_PostsIndex, async () =>
            //    {
            //        return await QueryPostsAsync(query);
            //    });
            //}

            return await QueryPostsAsync(query);
        }

        /// <summary>
        /// Returns a list of blog posts for a category.
        /// </summary>
        /// <param name="categorySlug"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public async Task<BlogPostList> GetPostsForCategoryAsync(string categorySlug, int pageIndex)
        {
            if (categorySlug.IsNullOrEmpty()) throw new FanException("Category does not exist.");

            // todo caching
            PostListQuery query = new PostListQuery(EPostListQueryType.BlogPostsByCategory)
            {
                CategorySlug = categorySlug,
                PageIndex = (pageIndex <= 0) ? 1 : pageIndex,
                PageSize = (await _settingSvc.GetSettingsAsync<BlogSettings>()).PostPerPage,
            };

            return await QueryPostsAsync(query);
        }

        /// <summary>
        /// Returns a list of blog posts for a tag.
        /// </summary>
        /// <param name="tagSlug"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public async Task<BlogPostList> GetPostsForTagAsync(string tagSlug, int pageIndex)
        {
            if (tagSlug.IsNullOrEmpty()) throw new FanException("Tag does not exist.");

            // todo caching
            PostListQuery query = new PostListQuery(EPostListQueryType.BlogPostsByTag)
            {
                TagSlug = tagSlug,
                PageIndex = (pageIndex <= 0) ? 1 : pageIndex,
                PageSize = (await _settingSvc.GetSettingsAsync<BlogSettings>()).PostPerPage,
            };

            return await QueryPostsAsync(query);
        }

        /// <summary>
        /// Returns a list of blog posts for archive.
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="page"></param>
        /// <returns></returns>
        public async Task<BlogPostList> GetPostsForArchive(int? year, int? month, int page = 1)
        {
            if (!year.HasValue) throw new FanException("Year must be provided.");
            var query = new PostListQuery(EPostListQueryType.BlogPostsArchive)
            {
                Year = year.Value,
                Month = month
            };

            return await QueryPostsAsync(query);
        }

        /// <summary>
        /// Returns a list of blog drafts.
        /// </summary>
        /// <returns></returns>
        public async Task<BlogPostList> GetPostsForDraftsAsync()
        {
            PostListQuery query = new PostListQuery(EPostListQueryType.BlogDrafts);

            return await QueryPostsAsync(query);
        }

        /// <summary>
        /// Returns specified number of <see cref="BlogPost"/> used by metaweblog.
        /// </summary>
        /// <param name="numberOfPosts">"All" is int.MaxValue</param>
        public async Task<BlogPostList> GetRecentPostsAsync(int numberOfPosts)
        {
            var query = new PostListQuery(EPostListQueryType.BlogPostsByNumber) { PageSize = numberOfPosts };

            return await QueryPostsAsync(query);
        }

        /// <summary>
        /// Returns total number of posts by each <see cref="EPostStatus"/>.
        /// </summary>
        /// <returns></returns>
        public async Task<PostCount> GetPostCountAsync()
        {
            return await _cache.GetAsync(BlogCache.KEY_POST_COUNT, BlogCache.Time_PostCount, async () =>
            {
                return await _postRepo.GetPostCountAsync();
            });
        }

        /// <summary>
        /// Returns a <see cref="Post"/> from data source, throws <see cref="FanException"/> if not found.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        /// <remarks>
        /// This returns Post not a BlogPost, which would maintain tracking for <see cref="PrepPostAsync(BlogPost, string)"/>.
        /// </remarks>
        private async Task<Post> QueryPostAsync(int id, EPostType type)
        {
            var post = await _postRepo.GetAsync(id, type);

            if (post == null)
            {
                throw new FanException($"{type} with id {id} is not found.");
            }

            return post;
        }

        /// <summary>
        /// Returns a <see cref="BlogPostList"/> based on query from data source.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        private async Task<BlogPostList> QueryPostsAsync(PostListQuery query)
        {
            var (posts, totalCount) = await _postRepo.GetListAsync(query);

            var blogPostList = new BlogPostList
            {
                PostCount = totalCount
            };
            foreach (var post in posts)
            {
                blogPostList.Posts.Add(await GetBlogPostAsync(post, parseShortcode: true));
            }

            return blogPostList;
        }

        /// <summary>
        /// Prepares a <see cref="BlogPost"/> into Post for create or update.
        /// </summary>
        /// <param name="blogPost">The incoming post with user data.</param>
        /// <param name="createOrUpdate">User is doing either a create or update post.</param>
        /// <returns></returns>
        private async Task<Post> PrepPostAsync(BlogPost blogPost, ECreateOrUpdate createOrUpdate)
        {
            // Validation
            if (blogPost.Status != EPostStatus.Draft) // skip if it's a draft
            {
                var validator = new PostValidator();
                ValidationResult result = await validator.ValidateAsync(blogPost);
                if (!result.IsValid)
                {
                    throw new FanException($"Failed to {createOrUpdate.ToString().ToLower()} blog post.", result.Errors);
                }
            }

            // Get post
            // NOTE: can't use this.GetPostAsync(blogPost.Id) as it returns a BlogPost not a Post which would lose tracking
            var post = (createOrUpdate == ECreateOrUpdate.Create) ? new Post() : await QueryPostAsync(blogPost.Id, EPostType.BlogPost);
            var coreSettings = await _settingSvc.GetSettingsAsync<CoreSettings>();

            // CreatedOn
            if (createOrUpdate == ECreateOrUpdate.Create)
            {
                // post time will be min value if user didn't set a time
                post.CreatedOn = (blogPost.CreatedOn <= DateTimeOffset.MinValue) ? DateTimeOffset.UtcNow : blogPost.CreatedOn.ToUniversalTime();
            }
            else if (post.CreatedOn != blogPost.CreatedOn) // user changed in post time
            {
                post.CreatedOn = (blogPost.CreatedOn <= DateTimeOffset.MinValue) ? post.CreatedOn : blogPost.CreatedOn.ToUniversalTime();
            }

            // UpdatedOn (DraftSavedOn)
            // when saving a draft this should be utc now, when publishing it becomes null
            if (blogPost.Status == EPostStatus.Draft) post.UpdatedOn = DateTimeOffset.UtcNow;
            else post.UpdatedOn = null;

            // Slug 
            if (blogPost.Status == EPostStatus.Draft && blogPost.Title.IsNullOrEmpty())
                post.Slug = null; // if user save a draft with empty title
            else
                post.Slug = await GetBlogPostSlugAsync(blogPost.Slug.IsNullOrEmpty() ? blogPost.Title : blogPost.Slug,
                                                       post.CreatedOn, createOrUpdate, blogPost.Id);

            // Title
            post.Title = blogPost.Title; // looks like OLW html encodes post title

            // Body & Excerpt, UserId
            post.Body = blogPost.Body.IsNullOrWhiteSpace() ? null : blogPost.Body;
            post.Excerpt = blogPost.Excerpt.IsNullOrWhiteSpace() ? null : blogPost.Excerpt;
            post.UserId = blogPost.UserId;

            // Status & CommentStatus
            post.Status = blogPost.Status;
            post.CommentStatus = blogPost.CommentStatus;

            _logger.LogDebug(createOrUpdate + " {@Post}", post);
            return post;
        }

        /// <summary>
        /// Gets a <see cref="BlogPost"/> for display to client from a <see cref="Post"/>.
        /// </summary>
        /// <param name="post"></param>
        /// <param name="parseShortcode">True will parse shortcode into html, false otherwise.</param>
        /// <returns></returns>
        /// <remarks>
        /// It readies <see cref="Post.CreatedOnDisplay"/>, Title, Excerpt, CategoryTitle, Tags and Body with shortcodes.
        /// </remarks>
        private async Task<BlogPost> GetBlogPostAsync(Post post, bool parseShortcode)
        {
            var blogPost = _mapper.Map<Post, BlogPost>(post);
            var coreSettings = await _settingSvc.GetSettingsAsync<CoreSettings>();
            var blogSettings = await _settingSvc.GetSettingsAsync<BlogSettings>();

            // Friendly post time if the post was published within 2 days
            // else show the actual date time in setting's timezone
            blogPost.CreatedOnDisplay = (DateTimeOffset.UtcNow.Day - blogPost.CreatedOn.Day) > 2 ?
                Util.ConvertTime(blogPost.CreatedOn, coreSettings.TimeZoneId).ToString("dddd, MMMM dd, yyyy") :
                Util.ConvertTime(blogPost.CreatedOn, coreSettings.TimeZoneId).Humanize();

            if (blogPost.UpdatedOn.HasValue)
            {
                blogPost.UpdatedOnDisplay =
                    Util.ConvertTime(blogPost.UpdatedOn.Value, coreSettings.TimeZoneId).ToString("MM/dd/yyyy");
            }

            // Title
            blogPost.Title = WebUtility.HtmlDecode(blogPost.Title); // since OLW encodes it, we decode it here

            // Excerpt
            blogPost.Excerpt = post.Excerpt.IsNullOrEmpty() ? Util.GetExcerpt(post.Body, EXCERPT_WORD_LIMIT) : post.Excerpt;

            // CategoryTitle
            blogPost.CategoryTitle = post.Category.Title;

            // Tags and TagTitles
            foreach (var postTag in post.PostTags)
            {
                blogPost.Tags.Add(postTag.Tag);
                blogPost.TagTitles.Add(postTag.Tag.Title);
            }

            // Shortcodes
            blogPost.Body = parseShortcode ? _shortcodeSvc.Parse(post.Body) : post.Body;

            // Embeds
            blogPost.Body = parseShortcode ? OembedParser.Parse(blogPost.Body) : blogPost.Body;

            _logger.LogDebug("Show {@BlogPost}", blogPost);
            return blogPost;
        }

        /// <summary>
        /// Returns a unique and valid slug for a blog post.
        /// </summary>
        /// <param name="input">This could be a slug or post title.</param>
        /// <param name="createdOn">Used for making sure slug is unique by searching posts.</param>
        /// <param name="createOrUpdate">Whether the operation is create or update post.</param>
        /// <param name="blogPostId">Used for making sure slug is unique when updating.</param>
        /// <returns></returns>
        /// <remarks>
        /// If input is slug, either this is update or a create with user inputted slug, then <see cref="Util.FormatSlug(string)"/>
        /// will not alter it. This is very important for SEO as updating slug on an existing post will
        /// break links in search results. On the other hand, if user deliberately updated the slug
        /// when doing an update on post, then it will alter it accordingly. Please see the test case
        /// on this method.
        /// </remarks>
        internal async Task<string> GetBlogPostSlugAsync(string input, DateTimeOffset createdOn, ECreateOrUpdate createOrUpdate, int blogPostId)
        {
            // when user manually inputted a slug, it could exceed max len
            if (input.Length > PostValidator.POST_TITLE_SLUG_MAXLEN)
            {
                input = input.Substring(0, PostValidator.POST_TITLE_SLUG_MAXLEN);
            }

            // remove/replace odd char, lower case etc
            var slug = Util.FormatSlug(input);

            // slug from title could be empty, e.g. the title is in Chinese
            // then we generate a random string of 6 chars
            if (string.IsNullOrEmpty(slug))
            {
                slug = Util.RandomString(8);
            }

            // make sure slug is unique
            int i = 2;
            if (createOrUpdate == ECreateOrUpdate.Create) // create
            {
                while (await _postRepo.GetAsync(slug, createdOn.Year, createdOn.Month, createdOn.Day) != null)
                {
                    slug = $"{slug}-{i}";
                    i++;
                }
            }
            else // update
            {
                var p = await _postRepo.GetAsync(slug, createdOn.Year, createdOn.Month, createdOn.Day);
                while (p != null && p.Id != blogPostId)
                {
                    slug = $"{slug}-{i}";
                    i++;
                    p = await _postRepo.GetAsync(slug, createdOn.Year, createdOn.Month, createdOn.Day);
                }
            }

            return slug;
        }

        /// <summary>
        /// Remove all cached objects for blog.
        /// </summary>
        private async Task RemoveAllCacheAsync()
        {
            await _cache.RemoveAsync(BlogCache.KEY_POSTS_INDEX);
            await _cache.RemoveAsync(BlogCache.KEY_ALL_CATS);
            await _cache.RemoveAsync(BlogCache.KEY_ALL_TAGS);
            await _cache.RemoveAsync(BlogCache.KEY_ALL_ARCHIVES);
            await _cache.RemoveAsync(BlogCache.KEY_POST_COUNT);
        }
    }
}
