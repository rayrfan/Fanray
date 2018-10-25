using Fan.Blog.Enums;
using Fan.Blog.Helpers;
using Fan.Blog.Models;
using Fan.Blog.Services.Interfaces;
using Fan.Medias;
using Fan.Membership;
using Fan.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Fan.Blog.MetaWeblog
{
    public class MetaWeblogService : IMetaWeblogService
    {
        private readonly IUserService _userSvc;
        private readonly SignInManager<User> _signInManager;
        private readonly IBlogPostService _blogSvc;
        private readonly ICategoryService _catSvc;
        private readonly ITagService _tagSvc;
        private readonly IImageService _imgSvc;
        private readonly ISettingService _settingSvc;
        private readonly ILogger<MetaWeblogService> _logger;

        public MetaWeblogService(
            IUserService userService,
            SignInManager<User> signInManager,
            IBlogPostService blogSvc,
            ICategoryService catSvc,
            ITagService tagService,
            IImageService imgService,
            ISettingService settingService,
            ILogger<MetaWeblogService> logger)
        {
            _userSvc = userService;
            _signInManager = signInManager;
            _blogSvc = blogSvc;
            _catSvc = catSvc;
            _tagSvc = tagService;
            _imgSvc = imgService;
            _settingSvc = settingService;
            _logger = logger;
        }

        // -------------------------------------------------------------------- Posts

        public async Task<string> NewPostAsync(string blogid, string userName, string password, MetaPost post, bool publish)
        {
            var user = await ValidUserAsync(userName, password);

            try
            {
                var blogPost = new BlogPost
                {
                    UserId = user.Id,
                    Title = post.Title,
                    Slug = post.Slug,
                    Body = post.Description,
                    Excerpt = post.Excerpt,
                    CreatedOn = post.PostDate,
                    CategoryTitle = (post.Categories == null || post.Categories.Count <= 0) ? null : post.Categories[0],
                    TagTitles = post.Tags,
                    Status = publish ? EPostStatus.Published : EPostStatus.Draft,
                    CommentStatus = await GetPostCommentStatusAsync(post.CommentPolicy),
                };

                blogPost = await _blogSvc.CreatePostAsync(blogPost);

                return blogPost.Id.ToString();
            }
            catch (Exception ex)
            {
                throw new MetaWeblogException(EMetaWeblogCode.NewPost, ex.Message);
            }
        }

        public async Task<bool> EditPostAsync(string postId, string userName, string password, MetaPost post, bool publish)
        {
            var user = await ValidUserAsync(userName, password);

            try
            {
                var blogPost = new BlogPost
                {
                    Id = Convert.ToInt32(postId),
                    UserId = user.Id,
                    Title = post.Title,
                    Slug = post.Slug,
                    Body = post.Description,
                    Excerpt = post.Excerpt,
                    CreatedOn = post.PostDate,
                    CategoryTitle = (post.Categories == null || post.Categories.Count <= 0) ? null : post.Categories[0],
                    TagTitles = post.Tags,
                    Status = publish ? EPostStatus.Published : EPostStatus.Draft,
                    CommentStatus = await GetPostCommentStatusAsync(post.CommentPolicy),
                };

                await _blogSvc.UpdatePostAsync(blogPost);

                return true;
            }
            catch (Exception ex)
            {
                throw new MetaWeblogException(EMetaWeblogCode.EditPost, ex.Message);
            }
        }

        public async Task<bool> DeletePostAsync(string appKey, string postId, string userName, string password)
        {
            await ValidUserAsync(userName, password);

            try
            {
                var id = Convert.ToInt32(postId);
                await _blogSvc.DeletePostAsync(id);

                return true;
            }
            catch (Exception ex)
            {
                throw new MetaWeblogException(EMetaWeblogCode.DeletePost, ex.Message);
            }
        }

        public async Task<MetaPost> GetPostAsync(string postId, string userName, string password, string rootUrl)
        {
            await ValidUserAsync(userName, password);

            try
            {
                int id = Int32.Parse(postId);
                var post = await _blogSvc.GetPostAsync(id);

                return ToMetaPost(post, rootUrl);
            }
            catch (Exception ex)
            {
                throw new MetaWeblogException(EMetaWeblogCode.GetPost, ex.Message);
            }
        }

        public async Task<List<MetaPost>> GetRecentPostsAsync(string blogId, string userName, string password, int numberOfPosts, string rootUrl)
        {
            await ValidUserAsync(userName, password);

            try
            {
                var recentPosts = await _blogSvc.GetRecentPostsAsync(numberOfPosts);
                var posts = new List<MetaPost>();
                foreach (var p in recentPosts.Posts)
                {
                    posts.Add(ToMetaPost(p, rootUrl));
                }

                return posts;
            }
            catch (Exception ex)
            {
                throw new MetaWeblogException(EMetaWeblogCode.GetRecentPosts, ex.Message);
            }
        }

        // -------------------------------------------------------------------- Categories / Tags

        public async Task<List<MetaCategory>> GetCategoriesAsync(string blogId, string userName, string password, string rootUrl)
        {
            await ValidUserAsync(userName, password);

            try
            {
                var cats = await _catSvc.GetAllAsync();
                var metaCats = new List<MetaCategory>();

                foreach (var cat in cats)
                {
                    metaCats.Add(new MetaCategory
                    {
                        Description = cat.Title,
                        HtmlUrl = rootUrl + cat.RelativeLink,
                        Id = cat.Id.ToString(),
                        RssUrl = rootUrl + cat.RssRelativeLink,
                        Title = cat.Title,
                    });
                }

                return metaCats;
            }
            catch (Exception ex)
            {
                throw new MetaWeblogException(EMetaWeblogCode.GetCategories, ex.Message);
            }
        }

        /// <summary>
        /// Creates a new category. Note only when supportsNewCategoriesInline is false do we get
        /// a call to this operation, otherwise the new category is created as part of creating a
        /// post.
        /// </summary>
        /// <param name="name"></param>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        public async Task<int> CreateCategoryAsync(string name, string userName, string password)
        {
            await ValidUserAsync(userName, password);

            try
            {
                var cat = await _catSvc.CreateAsync(name);

                return cat.Id;
            }
            catch (Exception ex)
            {
                throw new MetaWeblogException(EMetaWeblogCode.CreateCategory, ex.Message);
            }
        }

        public async Task<List<string>> GetKeywordsAsync(string blogId, string userName, string password)
        {
            await ValidUserAsync(userName, password);

            try
            {
                var keywords = new List<string>();
                var tags = await _tagSvc.GetAllAsync();
                foreach (var tag in tags)
                {
                    keywords.Add(tag.Title);
                }

                keywords.Sort();
                return keywords;
            }
            catch (Exception ex)
            {
                throw new MetaWeblogException(EMetaWeblogCode.GetKeywords, ex.Message);
            }
        }

        // -------------------------------------------------------------------- Other

        public async Task<List<MetaBlogInfo>> GetUsersBlogsAsync(string appKey, string userName, string password, string rootUrl)
        {
            await ValidUserAsync(userName, password);

            try
            {
                var settings = await _settingSvc.GetSettingsAsync<CoreSettings>();
                var blogs = new List<MetaBlogInfo>();
                var blog = new MetaBlogInfo { Url = rootUrl, BlogName = settings.Title };
                blogs.Add(blog);

                return blogs;
            }
            catch (Exception ex)
            {
                throw new MetaWeblogException(EMetaWeblogCode.GetUsersBlogs, ex.Message);
            }
        }

        public async Task<MetaMediaInfo> NewMediaObjectAsync(string blogId, string userName, string password, MetaMediaObject mediaObject, HttpContext request)
        {
            var user = await ValidUserAsync(userName, password);

            try
            {
                var userId = user.Id;
                var media = await _imgSvc.UploadAsync(new MemoryStream(mediaObject.Bits), 
                    userId, mediaObject.Name, mediaObject.Type, EUploadedFrom.MetaWeblog);

                return new MetaMediaInfo()
                {
                    Url = _imgSvc.GetAbsoluteUrl(media, EImageSize.Original)
                };
            }
            catch (Exception ex)
            {
                throw new MetaWeblogException(EMetaWeblogCode.NewMediaObject, ex.Message);
            }
        }

        // -------------------------------------------------------------------- Private helpers

        /// <summary>
        /// Returns user if credential is valid, throws <see cref="MetaWeblogException"/> 
        /// if uer not found or sign in fails.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        private async Task<User> ValidUserAsync(string userName, string password)
        {
            var user = await _userSvc.FindByEmailOrUsernameAsync(userName);

            if (user != null &&
                await _signInManager.CanSignInAsync(new User { UserName = user.UserName, PasswordHash = password }))
            {
                return user;
            }

            throw new MetaWeblogException(EMetaWeblogCode.AuthenticationFailed, "User sign in failed.");
        }

        private MetaPost ToMetaPost(BlogPost blogPost, string rootUrl)
        {
            return new MetaPost
            {
                AuthorId = blogPost.UserId.ToString(),
                Categories = new List<string> { blogPost.CategoryTitle },
                CommentPolicy = (blogPost.CommentStatus == ECommentStatus.AllowComments ||
                                 blogPost.CommentStatus == ECommentStatus.AllowCommentsWithApproval) ? "1" : "0",
                Description = blogPost.Body,
                Excerpt = blogPost.Excerpt,
                PostDate = blogPost.CreatedOn,
                PostId = blogPost.Id.ToString(),
                Publish = blogPost.Status == EPostStatus.Published,
                Slug = blogPost.Slug,
                Tags = blogPost.TagTitles,
                Title = blogPost.Title,
                Link = rootUrl + BlogRoutes.GetPostRelativeLink(blogPost.CreatedOn, blogPost.Slug),
            };
        }

        /// <summary>
        /// Returns <see cref="ECommentStatus"/> by analyzing the 4 possible post comment policy values.
        /// </summary>
        /// <param name="commentPolicy"></param>
        /// <returns></returns>
        private async Task<ECommentStatus> GetPostCommentStatusAsync(string commentPolicy)
        {
            if (commentPolicy.IsNullOrEmpty())
            {
                var settings = await _settingSvc.GetSettingsAsync<BlogSettings>();
                return settings.AllowComments ? ECommentStatus.AllowComments : ECommentStatus.NoComments;
            }
            else if (commentPolicy == "1")
            {
                return ECommentStatus.AllowComments;
            }

            return ECommentStatus.NoComments; // "0", "2"
        }
    }
}
