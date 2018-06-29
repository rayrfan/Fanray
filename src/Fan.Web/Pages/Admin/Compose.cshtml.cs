using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Fan.Blogs.Enums;
using Fan.Blogs.Helpers;
using Fan.Blogs.Models;
using Fan.Blogs.Services;
using Fan.Medias;
using Fan.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Fan.Web.Pages.Admin
{
    public class ComposeModel : PageModel
    {
        private readonly IBlogService _blogSvc;
        private readonly ILogger<ComposeModel> _logger;
        private readonly UserManager<User> _userManager;
        private readonly IMediaService _mediaSvc;

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
            public bool Published { get; set; }
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
            bool published = false;
            if (postId > 0) // existing post
            {
                var post = await _blogSvc.GetPostAsync(postId);
                postVm = new PostVM
                {
                    Id = post.Id,
                    Title = post.Title,
                    Body = post.Body,
                    PostDate = post.CreatedOn.ToString("yyyy-MM-dd"),
                    Slug = post.Slug,
                    Excerpt = post.Excerpt,
                    CategoryId = post.CategoryId ?? 1,
                    Tags = post.TagTitles,
                };

                published = post.Status == EPostStatus.Published;
            }
            else // new post
            {
                postVm = new PostVM
                {
                    Title = "",
                    Body = "",
                    PostDate = DateTimeOffset.Now.ToString("yyyy-MM-dd"),
                    CategoryId = 1,
                    Tags = new List<string>(),
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
                Published = published,
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

            return new JsonResult(blogPost);
        }

        /// <summary>
        /// Ajax POST to upload images.
        /// </summary>
        /// <returns>
        /// A list of urls to original sized images.
        /// </returns>
        /// <param name="images">
        /// The name of this parameter must match <code>formData.append('images', files[i]); </code>
        /// in the uploadImages method in javascript.
        /// </param>
        public async Task<JsonResult> OnPostImageAsync(IList<IFormFile> images)
        {
            var userId = Convert.ToInt32(_userManager.GetUserId(HttpContext.User));

            var urls = new List<string>();
            foreach (var image in images)
            {
                using (Stream stream = image.OpenReadStream())
                {
                    var url = await _mediaSvc.UploadImageAsync(stream, EAppType.Blog, userId, 
                        image.FileName, EUploadedFrom.Browser);

                    urls.Add(url);
                }
            }

            return new JsonResult(urls);
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