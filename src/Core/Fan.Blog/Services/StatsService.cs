using Fan.Blog.Data;
using Fan.Blog.Enums;
using Fan.Blog.Helpers;
using Fan.Blog.Models;
using Fan.Blog.Services.Interfaces;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fan.Blog.Services
{
    public class StatsService : IStatsService
    {
        private readonly IPostRepository postRepository;
        private readonly IDistributedCache distributedCache;
        private readonly IMemoryCache memeoryCache;
        private readonly HttpContext context;

        public StatsService(IHttpContextAccessor contextAccessor,
            IPostRepository postRepository,
            IDistributedCache distributedCache,
            IMemoryCache memeoryCache)
        {
            this.context = contextAccessor.HttpContext;
            this.postRepository = postRepository;
            this.distributedCache = distributedCache;
            this.memeoryCache = memeoryCache;
        }

        /// <summary>
        /// Returns archive information.
        /// </summary>
        /// <returns></returns>
        public async Task<Dictionary<int, List<MonthItem>>> GetArchivesAsync()
        {
            return await distributedCache.GetAsync(BlogCache.KEY_ALL_ARCHIVES, BlogCache.Time_Archives, async () =>
            {
                var months = new Dictionary<DateTime, int>();
                var years = new Dictionary<int, List<MonthItem>>();

                var dates = await postRepository.GetPostDateTimesAsync();
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
            return await distributedCache.GetAsync(BlogCache.KEY_POST_COUNT, BlogCache.Time_PostCount, async () =>
            {
                return await postRepository.GetPostCountAsync();
            });
        }

        /// <summary>
        /// Increases post view count.
        /// </summary>
        /// <param name="postType"></param>
        /// <param name="postId"></param>
        /// <returns></returns>
        public async Task IncViewCountAsync(EPostType postType, int postId)
        {
            var cacheKey = postType == EPostType.BlogPost ?
                string.Format(BlogCache.KEY_POST_VIEW_COUNT, postId) :
                string.Format(BlogCache.KEY_PAGE_VIEW_COUNT, postId);
            var ip = (context == null || context.Connection.RemoteIpAddress == null) ? 
                "::1" : // ip is null when running tests
                context.Connection.RemoteIpAddress.ToString();

            var ipList = memeoryCache.Get<IList<string>>(cacheKey);
            if (ipList == null) 
            {
                ipList = new List<string> { ip };
                memeoryCache.Set(cacheKey, ipList, BlogCache.Time_ViewCount);
                await postRepository.IncViewCountAsync(postId, ipList.Count);
            }
            else if (!ipList.Contains(ip))
            {
                ipList.Add(ip);
                await postRepository.IncViewCountAsync(postId, ipList.Count);
            }
        }
    }
}
