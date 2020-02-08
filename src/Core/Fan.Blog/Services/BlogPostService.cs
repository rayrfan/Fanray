using AutoMapper;
using Fan.Blog.Data;
using Fan.Blog.Enums;
using Fan.Blog.Events;
using Fan.Blog.Helpers;
using Fan.Blog.Models;
using Fan.Blog.Services.Interfaces;
using Fan.Blog.Validators;
using Fan.Exceptions;
using Fan.Helpers;
using Fan.Settings;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("Fan.Blog.Tests")]

namespace Fan.Blog.Services
{
    public class BlogPostService : IBlogPostService
    {
        private readonly IPostRepository postRepository;
        private readonly ISettingService settingService;
        private readonly IImageService imageService;
        private readonly IDistributedCache cache;
        private readonly ILogger<BlogPostService> logger;
        private readonly IMapper mapper;
        private readonly IMediator mediator;

        public BlogPostService(
            ISettingService settingService,
            IImageService imageService,
            IPostRepository postRepository,
            IDistributedCache cache,
            ILogger<BlogPostService> logger,
            IMapper mapper,
            IMediator mediator)
        {
            this.settingService = settingService;
            this.imageService = imageService;
            this.postRepository = postRepository;
            this.cache = cache;
            this.mapper = mapper;
            this.logger = logger;
            this.mediator = mediator;
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
            if (blogPost == null) throw new ArgumentNullException(nameof(blogPost));
            await blogPost.ValidateTitleAsync();

            // prep
            var post = await ConvertToPostAsync(blogPost, ECreateOrUpdate.Create);

            // before create
            await mediator.Publish(new BlogPostBeforeCreate
            {
                CategoryTitle = blogPost.CategoryTitle,
                TagTitles = blogPost.TagTitles
            });

            // create (post will get new id)
            await postRepository.CreateAsync(post, blogPost.CategoryTitle, blogPost.TagTitles);

            // invalidate cache only when published
            if (blogPost.Status == EPostStatus.Published)
            {
                await RemoveBlogCacheAsync();
            }

            // after create
            await mediator.Publish(new BlogPostCreated { BlogPost = blogPost });

            return await GetAsync(post.Id);
        }

        /// <summary>
        /// Updates a <see cref="BlogPost"/>.
        /// </summary>
        /// <param name="blogPost">Contains incoming blog post data to update.</param>
        public async Task<BlogPost> UpdateAsync(BlogPost blogPost)
        {
            // validate
            if (blogPost == null || blogPost.Id <= 0) throw new ArgumentException(null, nameof(blogPost));
            await blogPost.ValidateTitleAsync();

            // prep current post with blog post
            var post = await ConvertToPostAsync(blogPost, ECreateOrUpdate.Update);

            // before update
            await mediator.Publish(new BlogPostBeforeUpdate
            {
                CategoryTitle = blogPost.CategoryTitle,
                TagTitles = blogPost.TagTitles,
                PostTags = post.PostTags,
            });

            // update
            await postRepository.UpdateAsync(post, blogPost.CategoryTitle, blogPost.TagTitles);

            // invalidate cache 
            await RemoveBlogCacheAsync();
            await RemoveSinglePostCacheAsync(post);

            // after update
            await mediator.Publish(new BlogPostUpdated { BlogPost = blogPost });

            return await GetAsync(post.Id);
        }

        /// <summary>
        /// Deletes a blog post and invalidates cache for posts on index page.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task DeleteAsync(int id)
        {
            var post = await GetAsync(id);
            await postRepository.DeleteAsync(id);
            await RemoveBlogCacheAsync();
            await RemoveSinglePostCacheAsync(post);
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
            var post = await QueryPostAsync(id);
            return ConvertToBlogPost(post);
        }

