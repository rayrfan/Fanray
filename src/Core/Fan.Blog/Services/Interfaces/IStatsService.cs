using Fan.Blog.Enums;
using Fan.Blog.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fan.Blog.Services.Interfaces
{
    public interface IStatsService
    {
        /// <summary>
        /// Returns a dictionary of year and months, the key is year and the value is a list of 
        /// <see cref="MonthItem"/> objects.
        /// </summary>
        /// <returns></returns>
        Task<Dictionary<int, List<MonthItem>>> GetArchivesAsync();

        /// <summary>
        /// Returns total number of posts by each <see cref="EPostStatus"/>.
        /// </summary>
        /// <returns></returns>
        Task<PostCount> GetPostCountAsync();

        /// <summary>
        /// Increases post view count.
        /// </summary>
        Task IncViewCountAsync(EPostType postType, int postId);
    }
}
