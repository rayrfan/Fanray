using AutoMapper;
using Fan.Blog.Data;
using Fan.Blog.Enums;
using Fan.Blog.Helpers;
using Fan.Blog.Models;
using Fan.Blog.Services.Interfaces;
using Fan.Blog.Validators;
using Fan.Exceptions;
using Fan.Helpers;
using Fan.Navigation;
using Fan.Settings;
using Markdig;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

[assembly: InternalsVisibleTo("Fan.Blog.IntegrationTests")]
[assembly: InternalsVisibleTo("Fan.Blog.UnitTests")]

namespace Fan.Blog.Services
{
    public class PageService : IPageService, INavProvider
    {
        private readonly ISettingService settingService;
        private readonly IPostRepository postRepository;
        private readonly IDistributedCache cache;
        private readonly ILogger<PageService> logger;
        private readonly IMapper mapper;
        private readonly IMediator mediator;

        public PageService(
            ISettingService settingService,
            IPostRepository postRepository,
            IDistributedCache cache,
            ILogger<PageService> logger,
            IMapper mapper,
            IMediator mediator)
        {
            this.settingService = settingService;
            this.postRepository = postRepository;
            this.cache = cache;
            this.logger = logger;
            this.mapper = mapper;
            this.mediator = mediator;
        }

        // -------------------------------------------------------------------- consts

        public const string DUPLICATE_TITLE_MSG = "A page with same title exists, please choose a different one.";
        public const string DUPLICATE_SLUG_MSG = "Page slug generated from your title conflicts with another page, please choose a different title.";
        public const string RESERVED_SLUG_MSG = "Page title conflicts with reserved URL '{0}', please choose a different one.";

        /// <summary>
        /// A parent page slug cannot be one of these values since it's intended to be used right 
        /// after web root.
        /// </summary>
        public static string[] Reserved_Slugs = new string[] 
        {
            "admin", "account", "api", "app", "apps", "assets",
            "blog", "blogs",
            "denied",
            "feed", "feeds", "forum", "forums",
            "image", "images", "img",
            "login", "logout",
            "media",
            "plugin", "plugins", "post", "posts", "preview",
            "register", "rsd",
            "setup", "static",
            "theme", "themes",
            "user", "users",
            "widget", "widgets",
        };

        /// <summary>
        /// Page allows shorthand "[[]]" links.
        /// </summary>
        /// <remarks>
        /// The regex is from https://stackoverflow.com/q/26856867/32240 
        /// </remarks>
        public const string DOUBLE_BRACKETS = @"\[.*?\]]";

        // -------------------------------------------------------------------- public methods

        /// <summary>
        /// Creates a <see cref="Page"/>.
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        /// <exception cref="FanException">
        /// Thrown if page has an invalidate title or slug.
        /// </exception>
        public async Task<Page> CreateAsync(Page page)
        {
            // validate
            await EnsurePageTitleAsync(page);

            // convert
            var post = await ConvertToPostAsync(page, ECreateOrUpdate.Create);

            // create
            await postRepository.CreateAsync(post);

            return await GetAsync(post.Id);
        }

        /// <summary>
        /// Updates a <see cref="Page"/>.
        /// </summary>
        /// <param name="page"></param>
        /// <returns></returns>
        /// <exception cref="FanException">
        /// Thrown if page has an invalidate title or slug. Invalids page cache if page status is 
        /// published.
        /// </exception>
        public async Task<Page> UpdateAsync(Page page)
        {
            // validate
            await EnsurePageTitleAsync(page);

            // get orig post
            var origPost = await GetAsync(page.Id);

            // convert
            var post = await ConvertToPostAsync(page, ECreateOrUpdate.Update);

            // update
            await postRepository.UpdateAsync(post);

            // invalidate cache for published
            if (page.Status == EPostStatus.Published)
            {
                var key = await GetCacheKeyAsync(page.Id, origPost);
                await cache.RemoveAsync(key);
            }

            // raise nav updated event
            await mediator.Publish(new NavUpdated());

            return await GetAsync(post.Id);
        }

        /// <summary>
        /// Deletes a <see cref="Page"/>, if a parent has children they will be deleted as well.
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        public async Task DeleteAsync(int id)
        {
            // get key by id before deletion
            var key = await GetCacheKeyAsync(id);

            // delete
            await postRepository.DeleteAsync(id);

            // invalidate cache
            await cache.RemoveAsync(key);

            // raise nav deleted event
            await mediator.Publish(new NavDeleted { Id = id, Type = ENavType.Page });
        }

