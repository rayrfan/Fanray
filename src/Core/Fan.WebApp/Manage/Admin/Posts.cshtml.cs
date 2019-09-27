using Fan.Blog.Helpers;
using Fan.Blog.Services.Interfaces;
using Fan.Helpers;
using Fan.Settings;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fan.WebApp.Manage.Admin
{
    public class PostsModel : PageModel
    {
        private readonly IBlogPostService blogPostService;
        private readonly IStatsService statsService;
        private readonly ISettingService settingService;

        public PostsModel(IBlogPostService blogPostService, 
            IStatsService statsService,
            ISettingService settingService)
        {
            this.blogPostService = blogPostService;
            this.statsService = statsService;
            this.settingService = settingService;
        }

        /// <summary>
        /// For post data table footer pagination.
        /// </summary>
        public const string DEFAULT_ROW_PER_PAGE_ITEMS = "[25, 50]";

        public const string POST_DATE_STRING_FORMAT = "yyyy-MM-dd";

        /// <summary>
        /// The json data to initially bootstrap page.
        /// </summary>
        public PostListVM Data { get; set; }

        /// <summary>
        /// Used for vuetify for active tab.
        /// </summary>
        /// <remarks>
        /// Note vuetify tab counts tab key 0, 1, 2... by their physical location, in my case
        /// Draft has value of 0 but is put after Published which has value of 1, so I manually
        /// reversed them, this does not affect anything else.
        /// </remarks>
        public string ActiveStatus { get; set; }

        /// <summary>
        /// GET initial page.
        /// </summary>
        public async Task OnGetAsync(string status = "published")
        {
            Data = await GetPostListVMAsync(status, pageNumber: 1, pageSize: 25);
            ActiveStatus = status;
        }

        /// <summary>
        /// Ajax GET post list view model by status, page number and page size.
        /// </summary>
        /// <remarks>
        /// NOTE: the parameter cannot be named "page".
        /// </remarks>
        public async Task<JsonResult> OnGetPostsAsync(string status, int pageNumber, int pageSize)
        {
            var list = await GetPostListVMAsync(status, pageNumber, pageSize);
            return new JsonResult(list);
        }

        /// <summary>
        /// Ajax DELETE a post by id, then returns the up to date posts, total posts and post statuses.
        /// </summary>
        /// <param name="postId"></param>
        /// <param name="status"></param>
        /// <param name="pageNumber"></param>
        /// <param name="pageSize"></param>
        /// <returns></returns>
        public async Task<JsonResult> OnDeleteAsync(int postId, string status, int pageNumber, int pageSize)
        {
            await blogPostService.DeleteAsync(postId);
            var list = await GetPostListVMAsync(status, pageNumber, pageSize);
            return new JsonResult(list);
        }

        /// <summary>
        /// Returns posts, total posts and post statuses.
        /// </summary>
        /// <param name="status">The post status <see cref="EPostStatus"/></param>
        /// <param name="pageNumber">Which page, 1-based.</param>
        /// <param name="pageSize">How many rows per page</param>
        /// <returns></returns>
        /// <remarks>
        /// I did a workaround for tabs, couldn't get "statuses" to work as the tab is not initially selected.
        /// </remarks>
        private async Task<PostListVM> GetPostListVMAsync(string status, int pageNumber, int pageSize)
        {
            var postList = status.Equals("published", StringComparison.InvariantCultureIgnoreCase) ?
                await blogPostService.GetListAsync(pageNumber, pageSize, cacheable: false) :
                await blogPostService.GetListForDraftsAsync(); // TODO drafts need pagination too

            var coreSettings = await settingService.GetSettingsAsync<CoreSettings>();
            var postVms = from p in postList.Posts
                          select new PostVM
                          {
                              Id = p.Id,
                              Title = p.Title,
                              Date = Util.ConvertTime(p.CreatedOn, coreSettings.TimeZoneId).ToString(POST_DATE_STRING_FORMAT),
                              Author = p.User.DisplayName,
                              EditLink = BlogRoutes.GetPostEditLink(p.Id),
                              PostLink = $"{Request.Scheme}://{Request.Host}" + BlogRoutes.GetPostRelativeLink(p.CreatedOn, p.Slug),
                              ViewCount = p.ViewCount,
                          };

            var postCount = await statsService.GetPostCountAsync();

            // prep vm
            return new PostListVM
            {
                Posts = postVms,
                TotalPosts = postList.PostCount,
                PublishedCount = postCount.Published,
                DraftCount = postCount.Draft,
            };
        }
    }

    public class PostListVM
    {
        public IEnumerable<PostVM> Posts { get; set; }
        public int TotalPosts { get; set; }
        public int PublishedCount { get; set; }
        public int DraftCount { get; set; }
        public string JsonPosts => Posts == null ? "" : JsonConvert.SerializeObject(Posts);
    }

    public class PostVM
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Date { get; set; }
        public string Author { get; set; }
        public string EditLink { get; set; }
        public string PostLink { get; set; }
        public int ViewCount { get; set; }
    }
}