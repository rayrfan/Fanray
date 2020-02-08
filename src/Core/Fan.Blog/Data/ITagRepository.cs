using Fan.Blog.Models;
using Fan.Data;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fan.Blog.Data
{
    /// <summary>
    /// Contract for a tag repository.
    /// </summary>
    public interface ITagRepository : IRepository<Tag>
    {
        /// <summary>
        /// Returns all tags or empty list if no tags found. The returned list is ordered by 
        /// <see cref="Tag.Count"/> desc.
        /// </summary>
        Task<List<Tag>> GetListAsync();
    }
}