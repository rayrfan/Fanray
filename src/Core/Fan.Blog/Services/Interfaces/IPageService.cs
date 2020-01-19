using Fan.Blog.Enums;
using Fan.Blog.Models;
using Fan.Exceptions;
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
        Task<Page> CreateAsync(Page page);

        /// <summary>
        /// Updates a <see cref="Page"/>.
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        Task<Page> UpdateAsync(Page page);

        /// <summary>
        /// Deletes a <see cref="Page"/>, if a parent has children they will be deleted as well.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        Task DeleteAsync(int id);

        /// <summary>
        /// Returns a page by <paramref name="id"/>. If the page is a parent its children will also
        /// be returned if any, if the page is a child its siblings will also be returned if any.
        /// </summary>
        /// <param name="id">The id of the page.</param>
        /// <returns>
        /// A <see cref="Page"/> for composer to edit.
        /// </returns>
        /// <exception cref="FanException">
        /// Thrown if page by <paramref name="id"/> is not found.
        /// </exception>
        Task<Page> GetAsync(int id);

        /// <summary>
        /// Returns a page by <paramref name="slugs"/>. If the page is a parent its children will be returned with it.
        /// </summary>
        /// <param name="isPreview">True if page is being retrieved for previewing.</param>
        /// <param name="slugs">The slugs that lead to the page.</param>
        /// <returns>
        /// A <see cref="Page"/> for public viewing.
        /// </returns>
        /// <exception cref="FanException">
        /// Thrown if page by <paramref name="slugs"/> is not found or the page is a <see cref="EPostStatus.Draft"/>
        /// or its parent is a <see cref="EPostStatus.Draft"/>.
        /// </exception>
        Task<Page> GetAsync(bool isPreview, params string[] slugs);

        /// <summary>
        /// Returns all parent pages, when <paramref name="withChildren"/> is true their children are also returned.
        /// </summary>
        /// <param name="withChildren">True will return children with the parents.</param>
        /// <returns></returns>
        Task<IList<Page>> GetParentsAsync(bool withChildren = false);

        /// <summary>
        /// Saves a parent page's navigation.
        /// </summary>
        /// <param name="pageId">The parent page id.</param>
        /// <param name="navMd">The navigation markdown.</param>
        /// <returns></returns>
        Task SaveNavAsync(int pageId, string navMd);
    }
}
