using Fan.Enums;
using Fan.Models;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fan.Data
{
    /// <summary>
    /// Sql implementation of the <see cref="IPostRepository"/> contract.
    /// </summary>
    public class SqlPostRepository : IPostRepository
    {
        private readonly FanDbContext _db;
        public SqlPostRepository(FanDbContext db)
        {
            _db = db;
        }

        /// <summary>
        /// Creates a new <see cref="Post"/>.
        /// </summary>
        /// <param name="post"></param>
        /// <returns></returns>
        public async Task<Post> CreateAsync(Post post)
        {
            await _db.Posts.AddAsync(post);
            await _db.SaveChangesAsync();
            return post;
        }

        /// <summary>
        /// Deletes a <see cref="Post"/> by Id, if the post is a root page, 
        /// it will also delete all child pages.
        /// </summary>
        public async Task DeleteAsync(int id)
        {
            // SingleAsync will throw if id is not found or not unique
            var post = await _db.Posts.SingleAsync(c => c.Id == id);

            // root page
            if (post.Type == EPostType.Page && post.RootId == 0)
            {
                var posts = _db.Posts.Where(po => po.RootId == id);
                _db.RemoveRange(posts);
            }
            else
            {
                _db.Remove(post);
            }

            await _db.SaveChangesAsync();
        }

        /// <summary>
        /// Returns a <see cref="Post"/> by id. If it is a BlogPost it'll return together with its 
        /// <see cref="Category"/> and <see cref="Tag"/>. Returns null if it's not found.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type">If it's BlogPost it'll return category and tags with it.</param>
        /// <exception cref="FanException">if post is not found.</exception>
        public async Task<Post> GetAsync(int id, EPostType type)
        {
            return (type == EPostType.BlogPost) ?
                await _db.Posts.Include(p => p.Category).Include(p => p.PostTags).ThenInclude(p => p.Tag).SingleOrDefaultAsync(p => p.Id == id) :
                await _db.Posts.SingleOrDefaultAsync(p => p.Id == id);
        }

        /// <summary>
        /// Returns a <see cref="Post"/> by slug. If it is a BlogPost it'll return together with its 
        /// <see cref="Category"/> and <see cref="Tag"/>. Returns null if it's not found.
        /// </summary>
        /// <param name="slug"></param>
        /// <param name="type">If it's BlogPost it'll return category and tags with it.</param>
        /// <returns></returns>
        public async Task<Post> GetAsync(string slug, EPostType type)
        {
            return (type == EPostType.BlogPost) ?
                await _db.Posts.Include(p => p.Category).Include(p => p.PostTags).ThenInclude(p => p.Tag)
                                  .SingleOrDefaultAsync(p =>
                                    p.Type == EPostType.BlogPost &&
                                    p.Status == EPostStatus.Published &&
                                    p.Slug.Equals(slug, StringComparison.CurrentCultureIgnoreCase)) :
                await _db.Posts.SingleOrDefaultAsync(p =>
                                    p.Type == type &&
                                    p.Status == EPostStatus.Published &&
                                    p.Slug.Equals(slug, StringComparison.CurrentCultureIgnoreCase));
        }

        /// <summary>
        /// Returns a <see cref="EPostStatus.Published"/> <see cref="Post"/>, returns null if it's not found.
        /// </summary>
        public async Task<Post> GetAsync(string slug, int year, int month, int day)
        {
            return await _db.Posts.Include(p => p.Category).Include(p => p.PostTags).ThenInclude(p => p.Tag)
                                  .SingleOrDefaultAsync(p =>
                                    p.Type == EPostType.BlogPost &&
                                    p.Status == EPostStatus.Published &&
                                    p.Slug.ToLower() == slug.ToLower() &&
                                    p.CreatedOn.Year == year &&
                                    p.CreatedOn.Month == month &&
                                    p.CreatedOn.Day == day);
        }

        /// <summary>
        /// Returns a list of posts and total post count by query or empty list if no posts found.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<(List<Post> posts, int totalCount)> GetListAsync(PostListQuery query)
        {
            List<Post> posts = null;
            int skip = (query.PageIndex - 1) * query.PageSize;
            int take = query.PageSize;
            IQueryable<Post> q = _db.Posts.Include(p => p.Category).Include(p => p.PostTags).ThenInclude(p => p.Tag);

            switch (query.QueryType)
            {
                case EPostListQueryType.BlogPosts:
                    q = q.Where(p => p.Status == EPostStatus.Published && p.Type == EPostType.BlogPost);
                    posts = await q.OrderByDescending(p => p.CreatedOn).Skip(skip).Take(take).ToListAsync();
                    break;
                case EPostListQueryType.BlogDrafts:
                    q = q.Where(p => p.Status == EPostStatus.Draft && p.Type == EPostType.BlogPost);
                    posts = await q.OrderByDescending(p => p.UpdatedOn).ToListAsync();
                    break;
                case EPostListQueryType.BlogPostsByCategory:
                    var cat = await _db.Categories.FirstAsync(t => t.Slug.Equals(query.CategorySlug, StringComparison.CurrentCultureIgnoreCase));
                    q = q.Where(p => p.CategoryId == cat.Id && p.Status == EPostStatus.Published && p.Type == EPostType.BlogPost);
                    posts = await q.OrderByDescending(p => p.CreatedOn).Skip(skip).Take(take).ToListAsync();
                    break;
                case EPostListQueryType.BlogPostsByTag:
                    var tag = await _db.Tags.FirstAsync(t => t.Slug.Equals(query.TagSlug, StringComparison.CurrentCultureIgnoreCase));
                    q = from p in q
                        from pt in p.PostTags
                        where p.Id == pt.PostId &&
                        pt.TagId == tag.Id && p.Status == EPostStatus.Published && p.Type == EPostType.BlogPost
                        select p;
                    posts = await q.OrderByDescending(p => p.CreatedOn).Skip(skip).Take(take).ToListAsync();
                    break;
                case EPostListQueryType.BlogPostsByNumber:
                    q = q.Where(p => p.Type == EPostType.BlogPost);
                    posts = await q.OrderByDescending(p => p.CreatedOn).Take(take).ToListAsync();
                    break;
                case EPostListQueryType.RootPages:
                    q = _db.Posts.Where(p => p.RootId == 0 && p.Status == EPostStatus.Published && p.Type == EPostType.Page);
                    posts = await q.OrderByDescending(p => p.CreatedOn).ToListAsync();
                    break;
                case EPostListQueryType.ChildPagesForRoot:
                    q = _db.Posts.Where(p => p.RootId == query.RootId && p.Status == EPostStatus.Published && p.Type == EPostType.Page);
                    posts = await q.OrderByDescending(p => p.CreatedOn).ToListAsync();
                    break;
                case EPostListQueryType.PageDrafts:
                    q = _db.Posts.Where(p => p.Status == EPostStatus.Draft && p.Type == EPostType.Page);
                    posts = await q.OrderByDescending(p => p.UpdatedOn).ToListAsync();
                    break;
                case EPostListQueryType.PagesByNumber:
                    q = _db.Posts.Where(p => p.Type == EPostType.Page);
                    posts = await q.OrderByDescending(p => p.CreatedOn).Take(take).ToListAsync();
                    break;
            }

            int postCount = await q.CountAsync();

            return (posts: posts, totalCount: postCount);
        }

        /// <summary>
        /// Updates a post.
        /// </summary>
        /// <param name="post">Not used just being returned.</param>
        /// <returns></returns>
        public async Task<Post> UpdateAsync(Post post)
        {
            await _db.SaveChangesAsync();
            return post;
        }
    }
}
