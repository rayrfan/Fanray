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
        /// Creates a <see cref="Category"/>.
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        public async Task<Category> CreateCategoryAsync(Category category)
        {
            category = await this.PrepCategoryAsync(category, ECreateOrUpdate.Create);
            category = await _catRepo.CreateAsync(category);
            await _cache.RemoveAsync(CACHE_KEY_ALL_CATS); // TODO what about posts
            return category;
        }

        /// <summary>
        /// Deletes a <see cref="Category"/>.
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
                throw new FanException("Default category cannot be deleted.");

            await _catRepo.DeleteAsync(id, blogSettings.DefaultCategoryId);
            await _cache.RemoveAsync(CACHE_KEY_ALL_CATS);
        }

        public async Task<Category> GetCategoryAsync(int id)
        {
            var cats = await this.GetCategoriesAsync();
            var cat = cats.SingleOrDefault(c => c.Id == id);
            if (cat == null)
                throw new FanException("Category is not found.");
            return cat;
        }

        public async Task<Category> GetCategoryAsync(string slug)
        {
            var cats = await this.GetCategoriesAsync();
            var cat = cats.SingleOrDefault(c => c.Slug.Equals(slug, StringComparison.CurrentCultureIgnoreCase));
            if (cat == null)
                throw new FanException("Category is not found.");
            return cat;
        }

        public async Task<List<Category>> GetCategoriesAsync()
        {
            return await _cache.GetAsync<List<Category>>(CACHE_KEY_ALL_CATS, new TimeSpan(0, 10, 0), async () => {
                return await _catRepo.GetListAsync();
            });
        }

        public async Task<Category> UpdateCategoryAsync(Category category)
        {
            category = await this.PrepCategoryAsync(category, ECreateOrUpdate.Update);
            category = await _catRepo.UpdateAsync(category);
            await _cache.RemoveAsync(CACHE_KEY_ALL_CATS);
            return category;
        }

        // -------------------------------------------------------------------- Private

        /// <summary>
        /// Prepares a category for Create or Update, making sure its title and slug are valid.
        /// </summary>
        private async Task<Category> PrepCategoryAsync(Category category, ECreateOrUpdate createOrUpdate)
        {
            // get all categories
            var allCats = await this.GetCategoriesAsync();

            // Title: validator makes sure it cannot be empty, or exceed max length or already exist
            var existingTitles = allCats.Select(c => c.Title);
            var validator = new TaxonomyValidator(existingTitles, ETaxonomyType.Category);
            ValidationResult result = await validator.ValidateAsync(category);
            if (!result.IsValid)
            {
                var cou = createOrUpdate.ToString().ToLower();
                throw new FanException($"Failed to {cou} category.", result.Errors);
            }

            // Slug: user can create / update slug, we format the slug if it's available else we 
            // use title to get the slug.
            var existingSlugs = allCats.Select(c => c.Slug);
            category.Slug = BlogServiceHelper.FormatTaxonomySlug(category.Slug.IsNullOrEmpty() ? category.Title : category.Slug, existingSlugs);

            // html encode title
            category.Title = WebUtility.HtmlEncode(category.Title);

            return category;
        }

      
    }
}
