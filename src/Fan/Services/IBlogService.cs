using Fan.Exceptions;
using Fan.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fan.Services
{
    public interface IBlogService
    {
        // -------------------------------------------------------------------- Settings

        /// <summary>
        /// Creates a <see cref="BlogSettings"/>.
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        Task<BlogSettings> CreateSettingsAsync(BlogSettings settings);
        /// <summary>
        /// Returns the <see cref="BlogSettings"/>, returns null if it does not exist.
        /// </summary>
        /// <remarks>
        /// A <see cref="BlogSettings"/> is created when the application runs for the first time.
        /// </remarks>
        Task<BlogSettings> GetSettingsAsync();
        /// <summary>
        /// Updates the <see cref="BlogSettings"/>, if BlogSettings does not exist, it will throw <see cref="FanException"/>.
        /// </summary>
        /// <param name="settings"></param>
        /// <returns></returns>
        Task<BlogSettings> UpdateSettingsAsync(BlogSettings settings);

        // -------------------------------------------------------------------- Categories

        /// <summary>
        /// Creates a <see cref="Category"/>, throws <see cref="FanException"/> if category title 
        /// or slug fails validation or exists already.
        /// </summary>
        /// <returns>A category with id.</returns>
        /// <exception cref="FanException"></exception>
        Task<Category> CreateCategoryAsync(Category category);
        /// <summary>
        /// Deletes a <see cref="Category"/> by id and re-categorize its posts to the default category.
        /// </summary>
        /// <remarks>
        /// Admin console will call this by passing in an id of the category to be deleted.
        /// </remarks>
        /// <exception cref="FanException">If the id to be deleted is the default category.</exception>
        Task DeleteCategoryAsync(int id);
        /// <summary>
        /// Returns a <see cref="Category"/> by id, throws FanException is category not found.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <remarks>
        /// Admin site will call this by passing in the id of the category.
        /// </remarks>
        /// <exception cref="FanException">If category is not found.</exception>
        Task<Category> GetCategoryAsync(int id);
        /// <summary>
        /// Returns a <see cref="Category"/> by slug, throws FanException is category not found.
        /// </summary>
        /// <param name="slug"></param>
        /// <returns></returns>
        /// <remarks>
        /// Client site will call this by passing in the slug of the category.
        /// </remarks>
        /// <exception cref="FanException">If category is not found.</exception>
        Task<Category> GetCategoryAsync(string slug);
        /// <summary>
        /// Returns all categories.
        /// </summary>
        Task<List<Category>> GetCategoriesAsync();
        /// <summary>
        /// Updates a <see cref="Category"/>, throws <see cref="FanException"/> if category title 
        /// or slug fails validation or exists already.
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        /// <exception cref="FanException"></exception>
        Task<Category> UpdateCategoryAsync(Category category);

        // -------------------------------------------------------------------- Tags

        Task<Tag> CreateTagAsync(Tag tag);
        Task DeleteTagAsync(int id);
        Task<Tag> GetTagAsync(int id);
        Task<Tag> GetTagAsync(string slug);
        Task<List<Tag>> GetTagsAsync();
        Task<Tag> UpdateTagAsync(Tag tag);

        // -------------------------------------------------------------------- Media

        Task<Media> UpsertMediaAsync(Media media);
        Task<Media> GetMediaAsync(string fileName);
    }
}