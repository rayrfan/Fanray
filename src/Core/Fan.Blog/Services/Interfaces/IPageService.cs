using Fan.Blog.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fan.Blog.Services.Interfaces
{
    public interface IPageService
    {
        /// <summary>
        /// Creates a <see cref="Page"/>.
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        Task<Page> CreatePageAsync(Page page);
        /// <summary>
        /// Updates a <see cref="Page"/>.
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        Task<Page> UpdatePageAsync(Page page);
        /// <summary>
        /// Deletes a <see cref="Page"/>
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task DeletePageAsync(int id);
        /// <summary>
        /// Returns a <see cref="Page"/> by id.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Page> GetPageAsync(int id);
        /// <summary>
        /// Returns a page by slugs.
        /// </summary>
        /// <param name="slugs"></param>
        /// <returns></returns>
        Task<Page> GetPageAsync(params string[] slugs);
        /// <summary>
        /// Returns a root page with child pages.
        /// </summary>
        /// <param name="slug"></param>
        /// <returns></returns>
        Task<Page> GetRootPageWithChildrenAsync(string slug);
        /// <summary>
        /// Returns a parent page with its children.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task<Page> GetParentPageWithChildrenAsync(int id);
        /// <summary>
        /// Returns all root pages.
        /// </summary>
        /// <returns></returns>
        Task<List<Page>> GetRootPagesAsync();
        /// <summary>
        /// Returns specified number of <see cref="Page"/> requested by metaweblog api.
        /// </summary>
        /// <param name="numberOfPages"></param>
        /// <returns></returns>
        Task<List<Page>> GetRecentPagesAsync(int nubmerOfPages);
    }
}