        /// <summary>
        /// Returns a <see cref="Page"/> by <paramref name="id"/>. If the page is a parent then its 
        /// children will also be returned if any, if the page is a child its siblings will also be 
        /// returned if any.
        /// </summary>
        /// <param name="id">The id of the page.</param>
        /// <exception cref="FanException">
        /// Thrown if page by <paramref name="id"/> is not found.
        /// </exception>
        /// <returns>
        /// A <see cref="Page"/> for composer to edit. The parent and children are tracked.
        /// </returns>
        public async Task<Page> GetAsync(int id)
        {
            var post = await QueryPostAsync(id);
            var page = mapper.Map<Post, Page>(post);

            if (page.IsParent) // fill in children
            {
                var childPosts = await postRepository.FindAsync(p => p.Type == EPostType.Page && p.ParentId == page.Id);
                if (childPosts != null)
                {
                    foreach (var childPost in childPosts)
                    {
                        page.Children.Add(mapper.Map<Post, Page>(childPost));
                    }
                }
            }
            else // fill in siblings
            {
                var parentPost = await QueryPostAsync(page.ParentId.Value);
                var parent = mapper.Map<Post, Page>(parentPost);
                var childPosts = await postRepository.FindAsync(p => p.Type == EPostType.Page && p.ParentId == parent.Id);
                if (childPosts != null)
                {
                    foreach (var childPost in childPosts)
                    {
                        parent.Children.Add(mapper.Map<Post, Page>(childPost));
                    }
                }

                page.Parent = parent; // set page's parent
            }

            return page;
        }

        /// <summary>
        /// Returns a <see cref="Page"/> by <paramref name="slugs"/>. If the page is a parent then its 
        /// children will also be returned if any, if the page is a child its siblings will also be 
        /// returned if any.
        /// </summary>
        /// <param name="slugs">The slugs that lead to the page.</param>
        /// <exception cref="FanException">
        /// Thrown if page by <paramref name="slugs"/> is not found or the page is a <see cref="EPostStatus.Draft"/>
        /// or its parent is a <see cref="EPostStatus.Draft"/>.
        /// </exception>
        /// <returns>A <see cref="Page"/> for public viewing.</returns>
        /// <remarks>
        /// Caches individual page instead of on <see cref="GetParentsAsync"/> as a bulk.
        /// </remarks>
        public async Task<Page> GetAsync(params string[] slugs)
        {
            if (slugs == null || slugs.Length <= 0)
            {
                throw new ArgumentNullException(nameof(slugs));
            }

            // caching
            var key = GetCacheKey(slugs[0]);
            var time = BlogCache.Time_ParentPage;

            if (slugs.Length > 1 && !slugs[1].IsNullOrEmpty()) // child page
            {
                key = GetCacheKey(slugs[0], slugs[1]);
                time = BlogCache.Time_ChildPage;
            }

            return await cache.GetAsync(key, time, async () =>
            {
                var parents = await GetParentsAsync(withChildren: true);

                // find slugs[0], throw if not found or draft, url encode takes care of url with foreign chars
                var page = parents.SingleOrDefault(p => p.Slug.Equals(WebUtility.UrlEncode(slugs[0]), StringComparison.CurrentCultureIgnoreCase));
                if (page == null || page.Status == EPostStatus.Draft)
                {
                    throw new FanException(EExceptionType.ResourceNotFound);
                }

                // page requested is a child, throw if not found or draft
                if (page.IsParent && slugs.Length > 1 && !slugs[1].IsNullOrEmpty())
                {
                    var child = page.Children.SingleOrDefault(p => p.Slug.Equals(slugs[1], StringComparison.CurrentCultureIgnoreCase));
                    if (child == null || child.Status == EPostStatus.Draft)
                    {
                        throw new FanException(EExceptionType.ResourceNotFound);
                    }

                    page = child;
                }

                return page;
            });
        }

