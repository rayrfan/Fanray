using Fan.Blog.Helpers;
using Fan.Blog.Models;
using Fan.Blog.Services;
using Fan.Blog.Services.Interfaces;
using Fan.Helpers;
using Fan.Settings;
using Fan.Shortcodes;
using Fan.Web.Mvc.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using Microsoft.SyndicationFeed;
using Microsoft.SyndicationFeed.Rss;
using System;
using System.Globalization;
using System.IO;
using System.Threading.Tasks;
using System.Xml;

namespace Fan.Web.Mvc.Controllers
{
    public class BlogController : Controller
    {
        private readonly IBlogPostService _blogSvc;
        private readonly ICategoryService _catSvc;
        private readonly ITagService _tagSvc;
        private readonly ISettingService _settingSvc;
        private readonly ILogger<BlogController> _logger;
        private readonly IDistributedCache _cache;
        private readonly IShortcodeService _shortcodeSvc;

        public BlogController(
            IBlogPostService blogService,
            ICategoryService catService,
            ITagService tagService,
            ISettingService settingService,
            IDistributedCache cache,
            IShortcodeService shortcodeService,
            ILogger<BlogController> logger)
        {
            _blogSvc = blogService;
            _catSvc = catService;
            _tagSvc = tagService;
            _settingSvc = settingService;
            _cache = cache;
            _shortcodeSvc = shortcodeService;
            _logger = logger;
        }

        /// <summary>
        /// Blog index page, redirect to home setup on initial launch.
        /// </summary>
        /// <returns></returns>
        public async Task<IActionResult> Index(int? page)
        {
            if (!page.HasValue || page <= 0) page = BlogPostService.DEFAULT_PAGE_INDEX;
            var blogSettings = await _settingSvc.GetSettingsAsync<BlogSettings>();
            var posts = await _blogSvc.GetListAsync(page.Value, blogSettings.PostPerPage);

            var vm = new BlogPostListViewModel(posts, blogSettings, Request, page.Value);
            return View(vm);
        }

        /// <summary>
        /// Returns content of the RSD file which will tell where the MetaWeblog API endpoint is.
        /// </summary>
        /// <returns></returns>
        /// <remarks>
        /// https://en.wikipedia.org/wiki/Really_Simple_Discovery
        /// </remarks>
        public IActionResult Rsd()
        {
            var rootUrl = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";
            return View("Rsd", rootUrl);
        }

        /// <summary>
        /// Returns viewing of a single post.
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="day"></param>
        /// <param name="slug"></param>
        /// <returns></returns>
        public async Task<IActionResult> Post(int year, int month, int day, string slug)
        {
            var blogPost = await _blogSvc.GetAsync(slug, year, month, day);
            var blogSettings = await _settingSvc.GetSettingsAsync<BlogSettings>();
            var vm = new BlogPostViewModel(blogPost, blogSettings, Request);
            return View(vm);
        }

        /// <summary>
        /// Returns previewing of a single post.  
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="day"></param>
        /// <param name="slug"></param>
        /// <returns></returns>
        public async Task<IActionResult> Preview(int year, int month, int day, string slug)
        {
            try
            {
                // Get back blog post from TempData
                DateTime dt = new DateTime(year, month, day);
                var link = BlogRoutes.GetPostPreviewRelativeLink(dt, slug);
                var blogPost = TempData.Get<BlogPost>(link);

                // Prep it
                blogPost.Body = _shortcodeSvc.Parse(blogPost.Body);
                blogPost.Body = OembedParser.Parse(blogPost.Body);
                var blogSettings = await _settingSvc.GetSettingsAsync<BlogSettings>();
                blogSettings.DisqusShortname = ""; // when preview turn off disqus
                var vm = new BlogPostViewModel(blogPost, blogSettings, Request);

                // Show it
                return View("Post", vm);
            }
            catch (Exception)
            {
                // when user access the preview link directly or when user clicks on other links 
                // and navigates away during the preview, hacky need to find a better way.
                return RedirectToAction("ErrorCode", "Home", new { statusCode = 404 });
            }
        }

        public async Task<IActionResult> PostPerma(int id)
        {
            var post = await _blogSvc.GetAsync(id);
            return Redirect(BlogRoutes.GetPostRelativeLink(post.CreatedOn, post.Slug));
        }

        public async Task<IActionResult> Category(string slug)
        {
            var cat = await _catSvc.GetAsync(slug);
            var posts = await _blogSvc.GetListForCategoryAsync(slug, 1);
            var blogSettings = await _settingSvc.GetSettingsAsync<BlogSettings>();
            var vm = new BlogPostListViewModel(posts, blogSettings, Request, cat);
            return View(vm);
        }

