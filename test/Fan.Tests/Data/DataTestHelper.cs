using Fan.Data;
using Fan.Enums;
using Fan.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;

namespace Fan.Tests.Data
{
    /// <summary>
    /// Helps with the creation of <see cref="FanDbContext"/> with SQLite in memory database and
    /// EF Core InMemory Provider, as well as seeding initial blog data that some of the tests 
    /// depend on. 
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
    public static class DataTestHelper
    {
        /// <summary>
        /// Returns <see cref="FanDbContext"/> with SQLite Database Provider in-memory mode.
        /// </summary>
        public static FanDbContext GetContextWithSqlite()
        {
            var connection = new SqliteConnection() { ConnectionString = "Data Source=:memory:" };
            connection.Open(); 

            var builder = new DbContextOptionsBuilder<FanDbContext>();
            builder.UseSqlite(connection);

            var context = new FanDbContext(builder.Options);
            context.Database.EnsureCreated();

            return context;
        }

        /// <summary>
        /// Returns <see cref="FanDbContext"/> with Entity Framework Core In-Memory Database.
        /// </summary>
        public static FanDbContext GetContextWithEFCore()
        {
            var _options = new DbContextOptionsBuilder<FanDbContext>().UseInMemoryDatabase("FanInMemDb").Options;
            return new FanDbContext(_options);
        }

        public const string POST_SLUG = "test-post";
        public const string CAT_TITLE = "Technology";
        public const string CAT_SLUG = "tech";
        public const string TAG1_TITLE = "asp.net";
        public const string TAG2_TITLE = "c#";
        public const string TAG1_SLUG = "aspnet";
        public const string TAG2_SLUG = "cs";

        /// <summary>
        /// Seeds 1 blog post associated with 1 category and 2 tags.
        /// </summary>
        /// <param name="db"></param>
        public static void SeedTestPost(this FanDbContext db)
        {
            db.Metas.Add(new Meta { Key = "BlogSettings", Value = JsonConvert.SerializeObject(new BlogSettings()) });
            db.Posts.Add(GetPost());
            db.SaveChanges();
        }

        /// <summary>
        /// Seeds a specified number of posts, even number posts are drafts and tagged with tag2, 
        /// while odd number posts are published and tagged with tag1.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="numOfPosts"></param>
        public static void SeedTestPosts(this FanDbContext db, int numOfPosts)
        {
            db.Metas.Add(new Meta { Key = "BlogSettings", Value = JsonConvert.SerializeObject(new BlogSettings()) });
            db.Posts.AddRange(GetPosts(numOfPosts));
            db.SaveChanges();
        }

        /// <summary>
        /// Returns a post associated with 1 category and 2 tags.
        /// </summary>
        private static Post GetPost()
        {
            var cat = new Category { Slug = CAT_SLUG, Title = CAT_TITLE };
            var tag1 = new Tag { Slug = TAG1_SLUG, Title = TAG1_TITLE };
            var tag2 = new Tag { Slug = TAG2_SLUG, Title = TAG2_TITLE };

            var post = new Post
            {
                Body = "A post body.",
                Category = cat,
                UserName = "ray",
                CreatedOn = (new DateTime(2017, 01, 01)).ToUniversalTime(), 
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
        private static List<Post> GetPosts(int numOfPosts)
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
                    UserName = "ray",
                    CreatedOn = new DateTime(2017, 01, i), // be aware this is UTC time
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
