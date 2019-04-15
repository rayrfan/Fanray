using AutoMapper;
using Fan.Blog.Data;
using Fan.Blog.Enums;
using Fan.Blog.Events;
using Fan.Blog.Helpers;
using Fan.Blog.Models;
using Fan.Blog.Services.Interfaces;
using Fan.Exceptions;
using Fan.Helpers;
using Fan.Settings;
using Humanizer;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("Fan.Blog.IntegrationTests")]
[assembly: InternalsVisibleTo("Fan.Blog.UnitTests")]

namespace Fan.Blog.Services
{
    public partial class BlogPostService : IBlogPostService
    {
        private readonly IPostRepository _postRepo;
        private readonly ISettingService _settingSvc;
        private readonly IImageService _imageService;
        private readonly IDistributedCache _cache;
        private readonly ILogger<BlogPostService> _logger;
        private readonly IMapper _mapper;
        private readonly IMediator _mediator;

        public BlogPostService(
            ISettingService settingService,
            IImageService imageService,
            IPostRepository postRepo,
            IDistributedCache cache,
            ILogger<BlogPostService> logger,
            IMapper mapper,
            IMediator mediator)
        {
            _settingSvc = settingService;
            _imageService = imageService;
            _postRepo = postRepo;
            _cache = cache;
            _mapper = mapper;
            _logger = logger;
            _mediator = mediator;
        }

        // -------------------------------------------------------------------- consts

        /// <summary>
        /// By default show 10 posts per page.
        /// </summary>
        public const int DEFAULT_PAGE_SIZE = 10;

        /// <summary>
        /// Showing post starts with page 1.
        /// </summary>
        public const int DEFAULT_PAGE_INDEX = 1;

        /// <summary>
        /// How many words to extract into excerpt from body. Default 55.
        /// </summary>
        public const int EXCERPT_WORD_LIMIT = 55;

        public const int TITLE_MAXLEN = 256;

        // -------------------------------------------------------------------- public methods 

        /// <summary>
        /// Creates a <see cref="BlogPost"/>.
        /// </summary>
        /// <param name="blogPost">Contains incoming blog post data to create.</param>
        /// <returns></returns>
        /// <remarks>
        /// It creates tags, post and invalidates cache for posts on index page.
        /// </remarks>
        public async Task<BlogPost> CreateAsync(BlogPost blogPost)
        {
            // validate
            ValidatePost(blogPost);

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

            return await GetAsync(post.Id);
        }

        /// <summary>
        /// Updates a <see cref="BlogPost"/>.
        /// </summary>
        /// <param name="blogPost">Contains incoming blog post data to update.</param>
        public async Task<BlogPost> UpdateAsync(BlogPost blogPost)
        {
            // validate
            ValidatePost(blogPost);

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

            return await GetAsync(post.Id);
        }

