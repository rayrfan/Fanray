using Fan.Blog.Enums;
using Fan.Blog.Helpers;
using Fan.Blog.Models;
using Fan.Blog.Models.View;
using Fan.Helpers;
using Fan.Settings;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fan.Web.Helpers
{
    public class BlogViewModelHelper : IBlogViewModelHelper
    {
        private readonly ISettingService settingService;
        private readonly HttpRequest request;

        public BlogViewModelHelper(IHttpContextAccessor httpContextAccessor,
            ISettingService settingService)
        {
            this.settingService = settingService;
            request = httpContextAccessor.HttpContext.Request;
        }

        public async Task<BlogPostVM> GetBlogPostVMAsync(BlogPost blogPost)
        {
            var coreSettings = await settingService.GetSettingsAsync<CoreSettings>();
            var blogSettings = await settingService.GetSettingsAsync<BlogSettings>();

            var permalinkPart = BlogRoutes.GetPostPermalink(blogPost.Id);
            var relativeLink = BlogRoutes.GetPostRelativeLink(blogPost.CreatedOn, blogPost.Slug);
            var requestHostShort = request.Host.ToString().StartsWith("www.") ?
                                   request.Host.ToString().Remove(0, 4) : 
                                   request.Host.ToString();
            var permalinkShort = $"{request.Scheme}://{requestHostShort}{permalinkPart}";

            var hash = "";
            if (blogPost.Tags.Count > 0)
            {
                var sb = new StringBuilder();
                for (int i = 0; i < blogPost.Tags.Count; i++)
                {
                    var tag = blogPost.Tags[i];
                    sb.Append(tag.Slug.Replace("-", ""));
                    if (i < blogPost.Tags.Count - 1) sb.Append(",");
                }
                hash = sb.ToString();
            }

            return new BlogPostVM
            {
                Title = blogPost.Title,
                Body = blogPost.Body,
                Excerpt = blogPost.Excerpt,
                Author = blogPost.User.DisplayName,
                CreatedOn = blogPost.CreatedOn,
                CreatedOnDisplay = blogPost.CreatedOn.ToDisplayString(coreSettings.TimeZoneId),
                Tags = blogPost.Tags.OrderBy(t => t.Title).ToList(),
                Category = blogPost.Category,
                ViewCount = blogPost.ViewCount,

                RelativeLink = relativeLink,
                Permalink = $"{request.Scheme}://{request.Host}{permalinkPart}",
                CanonicalUrl = $"{request.Scheme}://{request.Host}{relativeLink}",
                EditLink = BlogRoutes.GetPostEditLink(blogPost.Id),

                DisqusPageIdentifier = $"{ECommentTargetType.BlogPost}_{blogPost.Id}",
                ShowDisqus = blogSettings.AllowComments && blogSettings.CommentProvider == ECommentProvider.Disqus && !blogSettings.DisqusShortname.IsNullOrEmpty(),
                DisqusShortname = blogSettings.DisqusShortname,

                TwitterShareLink = hash.IsNullOrEmpty() ?
                                   $"https://twitter.com/intent/tweet?text={blogPost.Title}&url={permalinkShort}" :
                                   $"https://twitter.com/intent/tweet?text={blogPost.Title}&url={permalinkShort}&hashtags={hash}",
                FacebookShareLink = $"https://www.facebook.com/sharer/sharer.php?u={permalinkShort}",
                LinkedInShareLink = $"http://www.linkedin.com/shareArticle?mini=true&url={permalinkShort}&title={blogPost.Title}",
            };
        }

        public async Task<BlogPostVM> GetBlogPostVMPreviewAsync(BlogPost blogPost)
        {
            blogPost.Body = OembedParser.Parse(blogPost.Body);
            var blogPostVM = await GetBlogPostVMAsync(blogPost);
            blogPostVM.DisqusShortname = ""; // when preview turn off disqus
            return blogPostVM;
        }

        public async Task<BlogPostListVM> GetBlogPostListVMAsync(BlogPostList blogPostList, int currentPage = 1)
        {
            var blogPostListVM = new BlogPostListVM();
            var blogSettings = await settingService.GetSettingsAsync<BlogSettings>();

            foreach (var blogPost in blogPostList.Posts)
            {
                var blogPostVM = await GetBlogPostVMAsync(blogPost);
                blogPostListVM.BlogPostViewModels.Add(blogPostVM);
            }

            blogPostListVM.PostListDisplay = blogSettings.PostListDisplay;
            blogPostListVM.PostCount = blogPostList.PostCount;

            if (currentPage <= 0) currentPage = 1;
            if ((currentPage * blogSettings.PostPerPage) < blogPostList.PostCount)
            {
                blogPostListVM.ShowOlder = true;
                blogPostListVM.OlderPostsUrl = $"{request.Path}?page={currentPage + 1}";
            }
            if (currentPage > 1)
            {
                blogPostListVM.ShowNewer = true;
                blogPostListVM.NewerPostsUrl = currentPage <= 2 ? $"{request.Path}" : $"{request.Path}?page={currentPage - 1}";
            }

            return blogPostListVM;
        }
        
        public async Task<BlogPostListVM> GetBlogPostListVMForArchiveAsync(BlogPostList blogPostList, int? year, int? month)
        {
            var blogPostListVM = await GetBlogPostListVMAsync(blogPostList);
            string monthName = (month.HasValue && month.Value > 0) ?
                               CultureInfo.CurrentCulture.DateTimeFormat.GetMonthName(month.Value) : "";
            blogPostListVM.ArchiveTitle = $"{monthName} {year.Value}";
            return blogPostListVM;
        }

        public async Task<BlogPostListVM> GetBlogPostListVMForCategoryAsync(BlogPostList blogPostList, Category cat, int currentPage = 1)
        {
            var blogPostListVM = await GetBlogPostListVMAsync(blogPostList, currentPage);
            blogPostListVM.CategoryTitle = cat.Title;
            blogPostListVM.Description = cat.Description;
            return blogPostListVM;
        }

        public async Task<BlogPostListVM> GetBlogPostListVMForTagAsync(BlogPostList blogPostList, Tag tag)
        {
            var blogPostListVM = await GetBlogPostListVMAsync(blogPostList);
            blogPostListVM.CategoryTitle = tag.Title;
            blogPostListVM.Description = tag.Description;
            return blogPostListVM;
        }
    }
}
