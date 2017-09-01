using AutoMapper;
using Fan.Data;
using Fan.Enums;
using Fan.Exceptions;
using Fan.Models;
using Fan.Validators;
using FluentValidation.Results;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Fan.Services
{
    public class BlogService : IBlogService
    {
        private readonly ICategoryRepository _catRepo;
        private readonly IMetaRepository _metaRepo;
        private readonly IPostRepository _postRepo;
        private readonly ITagRepository _tagRepo;
        private readonly IDistributedCache _cache;
        private readonly ILogger _logger;
        private readonly IMapper _mapper;

        public BlogService(ICategoryRepository catRepo,
            IMetaRepository metaRepo,
            IPostRepository postRepo,
            ITagRepository tagRepo,
            IDistributedCache cache,
            ILoggerFactory loggerFactory,
            IMapper mapper)
        {
            _catRepo = catRepo;
            _metaRepo = metaRepo;
            _postRepo = postRepo;
            _tagRepo = tagRepo;
            _cache = cache;
            _mapper = mapper;
            _logger = loggerFactory.CreateLogger<BlogService>();
        }

        public const string CACHE_KEY_ALL_CATS = "BlogCategories";
        public const string CACHE_KEY_ALL_TAGS = "BlogTags";
        public const string CACHE_KEY_BLOG_SETTINGS = "BlogSettings";
        public const string CACHE_KEY_POSTS_INDEX = "BlogPostsIndex";

        // -------------------------------------------------------------------- Settings

        /// <summary>
        /// Creates <see cref="BlogSettings"/>, if a BlogSettings already exists return it.
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public async Task<BlogSettings> CreateSettingsAsync(BlogSettings settings)
        {
            var meta = await _metaRepo.GetAsync("BlogSettings");
            if (meta != null) return settings;

            meta = new Meta
            {
                Key = "BlogSettings",
                Value = JsonConvert.SerializeObject(settings),
            };
            await _metaRepo.CreateAsync(meta);

            return settings;
        }

        /// <summary>
        /// Returns the <see cref="BlogSettings"/>, returns null if it does not exist.
        /// </summary>
        /// <remarks>
        /// A <see cref="BlogSettings"/> is created when the application runs for the first time.
        /// </remarks>
        public async Task<BlogSettings> GetSettingsAsync()
        {
            return await _cache.GetAsync<BlogSettings>(CACHE_KEY_BLOG_SETTINGS, new TimeSpan(0, 10, 0), async () =>
            {
                var meta = await _metaRepo.GetAsync("BlogSettings");

                return (meta == null) ? null : JsonConvert.DeserializeObject<BlogSettings>(meta.Value);
            });
        }

        /// <summary>
        /// Updates the <see cref="BlogSettings"/>, if BlogSettings does not exist, it will throw <see cref="FanException"/>.
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        public async Task<BlogSettings> UpdateSettingsAsync(BlogSettings settings)
        {
            var meta = await _metaRepo.GetAsync("BlogSettings");
            if (meta == null)
            {
                throw new FanException("BlogSettings cannot be updated because it does not exist in the system.");
            }

            meta.Value = JsonConvert.SerializeObject(settings);

            await _metaRepo.UpdateAsync(meta);
            await _cache.RemoveAsync(CACHE_KEY_BLOG_SETTINGS);

            return settings;
        }

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
            var blogSettings = await this.GetSettingsAsync();

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
            var cats = await this.GetCategoriesAsync();
            var cat = cats.SingleOrDefault(c => c.Id == id);
            if (cat == null)
            {
                throw new FanException("Category is not found.");
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
            var cats = await this.GetCategoriesAsync();
            var cat = cats.SingleOrDefault(c => c.Slug.Equals(slug, StringComparison.CurrentCultureIgnoreCase));
            if (cat == null)
            {
                throw new FanException("Category is not found.");
            }

            return cat;
        }

        /// <summary>
        /// Returns all categories, cached after calls to DAL.
        /// </summary>
        /// <returns></returns>
        public async Task<List<Category>> GetCategoriesAsync()
        {
            return await _cache.GetAsync<List<Category>>(CACHE_KEY_ALL_CATS, new TimeSpan(0, 10, 0), async () => {
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
            category = await this.PrepTaxonomyAsync(category, ECreateOrUpdate.Update) as Category;
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
            tag = await this.PrepTaxonomyAsync(tag, ECreateOrUpdate.Create) as Tag;
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
                throw new FanException("Tag is not found.");
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
                throw new FanException("Tag is not found.");
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
            tag = await this.PrepTaxonomyAsync(tag, ECreateOrUpdate.Update) as Tag;
            tag = await _tagRepo.UpdateAsync(tag);
            await _cache.RemoveAsync(CACHE_KEY_ALL_TAGS);

            return tag;
        }

        // -------------------------------------------------------------------- Media

        /// <summary>
        /// Creates or updates a media post.
        /// </summary>
        /// <param name="media"></param>
        /// <returns></returns>
        public async Task<Media> UpsertMediaAsync(Media media)
        {
            if (media == null) return media;

            var theMedia = await this.GetMediaAsync(media.Slug);

            if (theMedia != null)
            {
                theMedia.UserName = media.UserName;
                theMedia.Slug = media.Slug;
                theMedia.Title = media.Title;
                theMedia.MimeType = media.MimeType;
                theMedia.UpdatedOn = DateTime.UtcNow;

                var post = _mapper.Map<Media, Post>(theMedia);
                await _postRepo.UpdateAsync(post);
            }
            else
            {
                media.CreatedOn = DateTime.UtcNow;
                var post = _mapper.Map<Media, Post>(media);
                await _postRepo.CreateAsync(post);
            }

            return media;
        }

        /// <summary>
        /// Returns a media post by slug, returns null if not found.
        /// </summary>
        /// <param name="slug"></param>
        /// <returns></returns>
        public async Task<Media> GetMediaAsync(string slug)
        {
            var post = await _postRepo.GetAsync(slug, EPostType.Media);
            return _mapper.Map<Post, Media>(post);
        }

        // -------------------------------------------------------------------- Private

        /// <summary>
        /// Prepares a category or tag for create or update, making sure its title and slug are valid.
        /// </summary>
        /// <param name="tax">A category or tag.</param>
        /// <param name="createOrUpdate"></param>
        /// <returns></returns>
        private async Task<Taxonomy> PrepTaxonomyAsync(Taxonomy tax, ECreateOrUpdate createOrUpdate)
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
                var cou = createOrUpdate.ToString().ToLower();
                throw new FanException($"Failed to {cou} {type}.", result.Errors);
            }

            // Slug: user can create / update slug, we format the slug if it's available else we 
            // use title to get the slug.
            tax.Slug = BlogServiceHelper.FormatTaxonomySlug(tax.Slug.IsNullOrEmpty() ? tax.Title : tax.Slug, existingSlugs);

            // html encode title
            tax.Title = WebUtility.HtmlEncode(tax.Title);

            return tax;
        }
    }
}
