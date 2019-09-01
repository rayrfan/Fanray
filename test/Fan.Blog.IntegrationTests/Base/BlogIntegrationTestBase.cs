using Fan.Blog.Enums;
using Fan.Blog.IntegrationTests.Helpers;
using Fan.Blog.Models;
using Fan.Data;
using Fan.IntegrationTests.Base;
using Fan.Medias;
using System;
using System.Collections.Generic;

namespace Fan.Blog.IntegrationTests.Base
{
    /// <summary>
    /// Base class for all blog integration tests, it seeds initial blog data.
    /// </summary>
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
        /// Seeds 1 user and returns its id.
        /// </summary>
        protected int Seed_1User()
        {
            var user = Actor.User;
            _db.Users.Add(user);
            _db.SaveChanges();
            return user.Id;
        }

        /// <summary>
        /// Seeds 1 blog post associated with 1 category and 2 tags.
        /// </summary>
        /// <param name="db"></param>
        protected void Seed_1BlogPost_with_1Category_2Tags()
        {
            _db.Set<Meta>().AddRange(GetMetas());
            _db.Users.Add(Actor.User);
            _db.Set<Post>().Add(GetBlogPost());
            _db.SaveChanges();
        }

        /// <summary>
        /// Seeds a specified number of posts, even number posts are drafts and tagged with tag2, 
        /// while odd number posts are published and tagged with tag1.
        /// </summary>
        /// <param name="db"></param>
        /// <param name="numOfPosts"></param>
        protected void Seed_N_BlogPosts(int numOfPosts)
        {
            _db.Set<Meta>().AddRange(GetMetas());
            _db.Users.Add(Actor.User);
            _db.Set<Post>().AddRange(GetBlogPosts(numOfPosts));
            _db.SaveChanges();
        }

        /// <summary>
        /// Seeds a published parent page and returns its id.
        /// </summary>
        /// <returns></returns>
        protected int Seed_1Page()
        {
            _db.Users.Add(Actor.User);
            var page = GetPage();
            _db.Set<Post>().Add(page);
            _db.SaveChanges();
            return page.Id;
        }

        protected void Seed_2_Parents_With_1_Child_Each()
        {
            _db.Users.Add(Actor.User);
            _db.Set<Post>().AddRange(GetPages());
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
        /// Returns some blog settings.
        /// </summary>
        /// <returns></returns>
        private List<Meta> GetMetas()
        {
            var metas = new List<Meta>
            {
                new Meta { Id = 1, Key = "blogsettings.defaultcategoryid", Value = "1" }
            };

            return metas;
        }

        /// <summary>
        /// Returns a post associated with 1 category and 2 tags.
        /// </summary>
        private Post GetBlogPost()
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
        private List<Post> GetBlogPosts(int numOfPosts)
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


        private List<Post> GetPages()
        {
            var list = new List<Post>();
            var parent1 = GetPage(1);
            parent1.Id = 1;

            var parent2 = GetPage(2);
            parent2.Id = 2;

            var child1 = GetPage(3);
            child1.Id = 3;
            child1.ParentId = 1;

            var child2 = GetPage(4);
            child2.Id = 4;
            child2.ParentId = 2;

            parent1.Toc = "- [[Test Page 1]] \n- [[Test Page 2]]";

            list.Add(parent1);
            list.Add(parent2);
            list.Add(child1);
            list.Add(child2);

            return list;
        }

        /// <summary>
        /// Returns a published parent page.
        /// </summary>
        /// <returns></returns>
        private Post GetPage(int num = 1)
        {
            return new Post
            {
                Title = "Page" + num,
                Slug = "page" + num,
                Body = "<h1>Test Page</h1>",
                BodyMark = "# Test Page",
                UserId = Actor.ADMIN_ID,
                CreatedOn = new DateTimeOffset(new DateTime(2017, 01, 01), new TimeSpan(-7, 0, 0)),
                ParentId = null,
                Type = EPostType.Page,
                Status = EPostStatus.Published,
            };
        }

    }
}
