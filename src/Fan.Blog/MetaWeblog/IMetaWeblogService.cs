using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fan.Blog.MetaWeblog
{
    public interface IMetaWeblogService
    {
        // Category, Tag, Media

        Task<List<MetaCategory>> GetCategoriesAsync(string blogId, string userName, string password, string rootUrl);
        Task<int> CreateCategoryAsync(string name, string userName, string password);
        Task<List<string>> GetKeywordsAsync(string blogId, string userName, string password);
        Task<List<MetaBlogInfo>> GetUsersBlogsAsync(string appKey, string userName, string password, string rootUrl);
        Task<MetaMediaInfo> NewMediaObjectAsync(string blogId, string userName, string password, MetaMediaObject mediaObject, HttpContext request);

        // Post

        Task<string> NewPostAsync(string blogId, string userName, string password, MetaPost post, bool publish);
        Task<bool> EditPostAsync(string postId, string userName, string password, MetaPost post, bool publish);
        Task<bool> DeletePostAsync(string appKey, string postId, string userName, string password);
        Task<MetaPost> GetPostAsync(string postId, string userName, string password, string rootUrl);
        Task<List<MetaPost>> GetRecentPostsAsync(string blogId, string userName, string password, int numberOfPosts, string rootUrl);
    }
}
