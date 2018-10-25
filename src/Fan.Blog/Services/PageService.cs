using AutoMapper;
using Fan.Blog.Data;
using Fan.Blog.Enums;
using Fan.Blog.Models;
using Fan.Blog.Services.Interfaces;
using Fan.Exceptions;
using Fan.Helpers;
using Fan.Settings;
using FluentValidation.Results;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace Fan.Blog.Services
{
    /// <summary>
    /// Work in progress not ready.
    /// </summary>
    public class PageService : IPageService
    {
        private readonly IPostRepository _postRepo;
        private readonly ISettingService _settingSvc;
        private readonly IMapper _mapper;

        public PageService(
            IPostRepository postRepo,
            ISettingService settingService,
            IMapper mapper
            )
        {
            _postRepo = postRepo;
            _settingSvc = settingService;
            _mapper = mapper;
        }

        // -------------------------------------------------------------------- consts

        public const int TITLE_MAXLEN = 256;

        /// <summary>
        /// Slug cannot be one of these values if it's intended to be used right after web root.
        /// </summary>
        /// <remarks>
        /// Currently I decided not to apply these to category after reviewing WD is using
        /// /category/{catSlug}/ for all categories.
        /// </remarks>
        public static string[] Reserved_Slugs = new string[] 
        {
            "admin", "api", "account", "app","apps", "user", "users"
        };

        // -------------------------------------------------------------------- public methods

        public async Task<Page> CreatePageAsync(Page page)
        {
            if (page == null) return page;
            var post = await this.PrepPostAsync(page, ECreateOrUpdate.Create);
            await _postRepo.CreateAsync(post);

            return await this.GetPageAsync(page.Id);
        }

        public async Task<Page> UpdatePageAsync(Page page)
        {
            if (page == null) return page;
            var post = await this.PrepPostAsync(page, ECreateOrUpdate.Update);
            await _postRepo.UpdateAsync(page);

            return await this.GetPageAsync(page.Id);
        }

        public async Task DeletePageAsync(int id)
        {
            await _postRepo.DeleteAsync(id);
        }

        public async Task<Page> GetPageAsync(int id)
        {
            var post = await QueryPageAsync(id);
            if (post == null) throw new FanException("Blog post not found.");
            return await this.PrepPageAsync(post);
        }

        public async Task<Page> GetPageAsync(params string[] slugs)
        {
            // todo caching
            if (slugs == null || slugs.Length <= 0) return null;

            Page page = null;
            var rootPage = await GetRootPageWithChildrenAsync(slugs[0]);

            var childPages = rootPage.Children;
            for (int i = 1; i < slugs.Length; i++)
            {
                page = childPages.SingleOrDefault(p => p.Slug.Equals(slugs[i], StringComparison.CurrentCultureIgnoreCase));
                if (page == null) return null;
                childPages = page.Children;
            }

            return page;
        }

        public async Task<Page> GetRootPageWithChildrenAsync(string slug)
        {
            var rootPages = await GetRootPagesAsync();
            var rootPage = rootPages.SingleOrDefault(p => p.Slug.Equals(slug, StringComparison.CurrentCultureIgnoreCase));
            var query = new PostListQuery(EPostListQueryType.ChildPagesForRoot, rootPage.Id);
            var pages = await this.QueryPagesAsync(query);

            foreach (var page in pages)
            {
                var childPages = pages.Where(p => p.ParentId == page.Id);
                if (childPages != null)
                {
                    page.Children = childPages.ToList();
                    pages.RemoveAll(p => p.ParentId == page.Id);
                }
            }

            rootPage.Children = pages;

            return rootPage;
        }

        public async Task<Page> GetParentPageWithChildrenAsync(int id)
        {
            var parentPage = await this.GetPageAsync(id);
            var query = new PostListQuery(EPostListQueryType.ChildPagesForParent, id);
            var pages = await this.QueryPagesAsync(query);

            foreach (var page in pages)
            {
                parentPage.Children.Add(page);
            }

            return parentPage;
        }

        public async Task<List<Page>> GetRootPagesAsync()
        {
            PostListQuery query = new PostListQuery(EPostListQueryType.RootPages);
            return await this.QueryPagesAsync(query);
        }

        public async Task<List<Page>> GetRecentPagesAsync(int numberOfPages)
        {
            PostListQuery query =
                new PostListQuery(EPostListQueryType.PagesByNumber) { PageSize = numberOfPages };

            return await this.QueryPagesAsync(query);
        }

        // todo
        private async Task<Page> PrepPageAsync(Post post)
        {
            return null;
        }

        private async Task<Page> QueryPageAsync(int id)
        {
            return null;
        }

        /// <summary>
        /// Preps a page for create or update.
        /// </summary>
        /// <param name="page"></param>
        /// <param name="createOrUpdate"></param>
        /// <returns></returns>
        private async Task<Post> PrepPostAsync(Page page, ECreateOrUpdate createOrUpdate)
        {
            // validate
            var errMsg = "";
            if (page == null) errMsg = "Invalid page.";
            else if (page.Status != EPostStatus.Draft && page.Title.IsNullOrEmpty()) errMsg = "Page title cannot be empty.";
            else if (page.Title.Length > TITLE_MAXLEN) errMsg = $"Page title cannot exceed {TITLE_MAXLEN} chars.";
            if (!errMsg.IsNullOrEmpty()) throw new FanException(errMsg);

            // Get page
            var post = (createOrUpdate == ECreateOrUpdate.Create) ? new Post() : await this.GetPageAsync(page.Id); // throws if not found
            var blogSettings = await _settingSvc.GetSettingsAsync<BlogSettings>();

            // CreatedOn
            if (createOrUpdate == ECreateOrUpdate.Create)
            {
                // post time will be min value if user didn't set a time
                post.CreatedOn = (page.CreatedOn <= DateTimeOffset.MinValue) ? DateTimeOffset.UtcNow : page.CreatedOn.ToUniversalTime();
            }
            else if (post.CreatedOn != page.CreatedOn) // user changed in post time
            {
                post.CreatedOn = (page.CreatedOn <= DateTimeOffset.MinValue) ? post.CreatedOn : page.CreatedOn.ToUniversalTime();
            }

            // UpdatedOn
            if (page.Status == EPostStatus.Draft)
                post.UpdatedOn = page.CreatedOn;

            // Slug before Title
            post.Slug = string.IsNullOrEmpty(post.Slug) ?
                await this.FormatPageSlugAsync(page.Title, page.ParentId) :
                await this.FormatPageSlugAsync(page.Slug, page.ParentId);
            post.Title = WebUtility.HtmlEncode(page.Title);

            // Body
            post.Body = page.Body;

            // Status
            post.Status = page.Status;

            return post;
        }

        private async Task<List<Page>> QueryPagesAsync(PostListQuery query)
        {
            var results = await _postRepo.GetListAsync(query);
            var pages = _mapper.Map<List<Post>, List<Page>>(results.posts);
            return pages;
        }

        /// <summary>
        /// Returns a slug for a <see cref="Page"/>, the returned slug is unique and valid.
        /// </summary>
        /// <param name="input"></param>
        /// <param name="parentId"></param>
        /// <returns></returns>
        private async Task<string> FormatPageSlugAsync(string input, int? parentId)
        {
            // when input is the slug user inputted, it could exceed max len
            if (input.Length > TITLE_MAXLEN)
                input = input.Substring(0, TITLE_MAXLEN);

            var slug = Util.FormatSlug(input); // remove/replace odd char, lower case etc

            // slug from title could be empty, e.g. the title is in Chinese
            // then we generate a random string of 6 chars
            if (string.IsNullOrEmpty(slug))
                slug = Util.RandomString(8);

            if (!parentId.HasValue)
            {
                // then it's root page, we search against reserved words and dup
                var rootPages = await this.GetRootPagesAsync();
                int i = 2;
                while (rootPages.Any(c => c.Slug == slug) || Reserved_Slugs.Contains(slug))
                {
                    slug = $"{slug}-{i}";
                    i++;
                }
            }
            else
            {
                var parentPage = await this.GetParentPageWithChildrenAsync(parentId.Value);
                // search against dup siblings only
                int i = 2;
                while (parentPage.Children.Any(c => c.Slug == slug))
                {
                    slug = $"{slug}-{i}";
                    i++;
                }
            }

            return slug;
        }

    }
}
