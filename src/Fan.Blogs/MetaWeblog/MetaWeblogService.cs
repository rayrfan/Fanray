using Fan.Blogs.Enums;
using Fan.Blogs.Helpers;
using Fan.Blogs.Models;
using Fan.Blogs.Services;
using Fan.Medias;
using Fan.Models;
using Fan.Settings;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fan.Blogs.MetaWeblog
{
    public class MetaWeblogService : IMetaWeblogService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IBlogService _blogSvc;
        private readonly ISettingService _settingSvc;
        private readonly IMediaService _mediaSvc;
        private readonly ILogger<MetaWeblogService> _logger;

        public MetaWeblogService(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IBlogService blogSvc,
            ISettingService settingService,
            IMediaService mediaSvc,
            ILogger<MetaWeblogService> logger)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _blogSvc = blogSvc;
            _settingSvc = settingService;
            _mediaSvc = mediaSvc;
            _logger = logger;
        }

        // -------------------------------------------------------------------- Posts

        public async Task<string> NewPostAsync(string blogid, string userName, string password, MetaPost post, bool publish)
        {
            await EnsureValidUserAsync(userName, password);

            try
            {
                var blogPost = new BlogPost
                {
                    UserId = (await _userManager.FindByNameAsync(userName)).Id,
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
            await EnsureValidUserAsync(userName, password);

            try
            {
                var blogPost = new BlogPost
                {
                    Id = Convert.ToInt32(postId),
                    UserId = (await _userManager.FindByNameAsync(userName)).Id,
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
            await EnsureValidUserAsync(userName, password);

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
            await EnsureValidUserAsync(userName, password);

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
            await EnsureValidUserAsync(userName, password);

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
            await EnsureValidUserAsync(userName, password);

            try
            {
                var cats = await _blogSvc.GetCategoriesAsync();
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
            await EnsureValidUserAsync(userName, password);

            try
            {
                var cat = await _blogSvc.CreateCategoryAsync(new Category { Title = name });

                return cat.Id;
            }
            catch (Exception ex)
            {
                throw new MetaWeblogException(EMetaWeblogCode.CreateCategory, ex.Message);
            }
        }

        public async Task<List<string>> GetKeywordsAsync(string blogId, string userName, string password)
        {
            await EnsureValidUserAsync(userName, password);

            try
            {
                var keywords = new List<string>();
                var tags = await _blogSvc.GetTagsAsync();
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
            await EnsureValidUserAsync(userName, password);

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
            await EnsureValidUserAsync(userName, password);

            try
            {
                var userId = (await _userManager.FindByNameAsync(userName)).Id;
                var media = await _mediaSvc.UploadImageAsync(mediaObject.Bits, EAppType.Blog, userId, mediaObject.Name, EUploadedFrom.MetaWeblog);

                var appName = media.AppType.ToString().ToLowerInvariant();
                var year = media.UploadedOn.Year.ToString();
                var month = media.UploadedOn.Month.ToString("d2");
                var fileName = media.FileName;
                var url = $"{MediaService.IMAGE_HANDLER_PATH}/{appName}/original/{userId}/{year}/{month}/{fileName}";

                return new MetaMediaInfo()
                {
                    Url = url
                };
            }
            catch (Exception ex)
            {
                throw new MetaWeblogException(EMetaWeblogCode.NewMediaObject, ex.Message);
            }
        }

        // -------------------------------------------------------------------- Private helpers

        /// <summary>
        /// Ensures user is valid by sign in, throws <see cref="MetaWeblogException"/> if sign in fails.
        /// </summary>
        /// <param name="userName"></param>
        /// <param name="password"></param>
        /// <returns></returns>
        private async Task EnsureValidUserAsync(string userName, string password)
        {
            if (!await _signInManager.CanSignInAsync(new User { UserName = userName, PasswordHash = password }))
            {
                throw new MetaWeblogException(EMetaWeblogCode.AuthenticationFailed, "User sign in failed.");
            }
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
                Link = rootUrl + string.Format("/" + BlogRoutes.POST_RELATIVE_URL_TEMPLATE, blogPost.CreatedOn.Year, blogPost.CreatedOn.Month, blogPost.CreatedOn.Day, blogPost.Slug),
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
                return settings.AllowCommentsOnBlogPost ? ECommentStatus.AllowComments : ECommentStatus.NoComments;
            }
            else if (commentPolicy == "1")
            {
                return ECommentStatus.AllowComments;
            }

            return ECommentStatus.NoComments; // "0", "2"
        }
    }
}
