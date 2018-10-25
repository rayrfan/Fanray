using Fan.Exceptions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fan.Blog.Categories
{
    public interface ICategoryService
    {
        /// <summary>
        /// Returns a <see cref="Category"/> by id, throws FanException is category not found.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        /// <remarks>
        /// Admin site will call this by passing in the id of the category.
        /// </remarks>
        /// <exception cref="FanException">If category is not found.</exception>
        Task<Category> GetAsync(int id);
        /// <summary>
        /// Returns a <see cref="Category"/> by slug, throws FanException is category not found.
        /// </summary>
        /// <param name="slug"></param>
        /// <returns></returns>
        /// <remarks>
        /// Client site will call this by passing in the slug of the category.
        /// </remarks>
        /// <exception cref="FanException">If category is not found.</exception>
        Task<Category> GetAsync(string slug);
        /// <summary>
        /// Returns all categories.
        /// </summary>
        Task<List<Category>> GetAllAsync();
        /// <summary>
        /// Sets the id to default category.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task SetDefaultAsync(int id);
        /// <summary>
        /// Creates a <see cref="Category"/>, throws <see cref="FanException"/> if category title 
        /// fails validation or exists already.
        /// </summary>
        /// <returns>A category with id.</returns>
        /// <exception cref="FanException"></exception>
        Task<Category> CreateAsync(string title, string description = null);
        /// <summary>
        /// Updates a <see cref="Category"/>, throws <see cref="FanException"/> if category title 
        /// or slug fails validation or exists already.
        /// </summary>
        /// <param name="category"></param>
        /// <returns></returns>
        /// <exception cref="FanException"></exception>
        Task<Category> UpdateAsync(Category category);
        /// <summary>
        /// Deletes a <see cref="Category"/> by id and re-categorize its posts to the default category.
        /// </summary>
        /// <remarks>
        /// Admin console will call this by passing in an id of the category to be deleted.
        /// </remarks>
        /// <exception cref="FanException">If the id to be deleted is the default category.</exception>
        Task DeleteAsync(int id);
    }
}
