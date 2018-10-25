using Fan.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fan.Blog.Categories
{
    /// <summary>
    /// Contract for a category repository.
    /// </summary>
    public interface ICategoryRepository : IRepository<Category>
    {
        /// <summary>
        /// Deletes a <see cref="Category"/> by id and re-categorize its posts to the given 
        /// default category id.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="defaultCategoryId">
        /// This is the BlogSettings DefaultCategoryId, I choose to have it pass in
        /// from BLL for convenience instead of querying Meta for it.
        /// </param>
        Task DeleteAsync(int id, int defaultCategoryId);

        /// <summary>
        /// Returns a list of <see cref="Category"/>, the returned objects are not tracked.
        /// The returned list are order by alphabetically on <see cref="Category.Title"/>.
        /// </summary>
        Task<List<Category>> GetListAsync();
    }
}
