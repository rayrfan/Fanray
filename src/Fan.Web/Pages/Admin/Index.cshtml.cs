using Fan.Blogs.Enums;
using Fan.Blogs.Helpers;
using Fan.Blogs.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fan.Web.Pages.Admin
{
    public class IndexModel : PageModel
    {
        private readonly IBlogService _blogSvc;

        public IndexModel(IBlogService blogService)
        {
            _blogSvc = blogService;
        }

        public class PostListVM
        {
            public IEnumerable<PostVM> Posts {get;set;}
            public int TotalPosts { get; set; }
            public IEnumerable<StatusVM> Statuses { get; set; }
            public int PublishedCount { get; set; }
            public int DraftCount { get; set; }
        }

        public class PostVM
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Date { get; set; }
            public string Author { get; set; }
            public string EditLink { get; set; }
            public string PostLink { get; set; }
        }

        public class StatusVM
        {
            /// <summary>
            /// When using Vuetify Tabs, the property to bind its key must be named "id".
            /// </summary>
            public int Id { get; set; }
            public string Text { get; set; }
            public int Count { get; set; }
        }

        /// <summary>
        /// Ajax GET post list view model by status, page number and page size.
        /// </summary>
        /// <remarks>
        /// NOTE: the parameter cannot be named "page".
        /// </remarks>
        public async Task<JsonResult> OnGetPostsAsync(string status, int pageNumber, int pageSize)
        {
            var list = await GetPostListVmAsync(status, pageNumber, pageSize);
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
            await _blogSvc.DeletePostAsync(postId);
            var list = await GetPostListVmAsync(status, pageNumber, pageSize);
            return new JsonResult(list);
        }

        /// <summary>
        /// Returns posts, total posts and post statuses.
        /// </summary>
        /// <param name="status">The post status <see cref="EPostStatus"/></param>
        /// <param name="pageNumber">Which page</param>
        /// <param name="pageSize">How many rows per page</param>
        /// <returns></returns>
        /// <remarks>
        /// I did a workaround for tabs, couldn't get "statuses" to work as the tab is not initially selected.
        /// </remarks>
        private async Task<PostListVM> GetPostListVmAsync(string status, int pageNumber, int pageSize)
        {
            // posts and totalPosts
            var postList = status.Equals("published", StringComparison.InvariantCultureIgnoreCase) ?
                await _blogSvc.GetPostsAsync(pageNumber, pageSize) :
                await _blogSvc.GetPostsForDraftsAsync();

            var postVms = from p in postList.Posts
                          select new PostVM
                          {
                              Id = p.Id,
                              Title = p.Title,
                              Date = p.CreatedOn.ToString("yyyy-MM-dd"),
                              Author = p.User.DisplayName,
                              EditLink = string.Format("/" + BlogRoutes.POST_EDIT_URL_TEMPLATE, p.Id),
                              PostLink = $"{Request.Scheme}://{Request.Host}/" +
                              string.Format(BlogRoutes.POST_RELATIVE_URL_TEMPLATE, p.CreatedOn.Year, p.CreatedOn.Month, p.CreatedOn.Day, p.Slug),
                          };

            // statuses
            var postCount = await _blogSvc.GetPostCountAsync();
            var statusVMs = new List<StatusVM>
            {
                new StatusVM { Text = "Published", Id = (int)EPostStatus.Published, Count = postCount.Published },
                new StatusVM { Text = "Drafts", Id = (int)EPostStatus.Draft, Count = postCount.Draft },
            };

            // prep vm
            return new PostListVM
            {
                Posts = postVms,
                TotalPosts = postList.PostCount,
                Statuses = statusVMs,
                PublishedCount = postCount.Published,
                DraftCount = postCount.Draft,
            };
        }
    }
}