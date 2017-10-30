using AutoMapper;
using Fan.Blogs.Data;
using Fan.Blogs.Enums;
using Fan.Blogs.Helpers;
using Fan.Blogs.Models;
using Fan.Blogs.Validators;
using Fan.Enums;
using Fan.Exceptions;
using Fan.Helpers;
using Fan.Models;
using Fan.Services;
using Fan.Shortcodes;
using FluentValidation.Results;
using Humanizer;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Fan.Blogs.Services
{
    public class BlogService : IBlogService
    {
        private readonly IHostingEnvironment _hostingEnvironment;
        private readonly ISettingService _settingSvc;
        private readonly ICategoryRepository _catRepo;
        private readonly IPostRepository _postRepo;
        private readonly ITagRepository _tagRepo;
        private readonly IMediaRepository _mediaRepo;
        private readonly IDistributedCache _cache;
        private readonly ILogger<BlogService> _logger;
        private readonly IMapper _mapper;
        private readonly IShortcodeService _shortcodeSvc;

        public BlogService(
            ISettingService settingService,
            ICategoryRepository catRepo,
            IPostRepository postRepo,
            ITagRepository tagRepo,
            IMediaRepository mediaRepo,
            IHostingEnvironment env,
            IDistributedCache cache,
            ILogger<BlogService> logger,
            IMapper mapper,
            IShortcodeService shortcodeService)
        {
            _settingSvc = settingService;
            _catRepo = catRepo;
            _postRepo = postRepo;
            _tagRepo = tagRepo;
            _mediaRepo = mediaRepo;
            _hostingEnvironment = env;
            _cache = cache;
            _mapper = mapper;
            _logger = logger;
            _shortcodeSvc = shortcodeService;
        }

        public const string CACHE_KEY_ALL_CATS = "BlogCategories";
        public const string CACHE_KEY_ALL_TAGS = "BlogTags";
        public const string CACHE_KEY_POSTS_INDEX = "BlogPostsIndex";

        // -------------------------------------------------------------------- Categories

        /// <summary>
        /// Creates a <see cref="Category"/> and invalidates cache for all categories.
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public async Task<Category> CreateCategoryAsync(Category category)
        {
            category = await this.PrepTaxonomyAsync(category, ECreateOrUpdate.Create) as Category;
            category = await _catRepo.CreateAsync(category);
            await _cache.RemoveAsync(CACHE_KEY_ALL_CATS); // TODO what about posts

            return category;
        }

        /// <summary>
        /// Deletes a <see cref="Category"/> and reassigns posts to a default category, and 
        /// invalidates caceh for all categories.  Throws <see cref="FanException"/> if the
        /// category being deleted is the default category.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <remarks>
        /// The default category cannot be deleted, on the UI there is no delete button available
        /// for default category, thus when there is only one category left, it'll be the default,
        /// and you'll always have it available.
        /// </remarks>
        public async Task DeleteCategoryAsync(int id)
        {
            var blogSettings = await _settingSvc.GetSettingsAsync<BlogSettings>();

            // on the UI there is no delete button on the default cat
            // therefore when there is only one category left, it'll be the default.
            if (id == blogSettings.DefaultCategoryId)
            {
                throw new FanException("Default category cannot be deleted.");
            }

            await _catRepo.DeleteAsync(id, blogSettings.DefaultCategoryId);
            await _cache.RemoveAsync(CACHE_KEY_ALL_CATS);
        }

        /// <summary>
        /// Returns category by id, throws <see cref="FanException"/> if category with id is not found.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Category> GetCategoryAsync(int id)
        {
            var cats = await GetCategoriesAsync();
            var cat = cats.SingleOrDefault(c => c.Id == id);
            if (cat == null)
            {
                throw new FanException($"Category with id '{id}' is not found.");
            }

            return cat;
        }

        /// <summary>
        /// Returns category by slug, throws <see cref="FanException"/> if category with slug is not found.
        /// </summary>
        /// <param name="slug"></param>
        /// <returns></returns>
        public async Task<Category> GetCategoryAsync(string slug)
        {
            var cats = await GetCategoriesAsync();
            var cat = cats.SingleOrDefault(c => c.Slug.Equals(slug, StringComparison.CurrentCultureIgnoreCase));
            if (cat == null)
            {
                throw new FanException($"Category with slug '{slug}' is not found.");
            }

            return cat;
        }

        /// <summary>
        /// Returns all categories, cached after calls to DAL.
        /// </summary>
        /// <returns></returns>
        public async Task<List<Category>> GetCategoriesAsync()
        {
            return await _cache.GetAsync(CACHE_KEY_ALL_CATS, new TimeSpan(0, 10, 0), async () => {
                return await _catRepo.GetListAsync();
            });
        }

        /// <summary>
        /// Updates a <see cref="Category"/> and invalidates cache for all categories.
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public async Task<Category> UpdateCategoryAsync(Category category)
        {
            category = await PrepTaxonomyAsync(category, ECreateOrUpdate.Update) as Category;
            category = await _catRepo.UpdateAsync(category);
            await _cache.RemoveAsync(CACHE_KEY_ALL_CATS);

            return category;
        }

        // -------------------------------------------------------------------- Tags

        /// <summary>
        /// Creates a <see cref="Tag"/> and invalidates cache for all tags.
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public async Task<Tag> CreateTagAsync(Tag tag)
        {
            tag = await PrepTaxonomyAsync(tag, ECreateOrUpdate.Create) as Tag;
            tag = await _tagRepo.CreateAsync(tag);
            await _cache.RemoveAsync(CACHE_KEY_ALL_TAGS);

            return tag;
        }

        /// <summary>
        /// Deletes a <see cref="Tag"/> by id and invalidates cache for all tags.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task DeleteTagAsync(int id)
        {
            await _tagRepo.DeleteAsync(id);
            await _cache.RemoveAsync(CACHE_KEY_ALL_TAGS);
        }

        /// <summary>
        /// Returns tag by id, throws <see cref="FanException"/> if tag with id is not found.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Tag> GetTagAsync(int id)
        {
            var tags = await this.GetTagsAsync();
            var tag = tags.SingleOrDefault(c => c.Id == id);
            if (tag == null)
            {
                throw new FanException($"Tag with id '{id}' is not found.");
            }

            return tag;
        }

        /// <summary>
        /// Returns tag by slug, throws <see cref="FanException"/> if tag with slug is not found.
        /// </summary>
        /// <param name="slug"></param>
        /// <returns></returns>
        public async Task<Tag> GetTagAsync(string slug)
        {
            var tags = await this.GetTagsAsync();
            var tag = tags.SingleOrDefault(c => c.Slug.Equals(slug, StringComparison.CurrentCultureIgnoreCase));
            if (tag == null)
            {
                throw new FanException($"Tag with slug '{slug}' is not found.");
            }

            return tag;
        }

        /// <summary>
        /// Returns all tags, cached after calls to DAL.
        /// </summary>
        /// <returns></returns>
        public async Task<List<Tag>> GetTagsAsync()
        {
            return await _cache.GetAsync<List<Tag>>(CACHE_KEY_ALL_TAGS, new TimeSpan(0, 10, 0), async () => {
                return await _tagRepo.GetListAsync();
            });
        }

        /// <summary>
        /// Updates a <see cref="Tag"/> and invalidates cache for all tags.
        /// </summary>
        /// <param name="tag"></param>
        /// <returns></returns>
        public async Task<Tag> UpdateTagAsync(Tag tag)
        {
            tag = await PrepTaxonomyAsync(tag, ECreateOrUpdate.Update) as Tag;
            tag = await _tagRepo.UpdateAsync(tag);
            await _cache.RemoveAsync(CACHE_KEY_ALL_TAGS);

            return tag;
        }

        // -------------------------------------------------------------------- Media

        /// <summary>
        /// 
        /// </summary>
        /// <param name="userId"></param>
        /// <param name="name"></param>
        /// <param name="content"></param>
        /// <returns></returns>
        public async Task<string> UploadMediaAsync(int userId, string name, byte[] content)
        {
            // verify ext is supported
            string ext = Path.GetExtension(name);
            if (ext.IsNullOrEmpty() || !BlogConst.Accepted_Image_Types.Contains(ext, StringComparer.InvariantCultureIgnoreCase))
                throw new FanException("Upload image type is not supported.");

            // time
            var uploadedOn = DateTimeOffset.UtcNow;
            var year = uploadedOn.Year.ToString();
            var month = uploadedOn.Month.ToString("d2");

            // directory path to save this file in
            var dirPath = string.Format("{0}\\{1}\\{2}\\{3}",
                Path.Combine(_hostingEnvironment.WebRootPath),
                BlogConst.MEDIA_UPLOADS_FOLDER,
                year,
                month);

            if (!Directory.Exists(dirPath))
                Directory.CreateDirectory(dirPath);

            // file path
            string fileNameWithoutExt = Path.GetFileNameWithoutExtension(name);
            if (fileNameWithoutExt.Length > BlogConst.MEDIA_FILENAME_MAXLEN)
            {
                fileNameWithoutExt = fileNameWithoutExt.Substring(0, BlogConst.MEDIA_FILENAME_MAXLEN);
            }

            string slug = Util.FormatSlug(fileNameWithoutExt); // chinese fn ends up emtpy
            if (slug.IsNullOrEmpty())
            {
                slug = Util.RandomString(6);
            }

            string fileName = $"{slug}{ext}";
            var filePath = Path.Combine(dirPath, fileName); // C:\Fan.Web\wwwroot\uploads\2017\10\test-pic.jpg

            // user uploads file with an existing name, get a unique name
            // the problem is olw, if user resizes an image, be aware olw sends it as new file
            // also olw each time sends two copies of the file, orig and thumb
            int i = 2;
            while (File.Exists(filePath))
            {
                fileName = fileName.Insert(fileName.LastIndexOf('.'), $"-{i}");
                filePath = Path.Combine(dirPath, fileName);
            }

            // save file to file sys, always a new file
            using (var targetStream = File.Create(filePath))
            using (MemoryStream stream = new MemoryStream(content))
            {
                await stream.CopyToAsync(targetStream);
            }

            // save record to db
            var media = new Media
            {
                UserId = userId,
                FileName = fileName,
                Title = fileNameWithoutExt,
                Description = fileNameWithoutExt,
                Length = content.LongLength,
                Type = EMediaType.Image,
                UploadedOn = uploadedOn,
            };
            await _mediaRepo.CreateAsync(media);

            // a challenge here is that this returned url will be hardcoded into post
            // if user later switches to Blob Storage or CDN instead of file sys
            // all these post will break. If that happens, the easy remedy is keep 
            // existing copies of files where they were, not ideal.
            return $"{BlogConst.MEDIA_UPLOADS_FOLDER}/{year}/{month}/{fileName}";
        }

        public async Task<Media> UpdateMediaAsync(int id, string title, string description)
        {
            var media = await _mediaRepo.GetAsync(id);
            title = title.IsNullOrEmpty() ? "" : title;
            media.Title = title.Length > BlogConst.MEDIA_FILENAME_MAXLEN ?
             title.Substring(0, BlogConst.MEDIA_FILENAME_MAXLEN) : title;
            media.Description = description;
            return await _mediaRepo.UpdateAsync(media);
        }

        // -------------------------------------------------------------------- BlogPosts 

        /// <summary>
        /// Creates a <see cref="BlogPost"/> and invalidates cache for posts on index page.
        /// </summary>
        /// <param name="blogPost"></param>
        /// <returns></returns>
        public async Task<BlogPost> CreatePostAsync(BlogPost blogPost)
        {
            // blogPost is just used as a container of data
            if (blogPost == null) return blogPost;

            // preps BlogPost for saving and gets Post back
            var post = await PrepPostAsync(blogPost, ECreateOrUpdate.Create);

            // save post
            await _postRepo.CreateAsync(post);

            // invalidate cache only when published
            if (blogPost.Status == EPostStatus.Published)
            {
                await _cache.RemoveAsync(CACHE_KEY_POSTS_INDEX);
            }

            // get BlogPost again
            return await GetPostAsync(post.Id);
        }

        /// <summary>
        /// Updates a <see cref="BlogPost"/> and invalidates caceh for posts on index page.
        /// </summary>
        /// <param name="blogPost"></param>
        /// <returns></returns>
        public async Task<BlogPost> UpdatePostAsync(BlogPost blogPost)
        {
            // blogPost is just used as a container of data
            if (blogPost == null) return blogPost;

            var post = await PrepPostAsync(blogPost, ECreateOrUpdate.Update);

            // update
            await _postRepo.UpdateAsync(post);

            // invalidate cache 
            await _cache.RemoveAsync(CACHE_KEY_POSTS_INDEX);

            // return a new blogPost with latest data
            return await this.GetPostAsync(post.Id);
        }

        /// <summary>
        /// Deletes a blog post and invalidates cache for posts on index page.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task DeletePostAsync(int id)
        {
            await _postRepo.DeleteAsync(id);
            await _cache.RemoveAsync(CACHE_KEY_POSTS_INDEX);
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
        public async Task<BlogPost> GetPostAsync(string slug, int year, int month, int day)
        {
            // todo caching
            var post = await _postRepo.GetAsync(slug, year, month, day);

            return await GetBlogPostAsync(post);
        }

        /// <summary>
        /// Returns a list of blog posts for the blog index page.
        /// </summary>
        /// <param name="pageIndex"></param>
        /// <returns></returns>
        public async Task<BlogPostList> GetPostsAsync(int pageIndex)
        {
            PostListQuery query = new PostListQuery(EPostListQueryType.BlogPosts)
            {
                PageIndex = (pageIndex <= 0) ? 1 : pageIndex,
                PageSize = (await _settingSvc.GetSettingsAsync<BlogSettings>()).PageSize,
            };

            // cache only first page
            if (query.PageIndex == 1)
            {
                return await _cache.GetAsync(CACHE_KEY_POSTS_INDEX, new TimeSpan(0, 10, 0), async () =>
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
        public async Task<BlogPostList> GetPostsForCategoryAsync(string categorySlug, int pageIndex)
        {
            // todo caching
            PostListQuery query = new PostListQuery(EPostListQueryType.BlogPostsByCategory)
            {
                CategorySlug = categorySlug,
                PageIndex = (pageIndex <= 0) ? 1 : pageIndex,
                PageSize = (await _settingSvc.GetSettingsAsync<BlogSettings>()).PageSize,
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
            // todo caching
            PostListQuery query = new PostListQuery(EPostListQueryType.BlogPostsByTag)
            {
                TagSlug = tagSlug,
                PageIndex = (pageIndex <= 0) ? 1 : pageIndex,
                PageSize = (await _settingSvc.GetSettingsAsync<BlogSettings>()).PageSize,
            };

            return await QueryPostsAsync(query);
        }

        /// <summary>
        /// Returns a list of blog drafts.
        /// </summary>
        /// <returns></returns>
        public async Task<List<BlogPost>> GetPostsForDraftsAsync()
        {
            PostListQuery query = new PostListQuery(EPostListQueryType.BlogDrafts);

            return await QueryPostsAsync(query);
        }

        /// <summary>
        /// Returns specified number of <see cref="BlogPost"/> used by metaweblog.
        /// </summary>
        /// <param name="numberOfPosts">"All" is int.MaxValue</param>
        public async Task<List<BlogPost>> GetRecentPostsAsync(int numberOfPosts)
        {
            var query = new PostListQuery(EPostListQueryType.BlogPostsByNumber) { PageSize = numberOfPosts };

            return await QueryPostsAsync(query);
        }

        // -------------------------------------------------------------------- Private

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
            var results = await _postRepo.GetListAsync(query);

            var blogPostList = new BlogPostList(results.totalCount, query.PageSize);
            foreach (var post in results.posts)
            {
                blogPostList.Add(await GetBlogPostAsync(post));
            }

            return blogPostList;
        }

        /// <summary>
        /// Prepares a category or tag for create or update, making sure its title and slug are valid.
        /// </summary>
        /// <param name="tax">A category or tag.</param>
        /// <param name="createOrUpdate"></param>
        /// <returns></returns>
        private async Task<ITaxonomy> PrepTaxonomyAsync(ITaxonomy tax, ECreateOrUpdate createOrUpdate)
        {
            // get existing titles and slugs
            IEnumerable<string> existingTitles = null;
            IEnumerable<string> existingSlugs = null;
            ETaxonomyType type = ETaxonomyType.Category;
            if (tax is Category)
            {
                var allCats = await this.GetCategoriesAsync();
                existingTitles = allCats.Select(c => c.Title);
                existingSlugs = allCats.Select(c => c.Slug);
            }
            else
            {
                var allTags = await this.GetTagsAsync();
                existingTitles = allTags.Select(c => c.Title);
                existingSlugs = allTags.Select(c => c.Slug);
                type = ETaxonomyType.Tag;
            }

            // validator
            var validator = new TaxonomyValidator(existingTitles, type);
            ValidationResult result = await validator.ValidateAsync(tax);
            if (!result.IsValid)
            {
                throw new FanException($"Failed to {createOrUpdate.ToString().ToLower()} {type}.", result.Errors);
            }

            // Slug: user can create / update slug, we format the slug if it's available else we 
            // use title to get the slug.
            tax.Slug = BlogUtil.FormatTaxonomySlug(tax.Slug.IsNullOrEmpty() ? tax.Title : tax.Slug, existingSlugs);

            // html encode title
            tax.Title = WebUtility.HtmlEncode(tax.Title);

            _logger.LogDebug(createOrUpdate + " {@Taxonomy}", tax);
            return tax;
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
            var validator = new PostValidator();
            ValidationResult result = await validator.ValidateAsync(blogPost);
            if (!result.IsValid)
            {
                throw new FanException($"Failed to {createOrUpdate.ToString().ToLower()} blog post.", result.Errors);
            }

            // Get post
            // NOTE: can't use this.GetPostAsync(blogPost.Id) as it returns a BlogPost not a Post which would lose tracking
            var post = (createOrUpdate == ECreateOrUpdate.Create) ? new Post() : await QueryPostAsync(blogPost.Id, EPostType.BlogPost);
            var siteSettings = await _settingSvc.GetSettingsAsync<SiteSettings>();

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

            // UpdatedOn
            if (blogPost.Status == EPostStatus.Draft) post.UpdatedOn = post.CreatedOn;
            else post.UpdatedOn = null;

            // Slug before Title
            post.Slug = await GetBlogPostSlugAsync(blogPost.Slug.IsNullOrEmpty() ? blogPost.Title : blogPost.Slug,
                post.CreatedOn, createOrUpdate, blogPost.Id);
            post.Title = blogPost.Title; // looks like OLW html encodes post title

            // Body & Excerpt, UserId
            post.Body = blogPost.Body.IsNullOrWhiteSpace() ? null : blogPost.Body;
            post.Excerpt = blogPost.Excerpt.IsNullOrWhiteSpace() ? null : blogPost.Excerpt;
            post.UserId = blogPost.UserId;

            // Status & CommentStatus
            post.Status = blogPost.Status;
            post.CommentStatus = blogPost.CommentStatus;

            // Categories TODO check CategoryTitle first
            if (!string.IsNullOrEmpty(blogPost.CategoryTitle)) // CatTitle takes precedence if available
            {
                var cat = (await this.GetCategoriesAsync())
                    .SingleOrDefault(c => c.Title.Equals(blogPost.CategoryTitle, StringComparison.CurrentCultureIgnoreCase));
                if (cat == null)
                    post.Category = await this.CreateCategoryAsync(new Category { Title = blogPost.CategoryTitle });
                else
                    //post.Category = cat; // todo see if id works
                    post.CategoryId = cat.Id;
            }
            else if (blogPost.CategoryId.HasValue) // browser 
            {
                if (createOrUpdate == ECreateOrUpdate.Create)
                    post.CategoryId = blogPost.CategoryId.Value;
                else if (blogPost.CategoryId != post.CategoryId)
                    post.CategoryId = blogPost.CategoryId;
            }
            else
            {
                var blogSettings = await _settingSvc.GetSettingsAsync<BlogSettings>();
                post.CategoryId = blogSettings.DefaultCategoryId; // TODO test
            }

            // Tags & PosTags
            if (blogPost.TagTitles != null && blogPost.TagTitles.Count > 0)
            {
                // make sure list has no empty strings and only unique values
                blogPost.TagTitles = blogPost.TagTitles.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct().ToList();
                var allTags = await this.GetTagsAsync();

                if (createOrUpdate == ECreateOrUpdate.Create)
                {
                    foreach (var title in blogPost.TagTitles)
                    {
                        var tag = allTags.FirstOrDefault(t => t.Title.Equals(title, StringComparison.CurrentCultureIgnoreCase));
                        if (tag == null)
                            tag = await this.CreateTagAsync(new Tag { Title = title });

                        // NOTE: post.PostTags.Add(new PostTag { Post = post, Tag = tag }); 
                        // would fail with SqlServer, though during testing it worked with Sqlite In-Mem!!
                        // The reason is allTags are not tracked, you will receive the following exception
                        // SqlException: Cannot insert explicit value for identity column in table 'Tag' when IDENTITY_INSERT is set to OFF.
                        
                        //post.PostTags.Add(new PostTag { Post = post, Tag = tag });  // does not work with SqlServer
                        post.PostTags.Add(new PostTag { Post = post, TagId = tag.Id });
                    }
                }
                else
                {
                    var tagTitlesCurrent = post.PostTags.Select(pt => pt.Tag.Title).ToList();

                    var tagsToRemove = tagTitlesCurrent.Except(blogPost.TagTitles).ToList();
                    foreach (var title in tagsToRemove)
                    {
                        post.PostTags.Remove(post.PostTags.Single(pt => pt.Tag.Title == title));
                    }

                    var tagsToAdd = blogPost.TagTitles.Except(tagTitlesCurrent).ToList();
                    foreach (var title in tagsToAdd)
                    {
                        var tag = allTags.FirstOrDefault(t => t.Title.Equals(title, StringComparison.CurrentCultureIgnoreCase));
                        if (tag == null)
                            tag = await this.CreateTagAsync(new Tag { Title = title });

                        // here works because tag is just created so it is tracked
                        post.PostTags.Add(new PostTag { Post = post, Tag = tag });
                    }
                }
            }

            _logger.LogDebug(createOrUpdate + " {@Post}", post);
            return post;
        }

        /// <summary>
        /// Gets a <see cref="BlogPost"/> for display to client from a <see cref="Post"/>.
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        /// <remarks>
        /// It readies CreatedOnFriendly, Title, Excerpt, CategoryTitle, Tags and Body with shortcodes.
        /// </remarks>
        private async Task<BlogPost> GetBlogPostAsync(Post post)
        {
            var blogPost = _mapper.Map<Post, BlogPost>(post);
            var siteSettings = await _settingSvc.GetSettingsAsync<SiteSettings>();
            var blogSettings = await _settingSvc.GetSettingsAsync<BlogSettings>();

            // Friendly post time if the post was published within 2 days
            // else show the actual date time in setting's timezone
            blogPost.CreatedOnFriendly = (DateTimeOffset.UtcNow.Day - blogPost.CreatedOn.Day) > 2 ? 
                Util.ConvertTime(blogPost.CreatedOn, siteSettings.TimeZoneId).ToString("dddd, MMMM dd, yyyy") :
                blogPost.CreatedOn.Humanize();

            // Title
            blogPost.Title = WebUtility.HtmlDecode(blogPost.Title); // since OLW encodes it, we decode it here

            // Excerpt
            blogPost.Excerpt = post.Excerpt.IsNullOrEmpty() ? Util.GetExcerpt(post.Body, blogSettings.ExcerptWordLimit) : post.Excerpt;

            // CategoryTitle
            blogPost.CategoryTitle = post.Category.Title;

            // Tags and TagTitles
            foreach (var postTag in post.PostTags)
            {
                blogPost.Tags.Add(postTag.Tag);
                blogPost.TagTitles.Add(postTag.Tag.Title);
            }

            // Shortcodes
            blogPost.Body = _shortcodeSvc.Parse(post.Body);

            _logger.LogDebug("Show {@BlogPost}", blogPost);
            return blogPost;
        }

        /// <summary>
        /// Gets a unique and valid slug for a blog post.
        /// </summary>
        private async Task<string> GetBlogPostSlugAsync(string input, DateTimeOffset createdOn, ECreateOrUpdate createOrUpdate, int blogPostId) 
        {
            // when user manually inputted a slug, it could exceed max len
            if (input.Length > BlogConst.POST_TITLE_SLUG_MAXLEN)
            {
                input = input.Substring(0, BlogConst.POST_TITLE_SLUG_MAXLEN);
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
    }
}
