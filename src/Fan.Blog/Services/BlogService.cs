using AutoMapper;
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

        // -------------------------------------------------------------------- Categories

        /// <summary>
        /// Creates a <see cref="Category"/> and invalidates cache for all categories.
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public async Task<Category> CreateCategoryAsync(string title, string description = null)
        {
            Category category = new Category { Title = title, Description = description };
            category = await PrepTaxonomyAsync(category, ECreateOrUpdate.Create) as Category;
            category = await _catRepo.CreateAsync(category);

            await _cache.RemoveAsync(BlogCache.KEY_ALL_CATS);

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
            await _cache.RemoveAsync(BlogCache.KEY_ALL_CATS);
            await _cache.RemoveAsync(BlogCache.KEY_POSTS_INDEX);
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
                throw new FanException($"Category with id {id} is not found.");
            }

            return cat;
        }

        /// <summary>
        /// Returns category by slug, throws <see cref="FanException"/> if category with slug is null or not found.
        /// </summary>
        /// <param name="slug"></param>
        /// <returns></returns>
        public async Task<Category> GetCategoryAsync(string slug)
        {
            if (slug.IsNullOrEmpty()) throw new FanException("Category does not exist.");

            var cats = await GetCategoriesAsync();
            var cat = cats.SingleOrDefault(c => c.Slug.Equals(slug, StringComparison.CurrentCultureIgnoreCase));
            if (cat == null)
            {
                throw new FanException($"Category '{slug}' does not exist.");
            }

            return cat;
        }

        /// <summary>
        /// Returns all categories, cached after calls to DAL.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// This method must return all categories as <see cref="PrepPostAsync(BlogPost, ECreateOrUpdate)"/>
        /// depends on entire tags. If any filtering needs to be done for presentation purpose, then
        /// it must be done in presentation layer.
        /// </remarks>
        public async Task<List<Category>> GetCategoriesAsync()
        {
            return await _cache.GetAsync(BlogCache.KEY_ALL_CATS, BlogCache.Time_AllCats, async () => {
                return await _catRepo.GetListAsync();
            });
        }

        /// <summary>
        /// Sets the id to default category.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task SetDefaultCategoryAsync(int id)
        {
            await _settingSvc.UpsertSettingsAsync(new BlogSettings
            {
                DefaultCategoryId = id,
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
            await _catRepo.UpdateAsync(category);
            await _cache.RemoveAsync(BlogCache.KEY_ALL_CATS);
            await _cache.RemoveAsync(BlogCache.KEY_POSTS_INDEX);

            return category;
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
            List<string> existingTitles = null;
            List<string> existingSlugs = null;
            ETaxonomyType type = ETaxonomyType.Category;
            ITaxonomy origTax = tax;
            if (tax is Category cat)
            {
                if (cat.Id != 0) origTax = await _catRepo.GetAsync(cat.Id);
                var allCats = await GetCategoriesAsync();
                existingTitles = allCats.Select(c => c.Title).ToList();
                existingSlugs = allCats.Select(c => c.Slug).ToList();
            }
            else
            {
                //var tag = (Tag)tax;
                //if (tag.Id != 0) origTax = await _tagRepo.GetAsync(tag.Id);
                //var allTags = await GetTagsAsync();
                //existingTitles = allTags.Select(c => c.Title).ToList();
                //existingSlugs = allTags.Select(c => c.Slug).ToList();
                //type = ETaxonomyType.Tag;
            }

            // remove self if it is update
            if (createOrUpdate == ECreateOrUpdate.Update)
            {
                existingTitles.Remove(origTax.Title);
                existingSlugs.Remove(origTax.Slug);
            }

            // html encode title and description
            tax.Title = Util.CleanHtml(tax.Title);
            tax.Description = Util.CleanHtml(tax.Description);

            // validator
            var validator = new TaxonomyValidator(existingTitles);
            ValidationResult result = await validator.ValidateAsync(tax);
            if (!result.IsValid)
            {
                throw new FanException($"Failed to {createOrUpdate.ToString().ToLower()} {type}.", result.Errors);
            }

            // slug always updated according to title
            origTax.Slug = BlogUtil.FormatTaxonomySlug(tax.Title, existingSlugs);
            origTax.Title = tax.Title;
            origTax.Description = tax.Description;
            origTax.Count = tax.Count;

            _logger.LogDebug(createOrUpdate + " {@Taxonomy}", origTax);
            return origTax;
        }

        // -------------------------------------------------------------------- Images

        /// <summary>
        /// Deletes an image from data source and storage.
        /// </summary>
        /// <param name="mediaId"></param>
        /// <returns></returns>
        public async Task DeleteImageAsync(int mediaId)
        {
            var media = await _mediaSvc.GetMediaAsync(mediaId);
            var resizes = GetImageResizeList(media.UploadedOn);
            var resizeCount = media.ResizeCount; // how many files to delete

            // delete file from storage
            await DeleteImageFileAsync(media, EImageSize.Original);
            if (resizeCount == 3)
            {
                await DeleteImageFileAsync(media, EImageSize.Small);
                await DeleteImageFileAsync(media, EImageSize.Medium);
                await DeleteImageFileAsync(media, EImageSize.Large);
            }
            else if (resizeCount == 2)
            {
                await DeleteImageFileAsync(media, EImageSize.Small);
                await DeleteImageFileAsync(media, EImageSize.Medium);
            }
            else if (resizeCount == 1)
            {
                await DeleteImageFileAsync(media, EImageSize.Small);
            }

            // delete from db
            await _mediaSvc.DeleteMediaAsync(mediaId);
        }

        private async Task DeleteImageFileAsync(Media media, EImageSize size)
        {
            var path = GetImagePath(media.UploadedOn, size);
            await _storageProvider.DeleteFileAsync(media.FileName, path, IMAGE_PATH_SEPARATOR);
        }

        /// <summary>
        /// Returns absolute url to an image.
        /// </summary>
        /// <remarks>
        /// Based on the resize count, the url returned could be original or one of the resized image.
        /// </remarks>
        /// <param name="media">The media record representing the image.</param>
        /// <param name="size">The image size.</param>
        /// <returns></returns>
        public string GetImageUrl(Media media, EImageSize size)
        {
            var endpoint = _storageProvider.StorageEndpoint;
            var container = endpoint.EndsWith('/') ? _appSettings.MediaContainerName : $"/{_appSettings.MediaContainerName}";

            if ((size == EImageSize.Original || media.ResizeCount <= 0) ||
                (media.ResizeCount == 1 && size != EImageSize.Small) ||
                (media.ResizeCount == 2 && size == EImageSize.Large))
            {
                size = EImageSize.Original;
            }

            var imagePath = GetImagePath(media.UploadedOn, size);
            var fileName = media.FileName;

            return $"{endpoint}{container}/{imagePath}/{fileName}";
        }

        /// <summary>
        /// Uploads image.
        /// </summary>
        /// <param name="source"></param>
        /// <param name="userId"></param>
        /// <param name="fileName"></param>
        /// <param name="contentType">e.g. "image/jpeg"</param>
        /// <param name="uploadFrom"></param>
        /// <returns></returns>
        public async Task<Media> UploadImageAsync(Stream source, int userId, string fileName, string contentType,
            EUploadedFrom uploadFrom)
        {
            // check if file type is supported
            var ext = Path.GetExtension(fileName).ToLower();
            var ctype = "." + contentType.Substring(contentType.LastIndexOf("/") + 1).ToLower();
            if (ext.IsNullOrEmpty() || !Accepted_Image_Types.Contains(ext) || !Accepted_Image_Types.Contains(ctype))
            {
                throw new NotSupportedException("Upload file type is not supported.");
            }

            // uploadedOn 
            var uploadedOn = DateTimeOffset.UtcNow;

            // get the slugged filename and title from original filename
            var (fileNameSlugged, title) = ProcessFileName(fileName, uploadFrom);

            // get unique filename
            var uniqueFileName = await GetUniqueFileNameAsync(fileNameSlugged, uploadedOn);

            // get image resizes
            var resizes = GetImageResizeList(uploadedOn);

            return await _mediaSvc.UploadImageAsync(source, resizes, uniqueFileName, contentType, title,
                uploadedOn, EAppType.Blog, userId, uploadFrom);
        }

        /// <summary>
        /// Takes the original filename and returns a slugged filename and title attribute.
        /// </summary>
        /// <remarks>
        /// If the filename is too long it shorten it. Then it generates a slugged filename which 
        /// is hyphen separeated value for english original filenames, a random string value for 
        /// non-english filenames.  The title attribute is original filename html-encoded for safe
        /// display.
        /// </remarks>
        /// <param name="fileNameOrig">Original filename user is uploading.</param>
        /// <param name="uploadFrom">This is used solely because of olw quirks I have to handle.</param>
        /// <returns></returns>
        private (string fileNameSlugged, string title) ProcessFileName(string fileNameOrig, EUploadedFrom uploadFrom)
        {
            // extra filename without ext, note this will also remove the extra path info from OLW
            var fileNameWithoutExt = Path.GetFileNameWithoutExtension(fileNameOrig);

            // make sure file name is not too long
            if (fileNameWithoutExt.Length > MediaService.MEDIA_FILENAME_MAXLEN)
            {
                fileNameWithoutExt = fileNameWithoutExt.Substring(0, MediaService.MEDIA_FILENAME_MAXLEN);
            }

            // there is a quirk file uploaded from olw had "_2" suffixed to the name
            if (uploadFrom == EUploadedFrom.MetaWeblog && fileNameWithoutExt.EndsWith("_2"))
            {
                fileNameWithoutExt = fileNameWithoutExt.Remove(fileNameWithoutExt.Length - 2);
            }

            // slug file name
            var slug = Util.FormatSlug(fileNameWithoutExt);
            if (slug.IsNullOrEmpty()) // slug may end up empty
            {
                slug = Util.RandomString(6);
            }
            else if (uploadFrom == EUploadedFrom.MetaWeblog && slug == "thumb") // or may end up with only "thumb" for olw
            {
                slug = string.Concat(Util.RandomString(6), "_thumb");
            }

            var ext = Path.GetExtension(fileNameOrig).ToLower();
            var fileNameSlugged = $"{slug}{ext}";
            var fileNameEncoded = WebUtility.HtmlEncode(fileNameWithoutExt);

            return (fileNameSlugged: fileNameSlugged, title: fileNameEncoded);
        }

        /// <summary>
        /// Returns a unique filename after checking datasource to see if the filename exists already.
        /// </summary>
        /// <param name="uploadedOn"></param>
        /// <param name="fileNameSlugged"></param>
        /// <returns></returns>
        private async Task<string> GetUniqueFileNameAsync(string fileNameSlugged, DateTimeOffset uploadedOn)
        {
            int i = 1;
            while (await _mediaSvc.ExistsAsync(m => m.AppType == EAppType.Blog &&
                                                    m.UploadedOn.Year == uploadedOn.Year &&
                                                    m.UploadedOn.Month == uploadedOn.Month &&
                                                    m.FileName.Equals(fileNameSlugged)))
            {
                var lookUp = ".";
                var replace = $"-{i}.";
                if (i > 1)
                {
                    int j = i - 1;
                    lookUp = $"-{j}.";
                }

                fileNameSlugged = fileNameSlugged.Replace(lookUp, replace);
                i++;
            }

            return fileNameSlugged;
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
            await _mediator.Publish(new BlogPostBeforeCreate { TagTitles = blogPost.TagTitles });

            // create
            await _postRepo.CreateAsync(post, blogPost.TagTitles);

            // invalidate cache only when published
            if (blogPost.Status == EPostStatus.Published)
            {
                await BlogCache.RemoveAllBlogCacheAsync(_cache);
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
                TagTitles = blogPost.TagTitles,
                CurrentPost = await QueryPostAsync(blogPost.Id, EPostType.BlogPost),
            });

            // update
            await _postRepo.UpdateAsync(post, blogPost.TagTitles);

            // invalidate cache 
            await BlogCache.RemoveAllBlogCacheAsync(_cache);

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
            await BlogCache.RemoveAllBlogCacheAsync(_cache);
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

            // Categories TODO check CategoryTitle first
            if (!string.IsNullOrEmpty(blogPost.CategoryTitle)) // CatTitle takes precedence if available
            {
                var cat = (await GetCategoriesAsync())
                    .SingleOrDefault(c => c.Title.Equals(blogPost.CategoryTitle, StringComparison.CurrentCultureIgnoreCase));
                if (cat == null)
                    post.Category = await CreateCategoryAsync(blogPost.CategoryTitle);
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

        //private async Task InvalidateAllBlogCache()
        //{
        //    await _cache.RemoveAsync(CACHE_KEY_POSTS_INDEX);
        //    await _cache.RemoveAsync(CACHE_KEY_ALL_CATS);
        //    await _cache.RemoveAsync(CACHE_KEY_ALL_TAGS);
        //    await _cache.RemoveAsync(CACHE_KEY_ALL_ARCHIVES);
        //    await _cache.RemoveAsync(CACHE_KEY_POST_COUNT);
        //}
    }
}
