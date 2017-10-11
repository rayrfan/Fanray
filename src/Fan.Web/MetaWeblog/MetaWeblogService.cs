using Fan.Enums;
using Fan.Helpers;
using Fan.Models;
using Fan.Services;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;

namespace Fan.Web.MetaWeblog
{
    public class MetaWeblogService : IMetaWeblogService
    {
        private readonly UserManager<User> _userManager;
        private readonly SignInManager<User> _signInManager;
        private readonly IBlogService _blogSvc;
        private readonly ISettingService _settingSvc;
        private readonly ILogger<MetaWeblogService> _logger;
        private readonly IHostingEnvironment _hostingEnvironment;

        public MetaWeblogService(
            UserManager<User> userManager,
            SignInManager<User> signInManager,
            IBlogService blogSvc,
            ISettingService settingService,
            ILogger<MetaWeblogService> logger,
            IHostingEnvironment env)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _blogSvc = blogSvc;
            _settingSvc = settingService;
            _logger = logger;
            _hostingEnvironment = env;
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
                    // Default / None / Close all will yield NoComments
                    CommentStatus = (!post.CommentPolicy.IsNullOrEmpty() && post.CommentPolicy == "0") ? ECommentStatus.NoComments : ECommentStatus.AllowComments,
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
                    CommentStatus = (!post.CommentPolicy.IsNullOrEmpty() && post.CommentPolicy == "0") ? ECommentStatus.NoComments : ECommentStatus.AllowComments,
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
                foreach (var p in recentPosts)
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
                        RssUrl = "", // todo
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
                var settings = await _settingSvc.GetSettingsAsync<SiteSettings>();
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
                // userId
                int userId = (await _userManager.FindByNameAsync(userName)).Id;

                // filename
                string mediaObjectName = mediaObject.Name.Replace(" ", "_").Replace(":", "-");
                var fileName = mediaObjectName.Substring(mediaObjectName.LastIndexOf('/') + 1);

                // save path 
                // because I'm unable to find a way to get the post date of the post into here, I'm not using the year/month folders
                //var uploads = $"uploads\\{DateTime.UtcNow.Year}\\{DateTime.UtcNow.Month.ToString("d2")}\\";
                var savePath = $"{Path.Combine(_hostingEnvironment.WebRootPath)}\\{Const.MEDIA_UPLOADS_FOLDER}\\"; // "wwwroot\uploads\"

                // make sure save path is there
                if (!Directory.Exists(savePath))
                    Directory.CreateDirectory(savePath);

                // file path
                var filePath = Path.Combine(savePath, fileName);

                // make sure filename is unique
                // the problem with this is if user updates the post resizing the image, we'll end up with new files
                int i = 2;
                while (File.Exists(filePath))
                {
                    fileName = fileName.Insert(fileName.LastIndexOf('.'), $"_{i}");
                    filePath = Path.Combine(savePath, fileName);
                }

                // save file
                using (var targetStream = File.Create(filePath))
                using (MemoryStream stream = new MemoryStream(mediaObject.Bits))
                {
                    await stream.CopyToAsync(targetStream);
                }

                // create media to db
                await _blogSvc.UpsertMediaAsync(new Media
                {
                    UserId = userId,
                    Title = fileName,
                    Slug = fileName,
                    MimeType = MimeTypeMap.GetMimeType(Path.GetExtension(fileName)), // mediaObject.type
                    Status = EPostStatus.Published,
                });

                var mediaInfo = new MetaMediaInfo()
                {
                    Url = $"{Const.MEDIA_UPLOADS_FOLDER}/{fileName}"
                };

                return mediaInfo;
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
                Link = rootUrl + blogPost.RelativeLink,
                PostDate = blogPost.CreatedOn,
                PostId = blogPost.Id.ToString(),
                Publish = blogPost.Status == EPostStatus.Published,
                Slug = blogPost.Slug,
                Tags = blogPost.TagTitles,
                Title = blogPost.Title,
            };
        }
    }
}