        /// <summary>
        /// Returns all parent pages, when <paramref name="withChildren"/> is true their children are also returned.
        /// </summary>
        /// <param name="withChildren">True will return children with the parents.</param>
        /// <returns></returns>
        public async Task<IList<Page>> GetParentsAsync(bool withChildren = false)
        {
            var query = new PostListQuery(withChildren ? EPostListQueryType.PagesWithChildren : EPostListQueryType.Pages);
            var (posts, totalCount) = await postRepository.GetListAsync(query);

            // either all pages or just parents
            var pages = mapper.Map<IList<Post>, IList<Page>>(posts);

            if (!withChildren) return pages;

            var parents = pages.Where(p => p.IsParent);
            foreach (var parent in parents)
            {
                var children = pages.Where(p => p.ParentId == parent.Id);
                foreach (var child in children)
                {
                    child.Parent = parent;
                    parent.Children.Add(child);
                }
            }

            return parents.ToList();
        }

        /// <summary>
        /// Updates a parent page's navigation.
        /// </summary>
        /// <param name="pageId">The parent page id.</param>
        /// <param name="navMd">The navigation markdown.</param>
        /// <returns></returns>
        public async Task SaveNavAsync(int pageId, string navMd)
        {
            var post = await QueryPostAsync(pageId);
            post.Nav = navMd;
            await postRepository.UpdateAsync(post);

            // invalidate cache
            var key = await GetCacheKeyAsync(pageId, post);
            await cache.RemoveAsync(key);
        }

        public bool CanProvideNav(ENavType type) => type == ENavType.Page;

        public async Task<string> GetNavUrlAsync(int id)
        {
            var page = await GetAsync(id);
            return $"/{page.Slug}";
        }

        // -------------------------------------------------------------------- private methods 

        /// <summary>
        /// Returns a <see cref="Post"/> by a <see cref="Page"/> <paramref name="id"/> from data 
        /// source, throws <see cref="FanException"/> if not found, the returned post is tracked.
        /// </summary>
        /// <param name="id">A <see cref="Page"/> id.</param>
        /// <returns></returns>
        /// <exception cref="FanException">Thrown if post is not found.</exception>
        private async Task<Post> QueryPostAsync(int id)
        {
            var post = await postRepository.GetAsync(id, EPostType.Page);

            if (post == null)
            {
                throw new FanException(EExceptionType.ResourceNotFound, 
                    $"Page with id {id} is not found.");
            }

            return post;
        }

        /// <summary>
        /// Converts a <see cref="Page"/> to a <see cref="Post"/> for create or update.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="createOrUpdate">See <see cref="ECreateOrUpdate"/>.</param>
        /// <exception cref="FanException">
        /// Thrown if resulting <see cref="Post.Slug"/> from <see cref="Post.Title"/> is not unique.
        /// </exception>
        /// <remarks>
        /// This method massages a page's fields into post.
        /// </remarks>
        /// <returns>A <see cref="Post"/>.</returns>
        private async Task<Post> ConvertToPostAsync(Page page, ECreateOrUpdate createOrUpdate)
        {
            // Get post
            var post = (createOrUpdate == ECreateOrUpdate.Create) ? new Post() : await QueryPostAsync(page.Id);
            post.Type = EPostType.Page;

            // Parent id
            post.ParentId = page.ParentId;

            // CreatedOn
            if (createOrUpdate == ECreateOrUpdate.Create)
            {
                // post time will be min value if user didn't set a time
                post.CreatedOn = (page.CreatedOn <= DateTimeOffset.MinValue) ? DateTimeOffset.UtcNow : page.CreatedOn.ToUniversalTime();
            }
            else 
            {
                // get post.CreatedOn in local time
                var coreSettings = await settingService.GetSettingsAsync<CoreSettings>();
                var postCreatedOnLocal = post.CreatedOn.ToLocalTime(coreSettings.TimeZoneId);

                // user changed the post time 
                if (!postCreatedOnLocal.YearMonthDayEquals(page.CreatedOn))
                    post.CreatedOn = (page.CreatedOn <= DateTimeOffset.MinValue) ? post.CreatedOn : page.CreatedOn.ToUniversalTime();
            }

            // UpdatedOn (DraftSavedOn)
            post.UpdatedOn = DateTimeOffset.UtcNow;

            // Slug 
            var slug = SlugifyPageTitle(page.Title);
            await EnsurePageSlugAsync(slug, page);
            post.Slug = slug;

            // Title
            post.Title = page.Title;

            // Bodys
            string parentSlug = null;
            if (!page.IsParent)
            {
                var parent = await GetAsync(post.ParentId.Value);
                parentSlug = parent.Slug;
            }
            post.Body = FormatNavLinks(page.Body, page.IsParent ? slug : parentSlug);
            post.BodyMark = WebUtility.HtmlEncode(page.BodyMark); // decoded on the client

            // Excerpt TODO should I extract excerpt from body if user didn't put an excerpt?
            post.Excerpt = page.Excerpt;

            // UserId
            post.UserId = page.UserId;

            // Status & CommentStatus
            post.Status = page.Status;
            post.CommentStatus = ECommentStatus.NoComments;

            // PageLayout
            post.PageLayout = page.PageLayout ?? 1;

            logger.LogDebug(createOrUpdate + " Page: {@Post}", post);
            return post;
        }

