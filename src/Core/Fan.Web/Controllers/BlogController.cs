using Fan.Blog.Enums;
using Fan.Blog.Helpers;
using Fan.Blog.Models;
using Fan.Blog.Models.View;
using Fan.Blog.Services.Interfaces;
using Fan.Settings;
using Fan.Web.Attributes;
using Fan.Web.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.SyndicationFeed;
using Microsoft.SyndicationFeed.Rss;
using System;
using System.IO;
using System.Threading.Tasks;
using System.Xml;

namespace Fan.Web.Controllers
{
    public class BlogController : Controller
    {
        private readonly IBlogPostService blogPostService;
        private readonly ICategoryService categoryService;
        private readonly ITagService tagService;
        private readonly IStatsService statsService;
        private readonly ISettingService settingService;
        private readonly IHomeHelper homeHelper;
        private readonly IBlogViewModelHelper blogViewModelHelper;
        private readonly IDistributedCache distributedCache;

        public BlogController(
            IBlogPostService blogPostService,
            ICategoryService categoryService,
            ITagService tagService,
            IStatsService statsService,
            ISettingService settingService,
            IHomeHelper homeHelper,
            IBlogViewModelHelper blogViewModelHelper,
            IDistributedCache distributedCache)
        {
            this.blogPostService = blogPostService;
            this.categoryService = categoryService;
            this.tagService = tagService;
            this.statsService = statsService;
            this.settingService = settingService;
            this.homeHelper = homeHelper;
            this.blogViewModelHelper = blogViewModelHelper;
            this.distributedCache = distributedCache;
        }

        /// <summary>
        /// Blog index page, redirect to home setup on initial launch.
        /// </summary>
        /// <returns></returns>
        [ModelPreRender]
        public async Task<IActionResult> Index(int? page)
        {
            var (_, viewModel) = await homeHelper.GetBlogIndexAsync(page);
            return View(viewModel);
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
        [ModelPreRender]
        public async Task<IActionResult> Post(int year, int month, int day, string slug)
        {
            var blogPost = await blogPostService.GetAsync(slug, year, month, day);
            var blogPostVM = await blogViewModelHelper.GetBlogPostVMAsync(blogPost);
            await statsService.IncViewCountAsync(EPostType.BlogPost, blogPost.Id);
            return View(blogPostVM);
        }

        /// <summary>
        /// Returns previewing of a single post.  
        /// </summary>
        /// <param name="year"></param>
        /// <param name="month"></param>
        /// <param name="day"></param>
        /// <param name="slug"></param>
        /// <returns></returns>
        [ModelPreRender]
        public async Task<IActionResult> PreviewPost(int year, int month, int day, string slug)
        {
            try
            {
                // Get back blog post from TempData
                DateTime dt = new DateTime(year, month, day);
                var link = BlogRoutes.GetPostPreviewRelativeLink(dt, slug);
                var blogPost = TempData.Get<BlogPost>(link);

                // Prep vm
                var blogPostVM = await blogViewModelHelper.GetBlogPostVMPreviewAsync(blogPost);

                // Show it
                return View("Post", blogPostVM);
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
            var post = await blogPostService.GetAsync(id);
            return Redirect(BlogRoutes.GetPostRelativeLink(post.CreatedOn, post.Slug));
        }

        public async Task<IActionResult> Category(string slug, int? page)
        {
            var (viewPath, viewModel) = await homeHelper.GetBlogCategoryAsync(slug, page);
            return View(viewPath, viewModel);
        }

        public async Task<IActionResult> Tag(string slug)
        {
            var tag = await tagService.GetBySlugAsync(slug);
            var posts = await blogPostService.GetListForTagAsync(slug, 1);
            var blogPostListVM = await blogViewModelHelper.GetBlogPostListVMForTagAsync(posts, tag);
            return View(blogPostListVM);
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
            var posts = await blogPostService.GetListForArchive(year, month);
            var blogPostListVM = await blogViewModelHelper.GetBlogPostListVMForArchiveAsync(posts, year, month);
            return View(blogPostListVM);
        }

        /// <summary>
        /// Category feed.
        /// </summary>
        /// <param name="slug"></param>
        /// <returns></returns>
        public async Task<ContentResult> CategoryFeed(string slug)
        {
            var cat = await categoryService.GetAsync(slug);
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

        [ModelPreRender]
        public async Task<IActionResult> Page(string parentPage, string childPage)
        {
            var (_, viewModel) = await homeHelper.GetPageAsync(parentPage, childPage);
            await statsService.IncViewCountAsync(EPostType.Page, viewModel.Id);
            viewModel.ViewCount++;
            return View(viewModel);
        }

        [ModelPreRender]
        public IActionResult PreviewPage(string parentSlug, string childSlug)
        {
            try
            {
                var link = BlogRoutes.GetPagePreviewRelativeLink(parentSlug, childSlug);
                var pageVM = TempData.Get<PageVM>(link);
                return View("Page", pageVM);
            }
            catch (Exception)
            {
                // when user access the preview link directly or when user clicks on other links 
                // and navigates away during the preview, hacky need to find a better way.
                return RedirectToAction("ErrorCode", "Home", new { statusCode = 404 });
            }
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
            return await distributedCache.GetAsync(key, time, async () =>
            {
                var sw = new StringWriter();
                using (XmlWriter xmlWriter = XmlWriter.Create(sw, new XmlWriterSettings() { Async = true, Indent = true }))
                {
                    var postList = cat == null ?
                                await blogPostService.GetListAsync(1, 10, cacheable: false) :
                                await blogPostService.GetListForCategoryAsync(cat.Slug, 1);
                    var coreSettings = await settingService.GetSettingsAsync<CoreSettings>();
                    var blogSettings = await settingService.GetSettingsAsync<BlogSettings>();
                    var vm = await blogViewModelHelper.GetBlogPostListVMAsync(postList);

                    var settings = await settingService.GetSettingsAsync<CoreSettings>();
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