using Fan.Blogs.Helpers;
using Fan.Blogs.Models;
using Fan.Blogs.ViewModels;
using Fan.Enums;
using Fan.Services;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace Fan.Blogs.Services
{
    public class BlogMapper : IBlogMapper
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly ISettingService _settingSvc;
        public BlogMapper(IHttpContextAccessor httpContextAccessor,
            ISettingService settingService)
        {
            _httpContextAccessor = httpContextAccessor;
            _settingSvc = settingService;
        }

        public async Task<BlogPostViewModel> GetBlogPostViewModelAsync(BlogPost post)
        {
            var request = _httpContextAccessor.HttpContext.Request;
            var permalinkPart = string.Format(BlogConst.POST_PERMA_URL_TEMPLATE, post.Id);
            var postVM = new BlogPostViewModel
            {
                BlogPost = post,
                Settings = await _settingSvc.GetSettingsAsync<BlogSettings>(),
                Permalink = $"{request.Scheme}://{request.Host}/{permalinkPart}",
                CanonicalUrl = $"{request.Scheme}://{request.Host}{post.RelativeLink}",
                DisqusPageIdentifier = $"{ECommentTargetType.BlogPost}_{post.Id}",
            };

            return postVM;
        }

        public async Task<BlogPostViewModelList> GetBlogPostViewModelListAsync(BlogPostList postList)
        {
            var vm = new BlogPostViewModelList();
            foreach (var post in postList)
            {
                vm.PostViewModels.Add(await GetBlogPostViewModelAsync(post));
            }
            vm.Settings = await _settingSvc.GetSettingsAsync<BlogSettings>();
            vm.PostCount = postList.PostCount;
            vm.PageCount = postList.PageCount;

            return vm;
        }
    }
}
