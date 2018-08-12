using Fan.Blog.Enums;
using Fan.Blog.Helpers;
using Fan.Blog.Models;
using Fan.Blog.Services;
using Fan.Medias;
using Fan.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Fan.Web.Pages.Admin
{
    /// <summary>
    /// Model to Compose.cshtml.
    /// </summary>
    /// <remarks>
    /// This handles get, publish, update and save draft for a post.
    /// For image upload it calls the endpoint in Media.cshtml.cs.
    /// </remarks>
    public class ComposeModel : PageModel
    {
        private readonly IBlogService _blogSvc;
        private readonly ILogger<ComposeModel> _logger;
        private readonly UserManager<User> _userManager;
        private readonly IMediaService _mediaSvc;

        private const string DATE_FORMAT = "yyyy-MM-dd";

        // -------------------------------------------------------------------- constructor

        public ComposeModel(
            UserManager<User> userManager,
            IBlogService blogService,
            IMediaService mediaSvc,
            ILogger<ComposeModel> logger)
        {
            _userManager = userManager;
            _blogSvc = blogService;
            _mediaSvc = mediaSvc;
            _logger = logger;
        }

        // -------------------------------------------------------------------- inner classes

        /// <summary>
        /// The view model for composer.
        /// </summary>
        public class ComposeVM
        {
            public PostVM Post { get; set; }
            public IEnumerable<CatVM> AllCats { get; set; }
            public string[] AllTags { get; set; }
        }

        /// <summary>
        /// Post view model.
        /// </summary>
        public class PostVM
        {
            public int Id { get; set; }
            [Required]
            public string Title { get; set; }
            public string Body { get; set; }
            public string PostDate { get; set; }
            public string Slug { get; set; }
            public string Excerpt { get; set; }
            public int CategoryId { get; set; }
            public List<string> Tags { get; set; }
            public bool Published { get; set; }
            public bool IsDraft { get; set; }
            public string DraftDate { get; set; }
        }

        /// <summary>
        /// Category view model for Categories dropdown, property names must be "Value" and "Text".
        /// </summary>
        public class CatVM
        {
            public int Value { get; set; }
            public string Text { get; set; }
        }

        // -------------------------------------------------------------------- public methods

        /// <summary>
        /// Ajax GET to return <see cref="ComposeVM"/> to initialize the page.
        /// </summary>
        /// <remarks>
        /// NOTE: the parameter cannot be named "page".
        /// </remarks>
        /// <param name="postId">0 for a new post or an existing post id</param>
        /// <returns></returns>
        public async Task<JsonResult> OnGetPostAsync(int postId)
        {
            PostVM postVm;
            if (postId > 0) // existing post
            {
                var post = await _blogSvc.GetPostAsync(postId);
                postVm = new PostVM
                {
                    Id = post.Id,
                    Title = post.Title,
                    Body = post.Body,
                    PostDate = post.CreatedOn.ToString(DATE_FORMAT),
                    Slug = post.Slug,
                    Excerpt = post.Excerpt,
                    CategoryId = post.CategoryId ?? 1,
                    Tags = post.TagTitles,
                    Published = post.Status == EPostStatus.Published,
                    IsDraft = post.Status == EPostStatus.Draft,
                    DraftDate = post.UpdatedOn.HasValue ? post.UpdatedOn.Value.ToString(DATE_FORMAT) : "",
                };
            }
            else // new post
            {
                postVm = new PostVM
                {
                    Title = "",
                    Body = "",
                    PostDate = DateTimeOffset.Now.ToString(DATE_FORMAT),
                    CategoryId = 1,
                    Tags = new List<string>(),
                    Published = false,
                    IsDraft = false,
                };
            }

            var categories = await _blogSvc.GetCategoriesAsync();
            var allCats = from c in categories
                       select new CatVM
                       {
                           Value = c.Id,
                           Text = c.Title,
                       };

            var tags = await _blogSvc.GetTagsAsync();
            var allTags = tags.Select(t => t.Title).ToArray();

            return new JsonResult(new ComposeVM
            {
                Post = postVm,
                AllCats = allCats,
                AllTags = allTags,
            });
        }

        /// <summary>
        /// Ajax POST to publish a post.
        /// </summary>
        /// <returns>
        /// Absolute URL to the post.
        /// </returns>
        /// <remarks>
        /// The post could be new or previously published.
        /// </remarks>
        public async Task<JsonResult> OnPostPublishAsync([FromBody]PostVM post)
        {
            var blogPost = new BlogPost
            {                
                UserId = Convert.ToInt32(_userManager.GetUserId(HttpContext.User)),
                CategoryId = post.CategoryId,
                CreatedOn = GetCreatedOn(post.PostDate),
                TagTitles = post.Tags,
                Slug = post.Slug,
                Excerpt = post.Excerpt,
                Title = post.Title,
                Body = post.Body,
                Status = EPostStatus.Published,
            };

            if (post.Id <= 0)
            {
                blogPost = await _blogSvc.CreatePostAsync(blogPost);
            }
            else
            {
                blogPost.Id = post.Id;
                blogPost = await _blogSvc.UpdatePostAsync(blogPost);
            }

            return new JsonResult(GetPostAbsoluteUrl(blogPost));
        }

        /// <summary>
        /// Ajax POST to update an existing published post.
        /// </summary>
        /// <returns>
        /// Absolute URL to the post.
        /// </returns>
        public async Task<JsonResult> OnPostUpdateAsync([FromBody]PostVM post)
        {
            var blogPost = new BlogPost
            {
                Id = post.Id,
                UserId = Convert.ToInt32(_userManager.GetUserId(HttpContext.User)),
                CategoryId = post.CategoryId,
                CreatedOn = GetCreatedOn(post.PostDate),
                TagTitles = post.Tags,
                Slug = post.Slug,
                Excerpt = post.Excerpt,
                Title = post.Title,
                Body = post.Body,
                Status = EPostStatus.Published,
            };
            blogPost = await _blogSvc.UpdatePostAsync(blogPost);
            return new JsonResult(GetPostAbsoluteUrl(blogPost));
        }

        /// <summary>
        /// Ajax POST to save a post as draft.
        /// </summary>
        /// <returns>
        /// The updated <see cref="BlogPost"/>.
        /// </returns>
        /// <remarks>
        /// This is called by either auto save or user clicking on Save.
        /// </remarks>
        public async Task<JsonResult> OnPostSaveAsync([FromBody]PostVM post)
        {
            var blogPost = new BlogPost
            {
                UserId = Convert.ToInt32(_userManager.GetUserId(HttpContext.User)),
                CategoryId = post.CategoryId,
                CreatedOn = GetCreatedOn(post.PostDate),
                TagTitles = post.Tags,
                Slug = post.Slug,
                Excerpt = post.Excerpt,
                Title = post.Title,
                Body = post.Body,
                Status = EPostStatus.Draft,
            };

            if (post.Id <= 0)
            {
                blogPost = await _blogSvc.CreatePostAsync(blogPost);
            }
            else
            {
                blogPost.Id = post.Id;
                blogPost = await _blogSvc.UpdatePostAsync(blogPost);
            }

            var postVm = new PostVM
            {
                Id = blogPost.Id,
                Title = blogPost.Title,
                Body = blogPost.Body,
                PostDate = blogPost.CreatedOn.ToString(DATE_FORMAT),
                Slug = blogPost.Slug,
                Excerpt = blogPost.Excerpt,
                CategoryId = blogPost.CategoryId ?? 1,
                Tags = blogPost.TagTitles,
                Published = blogPost.Status == EPostStatus.Published,
                IsDraft = blogPost.Status == EPostStatus.Draft,
                DraftDate = blogPost.UpdatedOn.HasValue ? blogPost.UpdatedOnDisplay : "",
            };

            return new JsonResult(postVm);
        }

        // -------------------------------------------------------------------- private methods

        private string GetPostAbsoluteUrl(BlogPost blogPost)
        {
            var relativeUrl = string.Format(BlogRoutes.POST_RELATIVE_URL_TEMPLATE,
                blogPost.CreatedOn.Year, blogPost.CreatedOn.Month, blogPost.CreatedOn.Day, blogPost.Slug);
            return $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/{relativeUrl}";
        }

        /// <summary>
        /// Returns a DateTimeOffset by appending current time to the given date string for example "2018-05-18".
        /// </summary>
        /// <param name="date">A date string for example "2018-05-18"</param>
        /// <returns></returns>
        private DateTimeOffset GetCreatedOn(string date)
        {
            var dt = DateTimeOffset.Parse(date);
            return new DateTimeOffset(dt.Year, dt.Month, dt.Day, DateTimeOffset.Now.Hour, DateTimeOffset.Now.Minute, DateTimeOffset.Now.Second,
                DateTimeOffset.Now.Offset);
        }
    }
}