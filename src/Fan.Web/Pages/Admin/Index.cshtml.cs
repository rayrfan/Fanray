using Fan.Blogs.Enums;
using Fan.Blogs.Models;
using Fan.Blogs.Services;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
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

        public class PostVM
        {
            public int Id { get; set; }
            public string Title { get; set; }
            public string Date { get; set; }
        }

        public class TextValue
        {
            public string Text { get; set; }
            public int Value { get; set; }
        }

        /// <summary>
        /// Total post count for a particular status Published/Draft/Trashed.
        /// Used for pagination.
        /// </summary>
        public int TotalPostCount { get; set; }
        /// <summary>
        /// The posts based on blog, status and other user selections.
        /// </summary>
        public string JsonPosts { get; set; }
        /// <summary>
        /// All blogs.
        /// </summary>
        public string JsonBlogs { get; set; }
        public string JsonStatus { get; set; }

        public string ActiveStatus { get; set; }

        public async Task OnGetAsync(int? blogId, string status)
        {
            (JsonPosts, TotalPostCount) = await GetJsonPostsAsync(blogId, status);
            JsonBlogs = await GetJsonBlogsAsync();
            JsonStatus = GetJsonPostStatus();
        }

        private async Task<(string json, int count)> GetJsonPostsAsync(int? blog, string status)
        {
            if (string.IsNullOrEmpty(status)) status = "published";
            if (!blog.HasValue) blog = 1;

            BlogPostList postList = null;
            ActiveStatus = status;

            if (status == "draft")
                postList = await _blogSvc.GetPostsForDraftsAsync();
            else if (status == "trashed")
                postList = new BlogPostList();
            else
                postList = await _blogSvc.GetPostsAsync(1);

            var list = from p in postList.Posts
                       select new PostVM
                       {
                           Id = p.Id,
                           Title = p.Title,
                           Date = p.CreatedOn.ToString("yyyy-MM-dd"),
                       };

            return (JsonConvert.SerializeObject(list), postList.PostCount);
        }

        private async Task<string> GetJsonBlogsAsync()
        {
            var cats = await _blogSvc.GetCategoriesAsync();
            var blogs = from c in cats
                        select new TextValue
                        {
                            Value = c.Id,
                            Text = c.Title,
                        };

            return JsonConvert.SerializeObject(blogs);
        }

        /// <summary>
        /// Returns json of <see cref="EPostStatus"/>, like
        /// [{"Text":"Draft","Value":0},{"Text":"Published","Value":1},{"Text":"Trashed","Value":2},{"Text":"Scheduled","Value":3}]
        /// </summary>
        /// <returns></returns>
        private string GetJsonPostStatus()
        {
            var list = new List<TextValue>();
            var values = (EPostStatus[])Enum.GetValues(typeof(EPostStatus));
            foreach (var v in values)
            {
                list.Add(new TextValue { Text = v.ToString(), Value = (int)v });
            }

            return JsonConvert.SerializeObject(list);
        }
    }
}