        /// <summary>
        /// Returns cache key for a parent or child page.
        /// </summary>
        /// <param name="pageId"></param>
        /// <param name="parentId"></param>
        /// <returns></returns>
        private async Task<string> GetCacheKeyAsync(int pageId, Post post = null)
        {
            post = post ?? await QueryPostAsync(pageId);
            var key = string.Format(BlogCache.KEY_PAGE, post.Slug);

            if (post.ParentId.HasValue && post.ParentId.Value > 0) // child
            {
                var parentPost = await QueryPostAsync(post.ParentId.Value);
                key = string.Format(BlogCache.KEY_PAGE, post.Slug + "_" + parentPost.Slug);
            }

            return key;
        }

        private string GetCacheKey(string parentSlug, string childSlug = null) => 
            childSlug.IsNullOrEmpty() ?
                string.Format(BlogCache.KEY_PAGE, parentSlug) :
                string.Format(BlogCache.KEY_PAGE, parentSlug + "_" + childSlug);

        /// <summary>
        /// Ensures <see cref="Page"/> title is valid and throws <see cref="FanException"/> if validation fails.
        /// </summary>
        /// <param name="page">A <see cref="Page"/> to be created or updated.</param>
        /// <exception cref="FanException">Throws when validation fails</exception>
        /// <remarks>
        /// When comparing titles for duplicates, I use <see cref="StringComparison.InvariantCultureIgnoreCase"/> 
        /// <see cref="https://stackoverflow.com/a/72766/32240"/>, the goal is to prevent two different titles
        /// that could produce the same slug.
        /// </remarks>
        internal async Task EnsurePageTitleAsync(Page page)
        {
            if (page == null) throw new ArgumentNullException(nameof(page));

            // title empty on publish or over maxlen is not ok
            await page.ValidateTitleAsync();

            // title empty on draft is ok
            if (page.Title.IsNullOrEmpty())
            {
                return;
            }

            // duplicate title is not ok
            if (page.IsParent)
            {
                var parents = await GetParentsAsync();
                if (page.Id > 0)
                {
                    var parent = parents.SingleOrDefault(p => p.Id == page.Id);
                    if (parent != null) parents.Remove(parent); // remove itself if update
                }

                if (parents.Any(p => p.Title.Equals(page.Title, StringComparison.InvariantCultureIgnoreCase)))
                {
                    throw new FanException(DUPLICATE_TITLE_MSG);
                }
            }
            else // is a child then search its siblings
            {
                var parent = await GetAsync(page.ParentId.Value);
                if (page.Id > 0 && parent.HasChildren) // check page.Id > 0 to avoid a new page
                {
                    var child = parent.Children.Single(p => p.Id == page.Id);
                    if (child != null) parent.Children.Remove(child); // remove itself if update
                }

                if (parent.HasChildren &&
                    parent.Children.Any(p => p.Title.Equals(page.Title, StringComparison.InvariantCultureIgnoreCase)))
                {
                    throw new FanException(DUPLICATE_TITLE_MSG);
                }
            }
        }

