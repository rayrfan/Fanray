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

        /// <summary>
        /// Inner class only used by this page.
        /// </summary>
        public class ComposePost
        {
            public int Id { get; set; }
            [Required]
            public string Title { get; set; }
            public string Body { get; set; }
            public int BlogId { get; set; }
            public string PostDate { get; set; }
            public List<string> Tags { get; set; }
            public string Slug { get; set; }
            public string Excerpt { get; set; }

            public string TagsJson { get; set; }
            public string AllTagsJson { get; set; }
            public string AllBlogsJson { get; set; }
        }

        public class Blog
        {
            [JsonProperty]
            public string text { get; set; }
            [JsonProperty]
            public int value { get; set; }
        }

        /// <summary>
        /// Returns true if either post id less than or equal to 0 or post status is draft,
        /// otherwise false.
        /// </summary>
        /// <remarks>
        /// User could save an existing published post back to a draft, in this case post id
        /// would be greater than 0 but status would be draft.
        /// </remarks>
        public bool IsNew { get; set; }

        /// <summary>
        /// The post being composed.
        /// </summary>
        /// <remarks>
        /// When ajax POST, this property will be bound.
        /// </remarks>
        [FromBody]
        public ComposePost Post { get; set; }

        /// <summary>
        /// When user edits a post, it retrieves the post
        /// </summary>
        public async Task OnGetAsync(int postId)
        {
            if (postId > 0)
            {
                var post = await _blogSvc.GetPostAsync(postId);
                IsNew = post.Status == EPostStatus.Draft;

                Post = new ComposePost
                {
                    Id = post.Id,
                    Title = post.Title,
                    Body = post.Body,
                    PostDate = post.CreatedOn.ToString("yyyy-MM-dd"),
                    Tags = post.TagTitles,
                    TagsJson = JsonConvert.SerializeObject(post.TagTitles),
                    Slug = post.Slug,
                    Excerpt = post.Excerpt,
                };

                _logger.LogDebug("Composer Post: {@Post}");
            }
            else 
            {
                IsNew = true;
                Post = new ComposePost {
                    Title = "",
                    Body = "",
                    PostDate = DateTimeOffset.Now.ToString("yyyy-MM-dd"),
                    TagsJson = JsonConvert.SerializeObject(new string[0]),
                };
            }

            var tags = await _blogSvc.GetTagsAsync();
            string[] allTags = tags.Select(t => t.Title).ToArray();
            Post.AllTagsJson = JsonConvert.SerializeObject(allTags);

            var cats = await _blogSvc.GetCategoriesAsync();
            var blogs = from c in cats
                               select new Blog {
                                   value = c.Id,
                                   text = c.Title,
                               };

            // https://stackoverflow.com/a/7555096/32240
            var serializer = new JsonSerializer();
            var stringWriter = new StringWriter();
            using (var writer = new JsonTextWriter(stringWriter))
            {
                writer.QuoteName = false;
                serializer.Serialize(writer, blogs);
            }

            Post.AllBlogsJson = stringWriter.ToString(); //JsonConvert.SerializeObject(blogs);
        }

        /// <summary>
        /// Publish the post.
        /// </summary>
        /// <returns>The post's url.</returns>
        /// <remarks>
        /// The post could be new or previously published.
        /// </remarks>
        public async Task<JsonResult> OnPostPublishAsync()
        {
            var blogPost = new BlogPost
            {
                UserId = Convert.ToInt32(_userManager.GetUserId(HttpContext.User)),
                CategoryId = Post.BlogId,
                CreatedOn = GetCreatedOn(Post.PostDate),
                TagTitles = Post.Tags,
                Slug = Post.Slug,
                Excerpt = Post.Excerpt,
                Title = Post.Title,
                Body = Post.Body,
                Status = EPostStatus.Published,
            };

            if (Post.Id <= 0)
            {
                blogPost = await _blogSvc.CreatePostAsync(blogPost);
            }
            else
            {
                blogPost.Id = Post.Id;
                blogPost = await _blogSvc.UpdatePostAsync(blogPost);
            }

            return new JsonResult(GetPostAbsoluteUrl(blogPost));
        }

        /// <summary>
        /// Update an existing published post.
        /// </summary>
        /// <returns></returns>
        public async Task<JsonResult> OnPostUpdateAsync()
        {
            var blogPost = new BlogPost
            {
                Id = Post.Id,
                UserId = Convert.ToInt32(_userManager.GetUserId(HttpContext.User)),
                CategoryId = Post.BlogId,
                CreatedOn = GetCreatedOn(Post.PostDate),
                TagTitles = Post.Tags,
                Slug = Post.Slug,
                Excerpt = Post.Excerpt,
                Title = Post.Title,
                Body = Post.Body,
                Status = EPostStatus.Published,
            };
            blogPost = await _blogSvc.UpdatePostAsync(blogPost);
            return new JsonResult(GetPostAbsoluteUrl(blogPost));
        }

        /// <summary>
        /// Saves a post as draft.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// This is initiated by either auto save or user clicks on Save.
        /// </remarks>
        public async Task<JsonResult> OnPostSaveAsync()
        {
            var blogPost = new BlogPost
            {
                UserId = Convert.ToInt32(_userManager.GetUserId(HttpContext.User)),
                CategoryId = Post.BlogId,
                CreatedOn = GetCreatedOn(Post.PostDate),
                TagTitles = Post.Tags,
                Slug = Post.Slug,
                Excerpt = Post.Excerpt,
                Title = Post.Title,
                Body = Post.Body,
                Status = EPostStatus.Draft,
            };

            if (Post.Id <= 0)
            {
                blogPost = await _blogSvc.CreatePostAsync(blogPost);
            }
            else
            {
                blogPost.Id = Post.Id;
                blogPost = await _blogSvc.UpdatePostAsync(blogPost);
            }

            return new JsonResult(blogPost);
        }

        /// <summary>
        /// Upload images.
        /// </summary>
        /// <param name="images"></param>
        /// <returns></returns>
        /// <remarks>
        /// The name of images param must match formData.append('images', files[i]);
        /// </remarks>
        public async Task<JsonResult> OnPostImageAsync(IList<IFormFile> images)
        {
            var userId = Convert.ToInt32(_userManager.GetUserId(HttpContext.User));

            var urls = new List<string>();
            foreach (var image in images)
            {
                using (Stream s = image.OpenReadStream())
                {
                    var url = await _mediaSvc.UploadImageAsync(userId, image.FileName,
                            ReadFully(s), EAppType.Blog, EUploadedFrom.Browser);
                    urls.Add(url);
                }
            }

            return new JsonResult(urls);
        }

        private static byte[] ReadFully(Stream input)
        {
            byte[] buffer = new byte[16 * 1024];
            using (MemoryStream ms = new MemoryStream())
            {
                int read;
                while ((read = input.Read(buffer, 0, buffer.Length)) > 0)
                {
                    ms.Write(buffer, 0, read);
                }
                return ms.ToArray();
            }
        }

        private string GetPostAbsoluteUrl(BlogPost blogPost)
        {
            var relativeUrl = string.Format(BlogRoutes.POST_RELATIVE_URL_TEMPLATE,
                blogPost.CreatedOn.Year, blogPost.CreatedOn.Month, blogPost.CreatedOn.Day, blogPost.Slug);
            return $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}/{relativeUrl}";
        }

        /// <summary>
        /// Given a user input date "2018-05-18", returns a DateTimeOffset with also current time.
        /// </summary>
        /// <param name="date"></param>
        /// <returns></returns>
        private DateTimeOffset GetCreatedOn(string date)
        {
            var dt = DateTimeOffset.Parse(date);
            return new DateTimeOffset(dt.Year, dt.Month, dt.Day, DateTimeOffset.Now.Hour, DateTimeOffset.Now.Minute, DateTimeOffset.Now.Second,
                DateTimeOffset.Now.Offset);
        }
    }
}