        /// <summary>
        /// Deletes a blog post and invalidates cache for posts on index page.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task DeleteAsync(int id)
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
        public async Task<BlogPost> GetAsync(int id)
        {
            var post = await QueryPostAsync(id, EPostType.BlogPost);
            if (post == null) throw new FanException("Blog post not found.");
            return await GetBlogPostAsync(post);
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
        public async Task<BlogPost> GetAsync(string slug, int year, int month, int day)
        {
            // todo caching
            var post = await _postRepo.GetAsync(slug, year, month, day);
            if (post == null) throw new FanException("Blog post not found.");
            var blogPost = await GetBlogPostAsync(post);
            blogPost = await PreRenderAsync(blogPost);
            return blogPost;
        }

        /// <summary>
        /// Returns a list of blog posts.
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        /// <remarks>
        /// For the blog index page, admin post page, main rss feed.
        /// </remarks>
        public async Task<BlogPostList> GetListAsync(int pageIndex, int pageSize, bool cacheable = true)
        {
            PostListQuery query = new PostListQuery(EPostListQueryType.BlogPosts)
            {
                PageIndex = (pageIndex <= 0) ? 1 : pageIndex,
                PageSize = pageSize,
            };

            // cache only first page of the public site, not admin or rss
            if (query.PageIndex == 1 && cacheable)
            {
                return await _cache.GetAsync(BlogCache.KEY_POSTS_INDEX, BlogCache.Time_PostsIndex, async () =>
                {
                    return await QueryPostsAsync(query);
                });
            }

            return await QueryPostsAsync(query);
        }

        /// <summary>
        /// Returns a list of blog posts for a category.
        /// </summary>
        /// <param name="categorySlug"></param>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public async Task<BlogPostList> GetListForCategoryAsync(string categorySlug, int pageIndex)
        {
            if (categorySlug.IsNullOrEmpty()) throw new FanException("Category does not exist.");

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
        public async Task<BlogPostList> GetListForTagAsync(string tagSlug, int pageIndex)
        {
            if (tagSlug.IsNullOrEmpty()) throw new FanException("Tag does not exist.");

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
        public async Task<BlogPostList> GetListForArchive(int? year, int? month, int page = 1)
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
        public async Task<BlogPostList> GetListForDraftsAsync()
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

        // -------------------------------------------------------------------- private methods 

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
                var blogPost = await GetBlogPostAsync(post);
                blogPost = await PreRenderAsync(blogPost);
                blogPostList.Posts.Add(blogPost);
            }

            return blogPostList;
        }

        /// <summary>
        /// Validates a blog post and throws exception if validation fails.
        /// </summary>
        /// <param name="blogPost"></param>
        /// <returns></returns>
        private void ValidatePost(BlogPost blogPost)
        {
            // validate
            var errMsg = "";
            if (blogPost == null) errMsg = "Invalid blog post.";
            else if (blogPost.Status != EPostStatus.Draft && blogPost.Title.IsNullOrEmpty()) errMsg = "Blog post title cannot be empty.";
            else if (!blogPost.Title.IsNullOrEmpty() && blogPost.Title.Length > TITLE_MAXLEN) errMsg = $"Blog post title cannot exceed {TITLE_MAXLEN} chars.";
            if (!errMsg.IsNullOrEmpty()) throw new FanException(errMsg);
        }

        /// <summary>
        /// Prepares a <see cref="BlogPost"/> into Post for create or update.
        /// </summary>
        /// <param name="blogPost">The incoming post with user data.</param>
        /// <param name="createOrUpdate">User is doing either a create or update post.</param>
        /// <returns></returns>
        private async Task<Post> PrepPostAsync(BlogPost blogPost, ECreateOrUpdate createOrUpdate)
        {
            // Get post
            // NOTE: can't use this.GetPostAsync(blogPost.Id) as it returns a BlogPost not a Post which would lose tracking
            var post = (createOrUpdate == ECreateOrUpdate.Create) ? new Post() : await QueryPostAsync(blogPost.Id, EPostType.BlogPost);
            //var coreSettings = await _settingSvc.GetSettingsAsync<CoreSettings>();

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
        /// <returns></returns>
        /// <remarks>
        /// It readies <see cref="Post.CreatedOnDisplay"/>, Title, Excerpt, CategoryTitle, Tags and Body with shortcodes.
        /// </remarks>
        private async Task<BlogPost> GetBlogPostAsync(Post post)
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
        /// If input is slug, either this is update or a create with user inputted slug, then <see cref="Util.Slugify(string)"/>
        /// will not alter it. This is very important for SEO as updating slug on an existing post will
        /// break links in search results. On the other hand, if user deliberately updated the slug
        /// when doing an update on post, then it will alter it accordingly. Please see the test case
        /// on this method.
        /// </remarks>
        internal async Task<string> GetBlogPostSlugAsync(string input, DateTimeOffset createdOn, ECreateOrUpdate createOrUpdate, int blogPostId)
        {
            // when user manually inputted a slug, it could exceed max len
            if (input.Length > TITLE_MAXLEN)
            {
                input = input.Substring(0, TITLE_MAXLEN);
            }

            // make slug
            var slug = Util.Slugify(input, randomCharCountOnEmpty: 8);

            // make sure slug is unique
            int i = 2;
            if (createOrUpdate == ECreateOrUpdate.Create) // create
            {
                while (await _postRepo.GetAsync(slug, createdOn.Year, createdOn.Month, createdOn.Day) != null)
                {
                    slug = Util.UniquefySlug(slug, ref i);
                }
            }
            else // update
            {
                var p = await _postRepo.GetAsync(slug, createdOn.Year, createdOn.Month, createdOn.Day);
                while (p != null && p.Id != blogPostId)
                {
                    slug = Util.UniquefySlug(slug, ref i);
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

        /// <summary>
        /// Pre render processing of a blog post.
        /// </summary>
        /// <param name="blogPost"></param>
        /// <returns></returns>
        private async Task<BlogPost> PreRenderAsync(BlogPost blogPost)
        {
            if (blogPost == null) return blogPost;

            blogPost.Body = OembedParser.Parse(blogPost.Body);
            blogPost.Body = await _imageService.ProcessResponsiveImageAsync(blogPost.Body);

            return blogPost;
        }
    }
}
