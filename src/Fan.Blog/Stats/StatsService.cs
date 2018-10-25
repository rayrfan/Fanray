using Fan.Blog.Data;
using Fan.Blog.Helpers;
using Microsoft.Extensions.Caching.Distributed;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fan.Blog.Stats
{
    public class StatsService : IStatsService
    {
        private readonly IPostRepository _postRepo;
        private readonly IDistributedCache _cache;

        public StatsService(
            IPostRepository postRepo,
            IDistributedCache cache
            )
        {
            _postRepo = postRepo;
            _cache = cache;
        }

        // -------------------------------------------------------------------- public methods

        /// <summary>
        /// Returns archive information.
        /// </summary>
        /// <returns></returns>
        public async Task<Dictionary<int, List<MonthItem>>> GetArchivesAsync()
        {
            return await _cache.GetAsync(BlogCache.KEY_ALL_ARCHIVES, BlogCache.Time_Archives, async () =>
            {
                var months = new Dictionary<DateTime, int>();
                var years = new Dictionary<int, List<MonthItem>>();

                var dates = await _postRepo.GetPostDateTimesAsync();
                foreach (var month in dates)
                {
                    months.TryGetValue(month, out int count);
                    ++count;
                    months[month] = count;
                }

                foreach (var month in months)
                {
                    int year = month.Key.Year;
                    if (!years.Keys.Contains(year))
                    {
                        years.Add(year, new List<MonthItem>());
                    }

                    years[year].Add(new MonthItem
                    {
                        Title = month.Key.ToString("MMMM"),
                        Url = BlogRoutes.GetArchiveRelativeLink(year, month.Key.Month),
                        Count = month.Value,
                    });
                }

                return years;
            });
        }

        /// <summary>
        /// Returns total number of posts by each <see cref="EPostStatus"/>.
        /// </summary>
        /// <returns></returns>
        public async Task<PostCount> GetPostCountAsync()
        {
            return await _cache.GetAsync(BlogCache.KEY_POST_COUNT, BlogCache.Time_PostCount, async () =>
            {
                return await _postRepo.GetPostCountAsync();
            });
        }
    }
}