        public async Task<IActionResult> Tag(string slug)
        {
            var tag = await _tagSvc.GetBySlugAsync(slug);
            var posts = await _blogSvc.GetListForTagAsync(slug, 1);
            var blogSettings = await _settingSvc.GetSettingsAsync<BlogSettings>();
            var vm = new BlogPostListViewModel(posts, blogSettings, Request, tag);
            return View(vm);
        }

        /// <summary>
        /// Archive page.
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <returns></returns>
        public async Task<IActionResult> Archive(int? year, int? month)
        {
            if (!year.HasValue) return RedirectToAction("Index");

            var posts = await _blogSvc.GetListForArchive(year, month);
            var blogSettings = await _settingSvc.GetSettingsAsync<BlogSettings>();
            string monthName = (month.HasValue && month.Value > 0) ?
                CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month.Value) : "";

            var vm = new BlogPostListViewModel(posts, blogSettings, Request)
            {
                ArchiveTitle = $"{monthName} {year.Value}"
            };
            return View(vm);
        }

        /// <summary>
        /// Category feed.
        /// </summary>
        /// <param name="slug"></param>
        /// <returns></returns>
        public async Task<ContentResult> CategoryFeed(string slug)
        {
            var cat = await _catSvc.GetAsync(slug);
            var rss = await GetFeed(cat);
            return new ContentResult
            {
                ContentType = "application/xml",
                Content = rss,
                StatusCode = 200
            };
        }

        /// <summary>
        /// Blog main feed.
        /// </summary>
        /// <returns></returns>
        public async Task<ContentResult> Feed()
        {
            var rss = await GetFeed();
            return new ContentResult
            {
                ContentType = "application/xml",
                Content = rss,
                StatusCode = 200
            };
        }

        /// <summary>
        /// Returns the rss xml string for the blog or a blog category.
        /// The rss feed always returns first page with 10 results.
        /// </summary>
        /// <param name="cat"></param>
        /// <returns></returns>
        private async Task<string> GetFeed(Category cat = null)
        {
            var key = cat == null ? BlogCache.KEY_MAIN_RSSFEED : string.Format(BlogCache.KEY_CAT_RSSFEED, cat.Slug);
            var time = cat == null ? BlogCache.Time_MainRSSFeed : BlogCache.Time_CatRSSFeed;
            return await _cache.GetAsync(key, time, async () =>
            {
                var sw = new StringWriter();
                using (XmlWriter xmlWriter = XmlWriter.Create(sw, new XmlWriterSettings() { Async = true, Indent = true }))
                {
                    var postList = cat == null ?
                                await _blogSvc.GetListAsync(1, 10, cacheable: false) :
                                await _blogSvc.GetListForCategoryAsync(cat.Slug, 1);
                    var coreSettings = await _settingSvc.GetSettingsAsync<CoreSettings>();
                    var blogSettings = await _settingSvc.GetSettingsAsync<BlogSettings>();
                    var vm = new BlogPostListViewModel(postList, blogSettings, Request);

                    var settings = await _settingSvc.GetSettingsAsync<CoreSettings>();
                    var channelTitle = cat == null ? settings.Title : $"{cat.Title} - {settings.Title}";
                    var channelDescription = coreSettings.Tagline;
                    var channelLink = $"{Request.Scheme}://{Request.Host}";
                    var channelLastPubDate = postList.Posts.Count <= 0 ? DateTimeOffset.UtcNow : postList.Posts[0].CreatedOn;

                    var writer = new RssFeedWriter(xmlWriter);
                    await writer.WriteTitle(channelTitle);
                    await writer.WriteDescription(channelDescription);
                    await writer.Write(new SyndicationLink(new Uri(channelLink)));
                    await writer.WritePubDate(channelLastPubDate);
                    await writer.WriteGenerator("https://www.fanray.com");

                    foreach (var postVM in vm.BlogPostViewModels)
                    {
                        var post = postVM;
                        var item = new SyndicationItem()
                        {
                            Id = postVM.Permalink, // guid https://www.w3schools.com/xml/rss_tag_guid.asp
                            Title = post.Title,
                            Description = blogSettings.FeedShowExcerpt ? post.Excerpt : post.Body,
                            Published = post.CreatedOn,
                        };

                        // link to the post
                        item.AddLink(new SyndicationLink(new Uri(postVM.CanonicalUrl)));

                        // category takes in both cats and tags
                        item.AddCategory(new SyndicationCategory(post.Category.Title));
                        foreach (var tag in post.Tags)
                        {
                            item.AddCategory(new SyndicationCategory(tag.Title));
                        }

                        // https://www.w3schools.com/xml/rss_tag_author.asp
                        // the author tag exposes email  
                        //item.AddContributor(new SyndicationPerson(post.User.DisplayName, post.User.Email));

                        await writer.Write(item);
                    }

                    xmlWriter.Flush();
                }

                return sw.ToString();
            });
        }
    }
}