using Fan.Blog.Enums;
using Fan.Blog.Models;
using Fan.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fan.Blog.Data
{
    /// <summary>
    /// Sql implementation of the <see cref="IPostRepository"/> contract.
    /// </summary>
    public class SqlPostRepository : EntityRepository<Post>, IPostRepository
    {
        private readonly FanDbContext _db;
        public SqlPostRepository(FanDbContext db) : base(db)
        {
            _db = db;
        }

        /// <summary>
        /// Creates a <see cref="Post"/>.
        /// </summary>
        /// <param name="post">The post to create.</param>
        /// <param name="categoryTitle">The category title is available when called from metaweblog.</param>
        /// <param name="tagTitles">A list of tag titles associated with the post.</param>
        /// <returns>
        /// The inserted post with id.
        /// </returns>
        public async Task<Post> CreateAsync(Post post, string categoryTitle, IEnumerable<string> tagTitles)
        {
            // Category
            if (!categoryTitle.IsNullOrEmpty())
            {
                // cat title present, olw and setup
                post.Category = _db.Set<Category>().First(c => c.Title.ToUpper() == categoryTitle.ToUpper());
            }
            else if (!post.CategoryId.HasValue) // from browser CategoryId will have value
            {
                // from metaweblog with no cat inputted, give it the default cat id
                post.CategoryId =                     
                    Convert.ToInt32(_db.Set<Meta>().First(m => m.Key.Equals("blogsettings.defaultcategoryid")).Value);
            }

            // PostTags
            if (!tagTitles.IsNullOrEmpty())
            {
                // make sure list has no empty strings and only unique values
                tagTitles = tagTitles.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct();

                var tags = _db.Set<Tag>(); // all tags
                foreach (var title in tagTitles)
                {
                    // lookup the tag (any new tag is already created prior)
                    var tag = tags.First(t => t.Title.ToUpper() == title.ToUpper());
                    post.PostTags.Add(new PostTag { Post = post, Tag = tag });
                }
            }

            await _entities.AddAsync(post);
            await _db.SaveChangesAsync();
            return post;
        }

        /// <summary>
        /// Updates a <see cref="Post"/>.
        /// </summary>
        /// <param name="post">The post to update.</param>
        /// <param name="categoryTitle">The category title of the blog post input.</param>
        /// <param name="tagTitles">A list of tag titles associated with the post.</param>
        public async Task UpdateAsync(Post post, string categoryTitle, IEnumerable<string> tagTitles)
        {
            // Category
            if (!categoryTitle.IsNullOrEmpty()) // if cat title has value
            {
                // from metaweblog with a cat inputted
                post.Category = _db.Set<Category>().First(c => c.Title.ToUpper() == categoryTitle.ToUpper());
                post.CategoryId = post.Category.Id;
            }

            // PostTags
            if (!tagTitles.IsNullOrEmpty())
            {
                // make sure list has no empty strings and only unique values, olw passes empty string when no tags are given
                tagTitles = tagTitles.Where(s => !string.IsNullOrWhiteSpace(s)).Distinct();

                var currentTitles = post.PostTags.Select(pt => pt.Tag.Title);
                var titlesToRemove = currentTitles.Except(tagTitles).ToList();
                foreach (var title in titlesToRemove)
                {
                    post.PostTags.Remove(post.PostTags.Single(pt => pt.Tag.Title == title));
                }

                var tags = _db.Set<Tag>(); // all tags
                var titlesToAdd = tagTitles.Except(currentTitles);
                foreach (var title in titlesToAdd)
                {
                    var tag = tags.First(t => t.Title.ToUpper() == title.ToUpper());
                    post.PostTags.Add(new PostTag { PostId = post.Id, TagId = tag.Id });
                }
            }

            await _db.SaveChangesAsync();
        }

        /// <summary>
        /// Deletes a <see cref="Post"/> by Id, if the post is a parent page, 
        /// it will also delete all child pages.
        /// </summary>
        public new async Task DeleteAsync(int id)
        {
            // throws if id not found or not unique
            var post = await _entities.SingleAsync(c => c.Id == id);

            // if blog post or child page
            if (post.Type == EPostType.BlogPost || (post.ParentId.HasValue && post.ParentId > 0))
            {
                _db.Remove(post);
            }
            else // parent page which may have children
            {
                var posts = _entities.Where(p => p.ParentId == id).ToArray();
                Array.Resize(ref posts, posts.Length + 1); // put the parent itself in
                posts[posts.Length - 1] = post;
                
                _db.RemoveRange(posts);
            }

            await _db.SaveChangesAsync();
        }

        /// <summary>
        /// Increases post view count.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="count"></param>
        /// <returns></returns>
        public async Task IncViewCountAsync(int id, int count)
        {
            var post = await _entities.SingleOrDefaultAsync(p => p.Id == id);
            post.ViewCount += count;
            await _db.SaveChangesAsync();
        }

        /// <summary>
        /// Returns a <see cref="Post"/> by id. If it is a BlogPost it'll return together with its 
        /// <see cref="Category"/> and <see cref="Tag"/>. Returns null if it's not found.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="type">If it's BlogPost it'll return category and tags with it.</param>
        public async Task<Post> GetAsync(int id, EPostType type)
        {
            return (type == EPostType.BlogPost) ?
                await _entities.Include(p => p.User).Include(p => p.Category).Include(p => p.PostTags).ThenInclude(p => p.Tag).SingleOrDefaultAsync(p => p.Id == id) :
                await _entities.Include(p => p.User).SingleOrDefaultAsync(p => p.Id == id);
        }

        /// <summary>
        /// Returns a <see cref="EPostStatus.Published"/> <see cref="Post"/>, returns null if it's not found.
        /// </summary>
        public async Task<Post> GetAsync(string slug, int year, int month, int day) =>
            isSqlite ?
                 _entities.Include(p => p.User).Include(p => p.Category).Include(p => p.PostTags).ThenInclude(p => p.Tag).ToList()
                                   .SingleOrDefault(p =>
                                     p.Type == EPostType.BlogPost &&
                                     p.Status == EPostStatus.Published &&
                                     p.Slug.ToUpper() == slug.ToUpper() &&
                                     p.CreatedOn.Year == year &&
                                     p.CreatedOn.Month == month &&
                                     p.CreatedOn.Day == day) :
                 await _entities.Include(p => p.User).Include(p => p.Category).Include(p => p.PostTags).ThenInclude(p => p.Tag)
                               .SingleOrDefaultAsync(p =>
                                 p.Type == EPostType.BlogPost &&
                                 p.Status == EPostStatus.Published &&
                                 p.Slug.ToUpper() == slug.ToUpper() &&
                                 p.CreatedOn.Year == year &&
                                 p.CreatedOn.Month == month &&
                                 p.CreatedOn.Day == day);

        /// <summary>
        /// Returns a list of posts and total post count by query or empty list if no posts found.
        /// </summary>
        /// <param name="query"></param>
        /// <returns></returns>
        public async Task<(IList<Post> posts, int totalCount)> GetListAsync(PostListQuery query)
        {
            IList<Post> posts = null;
            int skip = (query.PageIndex - 1) * query.PageSize;
            int take = query.PageSize;
            IQueryable<Post> q = (query.QueryType == EPostListQueryType.Pages || query.QueryType == EPostListQueryType.PagesWithChildren) ?
                _entities.Include(p => p.User).Where(p => p.Type == EPostType.Page) :
                _entities.Include(p => p.User).Include(p => p.Category).Include(p => p.PostTags).ThenInclude(p => p.Tag).Where(p => p.Type == EPostType.BlogPost);

            switch (query.QueryType)
            {
                case EPostListQueryType.BlogPosts:
                    q = q.Where(p => p.Status == EPostStatus.Published);
                    posts = await q.OrderByDescending(p => p.CreatedOn).Skip(skip).Take(take).ToListAsync();
                    break;
                case EPostListQueryType.BlogDrafts:
                    q = q.Where(p => p.Status == EPostStatus.Draft);
                    posts = await q.OrderByDescending(p => p.UpdatedOn).ToListAsync();
                    break;
                case EPostListQueryType.BlogPostsByCategory:
                    var cat = await _db.Set<Category>().FirstAsync(t => t.Slug == query.CategorySlug);
                    q = q.Where(p => p.CategoryId == cat.Id && p.Status == EPostStatus.Published);
                    posts = await q.OrderByDescending(p => p.CreatedOn).Skip(skip).Take(take).ToListAsync();
                    break;
                case EPostListQueryType.BlogPostsByTag:
                    var tag = await _db.Set<Tag>().FirstAsync(t => t.Slug == query.TagSlug);
                    q = from p in q
                        from pt in p.PostTags
                        where p.Id == pt.PostId &&
                        pt.TagId == tag.Id && p.Status == EPostStatus.Published 
                        select p;
                    posts = await q.OrderByDescending(p => p.CreatedOn).Skip(skip).Take(take).ToListAsync();
                    break;
                case EPostListQueryType.BlogPostsArchive:
                    q = (query.Month.HasValue && query.Month > 0) ?
                        q.Where(p => p.CreatedOn.Year == query.Year && p.CreatedOn.Month == query.Month && p.Status == EPostStatus.Published) :
                        q.Where(p => p.CreatedOn.Year == query.Year && p.Status == EPostStatus.Published);
                    posts = await q.OrderByDescending(p => p.CreatedOn).ToListAsync();
                    break;
                case EPostListQueryType.BlogPostsByNumber:
                    posts = await q.OrderByDescending(p => p.CreatedOn).Take(take).ToListAsync();
                    break;
                case EPostListQueryType.BlogPublishedPostsByNumber:
                    q = q.Where(p => p.Status == EPostStatus.Published);
                    posts = await q.OrderByDescending(p => p.CreatedOn).Take(take).ToListAsync();
                    break;
                case EPostListQueryType.Pages:
                    q = q.Where(p => p.ParentId == null || p.ParentId == 0);
                    posts = await q.OrderByDescending(p => p.CreatedOn).ToListAsync();
                    break;
                case EPostListQueryType.PagesWithChildren:
                    posts = await q.OrderByDescending(p => p.CreatedOn).ToListAsync();
                    break;
            }

            int postCount = await q.CountAsync();

            return (posts, totalCount: postCount);
        }

        /// <summary>
        /// Returns CreatedOn of all published blog posts, used for archives.
        /// </summary>
        /// <returns></returns>
        public async Task<IEnumerable<DateTime>> GetPostDateTimesAsync()
        {
            return await _entities.Where(p => p.Status == EPostStatus.Published && p.Type == EPostType.BlogPost)
                .OrderByDescending(p => p.CreatedOn)
                .Select(p => new DateTime(p.CreatedOn.Year, p.CreatedOn.Month, 1))
                .ToListAsync();
        }

        /// <summary>
        /// Returns total number of posts by each <see cref="EPostStatus"/>.
        /// </summary>
        public async Task<PostCount> GetPostCountAsync() => new PostCount
        {
            Published = await _entities.Where(p => p.Status == EPostStatus.Published && p.Type == EPostType.BlogPost).CountAsync(),
            Draft = await _entities.Where(p => p.Status == EPostStatus.Draft && p.Type == EPostType.BlogPost).CountAsync()
        };
    }
}
