using Fan.Blog.IntegrationTests.Helpers;
using Fan.Blog.Enums;
using Fan.Blog.Models;
using Fan.IntegrationTests.Base;
using System;
using System.Collections.Generic;
using Fan.Medias;

namespace Fan.Blog.IntegrationTests.Base
{
    /// <summary>
    /// Base class for all blog integration tests.  It helps initialization of a in-memory based 
    /// BlogDbContext as well as seeding initial blog data that some of the tests depend on.
    /// </summary>
    /// <remarks>
    /// When it comes to test with an in-memory database, there are two choices, the 
    /// EF Core In-Memory Database Provider (Microsoft.EntityFrameworkCore.InMemory)
    /// or the SQLite Database Provider (Microsoft.EntityFrameworkCore.Sqlite) with the SQLite 
    /// in-memory mode. However EF Core provider does not enforce any integrity like a relational 
    /// database, for example, the Meta table cannot have duplicate keys, it doesn't enforce that.
    /// 
    /// For more info https://docs.microsoft.com/en-us/ef/core/miscellaneous/testing/index
    /// </remarks>
    public class BlogIntegrationTestBase : IntegrationTestBase
    {
        // -------------------------------------------------------------------- Seed data

        public const string POST_SLUG = "test-post";
        public const string CAT_TITLE = "Technology";
        public const string CAT_SLUG = "technology";
        public const string TAG1_TITLE = "asp.net";
        public const string TAG2_TITLE = "c#";
        public const string TAG1_SLUG = "aspnet";
        public const string TAG2_SLUG = "cs";

        /// <summary>
        /// Seeds 1 user.
        /// </summary>
        protected void SeedUser()
        {
            _db.Users.Add(Actor.User);
            _db.SaveChanges();
        }

        /// <summary>
        /// Seeds 1 blog post associated with 1 category and 2 tags.
        /// </summary>
        /// <param name="db"></param>
        protected void SeedTestPost()
        {
            _db.Users.Add(Actor.User);
            _db.Set<Post>().Add(GetPost());
            _db.SaveChanges();
        }

        /// <summary>
        /// Seeds a specified number of posts, even number posts are drafts and tagged with tag2, 
        /// while odd number posts are published and tagged with tag1.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="numOfPosts"></param>
        protected void SeedTestPosts(int numOfPosts)
        {
            _db.Users.Add(Actor.User);
            _db.Set<Post>().AddRange(GetPosts(numOfPosts));
            _db.SaveChanges();
        }

        /// <summary>
        /// Seeds images in Media table.
        /// </summary>
        /// <param name="filenameSlugged">The filename saved to media table, it's slugged with ext.</param>
        protected void SeedImages(string filenameSlugged)
        {
            _db.Users.Add(Actor.User);
            _db.Set<Media>().Add(new Media {
                Id = 1,
                AppType = EAppType.Blog,
                FileName = filenameSlugged,
                ContentType = "png",
                Height = 40,
                Length = 1000,
                MediaType = EMediaType.Image,
                UploadedFrom = EUploadedFrom.Browser,
                UploadedOn = DateTimeOffset.UtcNow,
                UserId = Actor.ADMIN_ID,
                Width = 40,
            });
            _db.SaveChanges();
        }

        // -------------------------------------------------------------------- private methods

        /// <summary>
        /// Returns a post associated with 1 category and 2 tags.
        /// </summary>
        private Post GetPost()
        {
            var cat = new Category { Slug = CAT_SLUG, Title = CAT_TITLE };
            var tag1 = new Tag { Slug = TAG1_SLUG, Title = TAG1_TITLE };
            var tag2 = new Tag { Slug = TAG2_SLUG, Title = TAG2_TITLE };

            var post = new Post
            {
                Body = "A post body.",
                Category = cat,
                UserId = Actor.ADMIN_ID,
                CreatedOn = new DateTimeOffset(new DateTime(2017, 01, 01), new TimeSpan(-7, 0, 0)),
                RootId = null,
                Title = "A published post",
                Slug = POST_SLUG,
                Type = EPostType.BlogPost,
                Status = EPostStatus.Published,
            };
            // this is outside because we are using post itself to create PostTag
            post.PostTags = new List<PostTag> {
                    new PostTag { Post = post, Tag = tag1 },
                    new PostTag { Post = post, Tag = tag2 },
                };

            return post;
        }

        /// <summary>
        /// Returns a specified number of posts, even number posts are drafts and tagged with tag2, 
        /// while odd number posts are published and tagged with tag1.
        /// </summary>
        /// <returns></returns>
        private List<Post> GetPosts(int numOfPosts)
        {
            if (numOfPosts < 1) throw new ArgumentException("Param numOfPosts must be > 1");

            var cat = new Category { Slug = CAT_SLUG, Title = CAT_TITLE };
            var tag1 = new Tag { Slug = TAG1_SLUG, Title = TAG1_TITLE };
            var tag2 = new Tag { Slug = TAG2_SLUG, Title = TAG2_TITLE };

            var list = new List<Post>();
            for (int i = 1; i <= numOfPosts; i++)
            {
                var post = new Post
                {
                    Body = $"A post body #{i}.",
                    Category = cat,
                    UserId = Actor.ADMIN_ID,
                    CreatedOn = new DateTimeOffset(new DateTime(2017, 01, i), new TimeSpan(-7, 0, 0)),
                    RootId = null,
                    Title = $"Test Post #{i}",
                    Slug = $"{POST_SLUG}-{i}",
                    Type = EPostType.BlogPost,
                    Status = (i % 2 == 0) ? EPostStatus.Draft : EPostStatus.Published, // drafts / published
                };

                if (i % 2 == 0)
                {
                    post.PostTags = new List<PostTag> { // posts tagged c#
                        new PostTag { Post = post, Tag = tag2 },
                    };
                }
                else
                {
                    post.PostTags = new List<PostTag> { // posts tagged asp.net
                        new PostTag { Post = post, Tag = tag1 },
                    };
                }

                list.Add(post);
            }

            return list;
        }
    }
}
