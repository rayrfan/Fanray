using Fan.Models;
using Fan.Services;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Threading.Tasks;

namespace Fan.Web.Controllers
{
    public class BlogController : Controller
    {
        private readonly IBlogService _blogSvc;
        public BlogController(IBlogService blogService)
        {
            _blogSvc = blogService;
        }

        public async Task<IActionResult> Index()
        {
            var posts = await _blogSvc.GetPostsAsync(1);
            return View(posts);
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
            var post = await _blogSvc.GetPostAsync(slug, year, month, day);
            return View(post);
        }

        public async Task<IActionResult> Category(string slug)
        {
            var cat = await _blogSvc.GetCategoryAsync(slug);
            var posts = await _blogSvc.GetPostsForCategoryAsync(slug, 1);

            return View(new Tuple<Category, BlogPostList>(cat, posts));
        }

        public async Task<IActionResult> Tag(string slug)
        {
            var tag = await _blogSvc.GetTagAsync(slug);
            var posts = await _blogSvc.GetPostsForTagAsync(slug, 1);

            return View(new Tuple<Tag, BlogPostList>(tag, posts));
        }
    }
}