using Fan.Blog.Data;
using Fan.Blog.Events;
using Fan.Blog.Helpers;
using Fan.Blog.Models;
using Fan.Blog.Services.Interfaces;
using Fan.Exceptions;
using Fan.Helpers;
using Fan.Settings;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Fan.Blog.Services
{
    /// <summary>
    /// The blog category service.
    /// </summary>
    public class CategoryService : ICategoryService,
                                   INotificationHandler<BlogPostBeforeCreate>,
                                   INotificationHandler<BlogPostBeforeUpdate>
    {
        private readonly ICategoryRepository _catRepo;
        private readonly ISettingService _settingSvc;
        private readonly IMediator _mediator;
        private readonly IDistributedCache _cache;
        private readonly ILogger<CategoryService> _logger;

        public CategoryService(ICategoryRepository catRepo,
                               ISettingService settingService,
                               IMediator mediator,
                               IDistributedCache cache,
                               ILogger<CategoryService> logger)
        {
            _catRepo = catRepo;
            _settingSvc = settingService;
            _mediator = mediator;
            _cache = cache;
            _logger = logger;
        }

        // -------------------------------------------------------------------- const

        /// <summary>
        /// The max allowed length of a category title is 24 chars.
        /// </summary>
        public const int TITLE_MAXLEN = 24;

        /// <summary>
        /// The max allowed length of a category slug is 24 chars.
        /// </summary>
        public const int SLUG_MAXLEN = 24;

        // -------------------------------------------------------------------- public methods

        /// <summary>
        /// Returns category by id, throws <see cref="FanException"/> if category with id is not found.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task<Category> GetAsync(int id)
        {
            var cats = await GetAllAsync();
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
        public async Task<Category> GetAsync(string slug)
        {
            if (slug.IsNullOrEmpty()) throw new FanException("Category does not exist.");

            var cats = await GetAllAsync();
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
        public async Task<List<Category>> GetAllAsync()
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
        public async Task SetDefaultAsync(int id)
        {
            await _settingSvc.UpsertSettingsAsync(new BlogSettings
            {
                DefaultCategoryId = id,
            });
        }

        /// <summary>
        /// Creates a new <see cref="Category"/>.
        /// </summary>
        /// <param name="category">The category with data to be created.</param>
        /// <exception cref="FanException">If title is empty or exists already.</exception>
        /// <returns>Created category.</returns>
        public async Task<Category> CreateAsync(string title, string description = null)
        {
            if (title.IsNullOrEmpty())
            {
                throw new FanException($"Category title cannot be empty.");
            }

            // prep title
            title = PrepareTitle(title);

            // make sure unique
            var allCats = await GetAllAsync();
            if (allCats.Any(t => t.Title.Equals(title, StringComparison.CurrentCultureIgnoreCase)))
            {
                throw new FanException($"'{title}' already exists.");
            }

            // prep slug, desc and count
            var category = new Category
            {
                Title = title,
                Slug = BlogUtil.SlugifyTaxonomy(title, SLUG_MAXLEN, allCats.Select(c => c.Slug)),
                Description = Util.CleanHtml(description),
                Count = 0,
            };

            // create
            category = await _catRepo.CreateAsync(category);

            // remove cache
            await _cache.RemoveAsync(BlogCache.KEY_ALL_CATS);
            await _cache.RemoveAsync(BlogCache.KEY_POSTS_INDEX);

            _logger.LogDebug("Created {@Category}", category);
            return category;
        }

        /// <summary>
        /// Updates an existing <see cref="Category"/>.
        /// </summary>
        /// <param name="category">The category with data to be updated.</param>
        /// <exception cref="FanException">If category is invalid or title exists.</exception>
        /// <returns>Updated category.</returns>
        public async Task<Category> UpdateAsync(Category category)
        {
            if (category == null || category.Id <= 0 || category.Title.IsNullOrEmpty())
            {
                throw new FanException($"Invalid category to update.");
            }

            // prep title
            category.Title = PrepareTitle(category.Title);

            // make sure it is unique
            var allCats = await GetAllAsync();
            allCats.RemoveAll(c => c.Id == category.Id); // remove selft
            if (allCats.Any(c => c.Title.Equals(category.Title, StringComparison.CurrentCultureIgnoreCase)))
            {
                throw new FanException($"'{category.Title}' already exists.");
            }

            // prep slug, description and count
            var entity = await _catRepo.GetAsync(category.Id);
            entity.Title = category.Title; // assign new title
            entity.Slug = BlogUtil.SlugifyTaxonomy(category.Title, SLUG_MAXLEN, allCats.Select(c => c.Slug)); // slug is based on title
            entity.Description = Util.CleanHtml(category.Description);
            entity.Count = category.Count;

            // update
            await _catRepo.UpdateAsync(category);

            // remove cache
            await _cache.RemoveAsync(BlogCache.KEY_ALL_CATS);
            await _cache.RemoveAsync(BlogCache.KEY_POSTS_INDEX);

            // return entity
            _logger.LogDebug("Updated {@Category}", entity);
            return entity;
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
        public async Task DeleteAsync(int id)
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

        // -------------------------------------------------------------------- event handlers

        /// <summary>
        /// Handles the <see cref="BlogPostBeforeCreate"/> event by creating a new category 
        /// if not already exists.
        /// </summary>
        /// <param name="notification"></param>
        /// <param name="cancellationToken"></param>
        /// <remarks>
        /// This only happens when calling from metaweblog with a new category.
        /// </remarks>
        public async Task Handle(BlogPostBeforeCreate notification, CancellationToken cancellationToken)
        {
            await HandleNewCatAsync(notification.CategoryTitle);
        }

        /// <summary>
        /// Handles the <see cref="BlogPostBeforeUpdate"/> event by creating a new category
        /// if not already exisits.
        /// </summary>
        /// <param name="notification"></param>
        /// <param name="cancellationToken"></param>
        /// <remarks>
        /// This only happens when calling from metaweblog with creating a new category.
        /// </remarks>
        public async Task Handle(BlogPostBeforeUpdate notification, CancellationToken cancellationToken)
        {
            await HandleNewCatAsync(notification.CategoryTitle);
        }

        /// <summary>
        /// Create a new category with the given title if category not exist already.
        /// </summary>
        /// <param name="categoryTitle"></param>
        /// <returns></returns>
        private async Task HandleNewCatAsync(string categoryTitle)
        {
            if (categoryTitle.IsNullOrEmpty()) return;

            // lookup
            var cat = (await GetAllAsync())
                   .SingleOrDefault(c => c.Title.Equals(categoryTitle, StringComparison.CurrentCultureIgnoreCase));

            // create if not exist
            if (cat == null)
                await CreateAsync(categoryTitle);
        }

        // -------------------------------------------------------------------- private methods

        /// <summary>
        /// Cleans category title from any html and shortens it if exceed max allow length.
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        private string PrepareTitle(string title)
        {
            title = Util.CleanHtml(title);
            title = title.Length > TITLE_MAXLEN ? title.Substring(0, TITLE_MAXLEN) : title;
            return title;
        }
    }
}