        /// <summary>
        /// Returns a <see cref="BlogPost"/> by slug and date time, throws <see cref="FanException"/> if not found.
        /// If the post is draft then it's considered not found.
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
            Post post = null;
            if (new DateTime(year, month, day).IsWithinDays(100))
            {
                var cacheKey = string.Format(BlogCache.KEY_POST, slug, year, month, day);
                post = await cache.GetAsync(cacheKey, BlogCache.Time_SingplePost, async () =>
                {
                    return await postRepository.GetAsync(slug, year, month, day);
                });
            }
            else
            {
                post = await postRepository.GetAsync(slug, year, month, day);
            }

            if (post == null) throw new FanException(EExceptionType.ResourceNotFound);
            var blogPost = ConvertToBlogPost(post);
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
                return await cache.GetAsync(BlogCache.KEY_POSTS_INDEX, BlogCache.Time_Posts_Index, async () =>
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
                PageSize = (await settingService.GetSettingsAsync<BlogSettings>()).PostPerPage,
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
                PageSize = (await settingService.GetSettingsAsync<BlogSettings>()).PostPerPage,
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
        /// Returns specific number of <see cref="BlogPost"/> used by metaweblog.
        /// </summary>
        /// <param name="numberOfPosts">"All" is int.MaxValue</param>
        public async Task<BlogPostList> GetRecentPostsAsync(int numberOfPosts)
        {
            var query = new PostListQuery(EPostListQueryType.BlogPostsByNumber) { PageSize = numberOfPosts };

            return await QueryPostsAsync(query);
        }

        /// <summary>
        /// Returns spcific number of published <see cref="BlogPost"/>.
        /// </summary>
        /// <param name="numberOfPosts"></param>
        /// <returns></returns>
        public async Task<BlogPostList> GetRecentPublishedPostsAsync(int numberOfPosts)
        {
            return await cache.GetAsync(BlogCache.KEY_POSTS_RECENT, BlogCache.Time_Posts_Recent, async () =>
            {
                var query = new PostListQuery(EPostListQueryType.BlogPublishedPostsByNumber)
                {
                    PageSize = numberOfPosts <= 0 ? 1 : numberOfPosts
                };

                return await QueryPostsAsync(query);
            });
        }

        /// <summary>
        /// Invalidates cache for blog post service.
        /// </summary>
        public async Task RemoveBlogCacheAsync()
        {
            await cache.RemoveAsync(BlogCache.KEY_POSTS_INDEX);
            await cache.RemoveAsync(BlogCache.KEY_POSTS_RECENT);
            await cache.RemoveAsync(BlogCache.KEY_ALL_CATS);
            await cache.RemoveAsync(BlogCache.KEY_ALL_TAGS);
            await cache.RemoveAsync(BlogCache.KEY_ALL_ARCHIVES);
            await cache.RemoveAsync(BlogCache.KEY_POST_COUNT);
        }

        // -------------------------------------------------------------------- private methods 

        /// <summary>
        /// Returns a <see cref="Post"/> from data source, throws <see cref="FanException"/> if not found.
        /// </summary>
        /// <param name="id">A blog post id.</param>
        /// <returns></returns>
        /// <remarks>
        /// Returned post is tracked.
        /// </remarks>
        private async Task<Post> QueryPostAsync(int id)
        {
            var post = await postRepository.GetAsync(id, EPostType.BlogPost);

            if (post == null)
            {
                throw new FanException($"Blog post with id {id} is not found.");
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
            var (posts, totalCount) = await postRepository.GetListAsync(query);

            var blogPostList = new BlogPostList
            {
                TotalPostCount = totalCount
            };
            foreach (var post in posts)
            {
                var blogPost = ConvertToBlogPost(post);
                blogPost = await PreRenderAsync(blogPost);
                blogPostList.Posts.Add(blogPost);
            }

            return blogPostList;
        }

        /// <summary>
        /// Prepares a <see cref="BlogPost"/> into Post for create or update.
        /// </summary>
        /// <param name="blogPost">The incoming post with user data.</param>
        /// <param name="createOrUpdate">User is doing either a create or update post.</param>
        /// <returns></returns>
        private async Task<Post> ConvertToPostAsync(BlogPost blogPost, ECreateOrUpdate createOrUpdate)
        {
            // Get post
            var post = (createOrUpdate == ECreateOrUpdate.Create) ? new Post() : await QueryPostAsync(blogPost.Id);

            // CreatedOn
            if (createOrUpdate == ECreateOrUpdate.Create)
            {
                // post time will be min value if user didn't set a time
                post.CreatedOn = (blogPost.CreatedOn <= DateTimeOffset.MinValue) ? DateTimeOffset.UtcNow : blogPost.CreatedOn.ToUniversalTime();
            }
            else 
            {
                // TODO Add a time picker on the composer

                // get post.CreatedOn in local time
                var coreSettings = await settingService.GetSettingsAsync<CoreSettings>();
                var postCreatedOnLocal = post.CreatedOn.ToLocalTime(coreSettings.TimeZoneId);

                // user changed the post time 
                if (!postCreatedOnLocal.YearMonthDayEquals(blogPost.CreatedOn))
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

            // CategoryId
            post.CategoryId = blogPost.CategoryId;

            logger.LogDebug(createOrUpdate + " {@Post}", post);
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
        private BlogPost ConvertToBlogPost(Post post)
        {
            var blogPost = mapper.Map<Post, BlogPost>(post);

            // Title
            blogPost.Title = WebUtility.HtmlDecode(blogPost.Title); // since OLW encodes it, we decode it here

            // Excerpt
            blogPost.Excerpt = post.Excerpt.IsNullOrEmpty() ? Util.GetExcerpt(post.Body, EXCERPT_WORD_LIMIT) : post.Excerpt;

            // CategoryTitle
            blogPost.CategoryTitle = post.Category?.Title;

            // Tags and TagTitles
            foreach (var postTag in post.PostTags)
            {
                blogPost.Tags.Add(postTag.Tag);
                blogPost.TagTitles.Add(postTag.Tag.Title);
            }

            // ViewCount
            blogPost.ViewCount = post.ViewCount;

            logger.LogDebug("Show {@BlogPost}", blogPost);
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
            // make slug
            var slug = Util.Slugify(input, maxlen: PostTitleValidator.TITLE_MAXLEN, randomCharCountOnEmpty: 8);

            // make sure slug is unique
            int i = 2;
            if (createOrUpdate == ECreateOrUpdate.Create) // create
            {
                while (await postRepository.GetAsync(slug, createdOn.Year, createdOn.Month, createdOn.Day) != null)
                {
                    slug = Util.UniquefySlug(slug, ref i);
                }
            }
            else // update
            {
                var p = await postRepository.GetAsync(slug, createdOn.Year, createdOn.Month, createdOn.Day);
                while (p != null && p.Id != blogPostId)
                {
                    slug = Util.UniquefySlug(slug, ref i);
                    p = await postRepository.GetAsync(slug, createdOn.Year, createdOn.Month, createdOn.Day);
                }
            }

            return slug;
        }

        private async Task RemoveSinglePostCacheAsync(Post post)
        {
            var cacheKey = string.Format(BlogCache.KEY_POST, post.Slug, post.CreatedOn.Year, post.CreatedOn.Month, post.CreatedOn.Day);
            await cache.RemoveAsync(cacheKey);
        }

        /// <summary>
        /// Pre render processing of a blog post. TODO consider refactor.
        /// </summary>
        /// <param name="blogPost"></param>
        /// <returns></returns>
        private async Task<BlogPost> PreRenderAsync(BlogPost blogPost)
        {
            if (blogPost == null) return blogPost;

            blogPost.Body = OembedParser.Parse(blogPost.Body);
            blogPost.Body = await imageService.ProcessResponsiveImageAsync(blogPost.Body);

            return blogPost;
        }
    }
}