        /// <summary>
        /// Ensures <see cref="Page"/> slug is valid, throws <see cref="FanException"/> if the 
        /// resulting slug is not unique.
        /// </summary>
        /// <param name="slug"></param>
        /// <param name="page"></param>
        /// <exception cref="FanException">Throws when validation fails</exception>
        /// <returns>The slug for a <see cref="Page"/>.</returns>
        /// <remarks>
        /// The page slug must be unique with their siblings (pages on the same level), and not one
        /// of the <see cref="Reserved_Slugs"/> values.
        /// </remarks>
        internal async Task EnsurePageSlugAsync(string slug, Page page)
        {
            if (slug.IsNullOrEmpty() || page == null) return;

            if (page.IsParent)
            {
                var parents = await GetParentsAsync();

                if (page.Id > 0) // remove self if update
                {
                    var parent = parents.SingleOrDefault(p => p.Id == page.Id);
                    if (parent != null) parents.Remove(parent); // remove itself if update
                }

                if (parents.Any(c => c.Slug == slug)) 
                {
                    throw new FanException(EExceptionType.DuplicateRecord, DUPLICATE_SLUG_MSG);
                }

                // parent needs to check reserved slugs
                if (Reserved_Slugs.Contains(slug))
                {
                    throw new FanException(EExceptionType.DuplicateRecord, string.Format(RESERVED_SLUG_MSG, slug));
                }
            }
            else
            {
                var parent = await GetAsync(page.ParentId.Value);
                if (page.Id > 0 && parent.HasChildren) // remove self if update
                {
                    var child = parent.Children.Single(p => p.Id == page.Id);
                    if (child != null) parent.Children.Remove(child); // remove itself if update
                }

                if (parent.HasChildren &&
                    parent.Children.Any(c => c.Slug == slug))
                {
                    throw new FanException(EExceptionType.DuplicateRecord, DUPLICATE_SLUG_MSG);
                }
            }
        }

        // -------------------------------------------------------------------- static methods 

        /// <summary>
        /// Converts navigation markdown into HTML.
        /// </summary>
        /// <param name="navMd">The navigation content in markdown.</param>
        /// <returns>
        /// The converted html or null if <paramref name="navMd"/> is null. 
        /// </returns>
        public static string NavMdToHtml(string navMd, string parentSlug)
        {
            if (navMd.IsNullOrEmpty()) return navMd;

            var matches = Regex.Matches(navMd, DOUBLE_BRACKETS);
            foreach (var match in matches)
            {
                var token = match.ToString();
                var text = token.Substring(2, token.Length - 4);

                var slug = Util.Slugify(text);
                if (!parentSlug.IsNullOrEmpty() && parentSlug != slug)
                {
                    slug = $"{parentSlug}/{slug}";
                }

                var link = $"[{text}](/{slug} \"{text}\")"; // [link](/uri "title")
                navMd = navMd.Replace(match.ToString(), link);
            }

            return Markdown.ToHtml(navMd);
        }

        /// <summary>
        /// Transforms "[[]]" links in post body into acutal links.
        /// </summary>
        /// <param name="body"></param>
        /// <param name="parentSlug"></param>
        /// <returns></returns>
        public static string FormatNavLinks(string body, string parentSlug)
        {
            if (body.IsNullOrEmpty()) return body;

            var matches = Regex.Matches(body, DOUBLE_BRACKETS);
            foreach (var match in matches)
            {
                var token = match.ToString();
                var text = token.Substring(2, token.Length - 4);

                var slug = Util.Slugify(text);
                if (!parentSlug.IsNullOrEmpty() && parentSlug != slug)
                {
                    slug = $"{parentSlug}/{slug}";
                }

                var link = $"[{text}](/{slug} \"{text}\")"; // [link](/uri "title")
                var linkHtml = Markdown.ToHtml(link);
                if (linkHtml.StartsWith("<p>"))
                {
                    linkHtml = linkHtml.Replace("<p>", "").Replace("</p>", "");
                }
                body = body.Replace(match.ToString(), linkHtml);
            }

            return body;
        }

        /// <summary>
        /// Returns slug based on page title.
        /// </summary>
        /// <param name="title"></param>
        /// <returns></returns>
        public static string SlugifyPageTitle(string title)
        {
            if (title.IsNullOrEmpty()) return title;

            var slug = Util.Slugify(title, maxlen: PostTitleValidator.TITLE_MAXLEN);
            if (slug.IsNullOrEmpty())
            {
                slug = WebUtility.UrlEncode(title);
                if (slug.Length > PostTitleValidator.TITLE_MAXLEN)
                {
                    slug = slug.Substring(0, PostTitleValidator.TITLE_MAXLEN);
                }
            }

            return slug;
        }
    }
}
