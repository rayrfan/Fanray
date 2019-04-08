using Fan.Blog.Enums;
using Fan.Blog.Helpers;
using Fan.Blog.Models;
using Fan.Blog.Services.Interfaces;
using Fan.Helpers;
using Fan.Medias;
using Fan.Membership;
using Fan.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Fan.Web.Manage.Admin
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
        private readonly IBlogPostService _blogSvc;
        private readonly ICategoryService _catSvc;
        private readonly ITagService _tagSvc;
        private readonly ISettingService _settingSvc;
        private readonly UserManager<User> _userManager;
        private readonly IMediaService _mediaSvc;

        // -------------------------------------------------------------------- constructor

        public ComposeModel(
            UserManager<User> userManager,
            IBlogPostService blogService,
            ICategoryService catService,
            ITagService tagService,
            IMediaService mediaSvc,
            ISettingService settingService)
        {
            _userManager = userManager;
            _blogSvc = blogService;
            _catSvc = catService;
            _tagSvc = tagService;
            _mediaSvc = mediaSvc;
            _settingSvc = settingService;
        }

        // -------------------------------------------------------------------- inner classes

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
            public List<string> Tags { get; set; } // titles, not slugs
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

        // -------------------------------------------------------------------- consts & properties

        /// <summary>
        /// How many seconds to wait after user stops typing to auto save. Default 10 seconds.
        /// </summary>
        public const int AUTOSAVE_INTERVAL = 10;
        /// <summary>
        /// Post date display format.
        /// </summary>
        /// <remarks>
        /// Vuetify datepicker works this format by default and it's a lot more work to change this format.
        /// </remarks>
        private const string DATE_FORMAT = "yyyy-MM-dd";
        public string PostJson { get; set; }
        public string CatsJson { get; set; }
        public string TagsJson { get; set; }
        public string Theme { get; set; }

        // -------------------------------------------------------------------- public methods

        /// <summary>
        /// GET to return <see cref="ComposeVM"/> to initialize the page.
        /// </summary>
        /// <remarks>
        /// NOTE: the parameter cannot be named "page".
        /// </remarks>
        /// <param name="postId">0 for a new post or an existing post id</param>
        /// <returns></returns>
        public async Task OnGetAsync(int postId)
        {
            // theme
            var coreSettings = await _settingSvc.GetSettingsAsync<CoreSettings>();
            Theme = coreSettings.Theme;

            // post
            PostVM postVM;
            if (postId > 0) // existing post
            {
                var post = await _blogSvc.GetAsync(postId);
                postVM = new PostVM
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
                var date = Util.ConvertTime(DateTimeOffset.UtcNow, coreSettings.TimeZoneId).ToString(DATE_FORMAT);
                var blogSettings = await _settingSvc.GetSettingsAsync<BlogSettings>();

                postVM = new PostVM
                {
                    Title = "",
                    Body = "",
                    PostDate = date,
                    CategoryId = blogSettings.DefaultCategoryId,
                    Tags = new List<string>(),
                    Published = false,
                    IsDraft = false,
                };
            }
            PostJson = JsonConvert.SerializeObject(postVM);

            // cats
            var categories = await _catSvc.GetAllAsync();
            var allCats = from c in categories
                          select new CatVM
                          {
                              Value = c.Id,
                              Text = c.Title,
                          };
            CatsJson = JsonConvert.SerializeObject(allCats);

            // tags
            var tags = await _tagSvc.GetAllAsync();
            var allTags = tags.Select(t => t.Title).ToArray();
            TagsJson = JsonConvert.SerializeObject(allTags);
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
                blogPost = await _blogSvc.CreateAsync(blogPost);
            }
            else
            {
                blogPost.Id = post.Id;
                blogPost = await _blogSvc.UpdateAsync(blogPost);
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
            blogPost = await _blogSvc.UpdateAsync(blogPost);
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
                blogPost = await _blogSvc.CreateAsync(blogPost);
            }
            else
            {
                blogPost.Id = post.Id;
                blogPost = await _blogSvc.UpdateAsync(blogPost);
            }

            var postVM = new PostVM
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

            return new JsonResult(postVM);
        }

        /// <summary>
        /// Preview
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        public async Task<JsonResult> OnPostPreviewAsync([FromBody]PostVM post)
        {
            // prep blog post
            List<Tag> tags = new List<Tag>();
            foreach (var title in post.Tags) // titles
            {
                tags.Add(await _tagSvc.GetByTitleAsync(title));
            }

            var blogPost = new BlogPost
            {
                User = await _userManager.GetUserAsync(HttpContext.User),
                UserId = Convert.ToInt32(_userManager.GetUserId(HttpContext.User)),
                Category = await _catSvc.GetAsync(post.CategoryId),
                CreatedOn = GetCreatedOn(post.PostDate),
                Tags = tags,
                Slug = post.Slug.IsNullOrEmpty() ? "untitled" : post.Slug,
                Excerpt = post.Excerpt,
                Title = post.Title.IsNullOrEmpty() ? "Untitled" : post.Title,
                Body = post.Body,
            };

            // prep TempData
            var prevRelLink = BlogRoutes.GetPostPreviewRelativeLink(blogPost.CreatedOn, blogPost.Slug);
            TempData.Put(prevRelLink, blogPost);

            // return preview url
            return new JsonResult($"{Request.Scheme}://{Request.Host}{prevRelLink}");
        }

        // -------------------------------------------------------------------- private methods

        private string GetPostAbsoluteUrl(BlogPost blogPost)
        {
            var relativeUrl = BlogRoutes.GetPostRelativeLink(blogPost.CreatedOn, blogPost.Slug);
            return $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}{relativeUrl}";
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