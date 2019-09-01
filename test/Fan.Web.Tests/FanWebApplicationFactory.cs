using Fan.Blog.Enums;
using Fan.Blog.Models;
using Fan.Data;
using Fan.Membership;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;

namespace Fan.Web.Tests
{
    /// <summary>
    /// The base class for http client test, it seeds data using EF in-memory db.
    /// </summary>
    /// <typeparam name="TStartup"></typeparam>
    /// <remarks>
    /// https://docs.microsoft.com/en-us/aspnet/core/test/integration-tests?view=aspnetcore-2.2
    /// </remarks>
    public class FanWebApplicationFactory<TStartup>
        : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.ConfigureServices(services =>
            {
                // create a new service provider
                var serviceProvider = new ServiceCollection()
                    .AddEntityFrameworkInMemoryDatabase()
                    .BuildServiceProvider();

                // add FanDbContext using an in-memory database
                services.AddDbContext<FanDbContext>(options =>
                {
                    options.UseInMemoryDatabase("InMemoryDbForTesting");
                    options.UseInternalServiceProvider(serviceProvider);
                });

                // build the service provider
                var sp = services.BuildServiceProvider();

                // create a scope to obtain a reference to FanDbContext
                using (var scope = sp.CreateScope())
                {
                    var scopedServices = scope.ServiceProvider;
                    var dbCtx = scopedServices.GetRequiredService<FanDbContext>();
                    var logger = scopedServices.GetRequiredService<ILogger<FanWebApplicationFactory<TStartup>>>();

                    // ensure database is created
                    dbCtx.Database.EnsureCreated();

                    try
                    {
                        // seed the database with test data
                        Seed(dbCtx);
                    }
                    catch (Exception ex)
                    {
                        logger.LogError(ex, $"An error occurred seeding the database: {ex.Message}");
                    }
                }
            });
        }

        // consts for easy test verification of seeded values

        const int USER_ID = 1;
        const string USER_NAME = "ray";
        const string POST_SLUG = "test-post";
        readonly DateTime POST_DATE = new DateTime(2017, 01, 01);
        const string CAT_TITLE = "Technology";
        const string CAT_SLUG = "technology";
        const string TAG1_TITLE = "asp.net";
        const string TAG2_TITLE = "c#";
        const string TAG1_SLUG = "aspnet";
        const string TAG2_SLUG = "cs";

        /// <summary>
        /// Seed settings, user, post, category and tags.
        /// </summary>
        /// <param name="db"></param>
        private void Seed(FanDbContext db)
        {
            db.Set<Meta>().AddRange(GetSettings()); // settings
            db.Users.Add(GetUser()); // user
            db.Set<Post>().Add(GetPostWith1Category2Tags()); // post with category and tags
            db.Set<Post>().AddRange(GetPageWith1Child());
            db.SaveChanges();
        }

        /// <summary>
        /// Returns some settings.
        /// </summary>
        /// <remarks>
        /// If you added new widget area in <see cref="Fan.Widgets.WidgetService"/> and used it in 
        /// a view, you have to add them here or the razor view will fail and give you 500.
        /// </remarks>
        private List<Meta> GetSettings()
        {
            var metas = new List<Meta>
            {
                new Meta { Id = 1, Key = "coresettings.title", Value = "Fanray", Type = EMetaType.Setting },
                new Meta { Id = 2, Key = "coresettings.tagline", Value = "A fanray blog", Type = EMetaType.Setting },
                new Meta { Id = 3, Key = "coresettings.theme", Value = "Clarity", Type = EMetaType.Setting },
                new Meta { Id = 4, Key = "coresettings.timezoneid", Value = "Pacific Standard Time", Type = EMetaType.Setting },
                new Meta { Id = 5, Key = "coresettings.googleanalyticstrackingid", Value = "", Type = EMetaType.Setting },
                new Meta { Id = 6, Key = "coresettings.setupdone", Value = "True", Type = EMetaType.Setting },
                new Meta { Id = 7, Key = "blogsettings.postperpage", Value = "10", Type = EMetaType.Setting },
                new Meta { Id = 8, Key = "blogsettings.defaultcategoryid", Value = "1", Type = EMetaType.Setting },
                new Meta { Id = 9, Key = "blogsettings.postlistdisplay", Value = "FullBody", Type = EMetaType.Setting },
                new Meta { Id = 10, Key = "blogsettings.allowcomments", Value = "True", Type = EMetaType.Setting },
                new Meta { Id = 11, Key = "blogsettings.commentprovider", Value = "Disqus", Type = EMetaType.Setting },
                new Meta { Id = 12, Key = "blogsettings.disqusshortname", Value = "", Type = EMetaType.Setting },
                new Meta { Id = 13, Key = "blogsettings.feedshowexcerpt", Value = "False", Type = EMetaType.Setting },
                new Meta { Id = 14, Key = "blog-sidebar1", Value = "{\"id\":\"blog-sidebar1\",\"widgetIds\":[]}", Type = EMetaType.WidgetAreaBySystem },
                new Meta { Id = 15, Key = "blog-sidebar2", Value = "{ \"id\":\"blog-sidebar2\",\"widgetIds\":[]}", Type = EMetaType.WidgetAreaBySystem },
                new Meta { Id = 16, Key = "blog-before-post", Value = "{ \"id\":\"blog-before-post\",\"widgetIds\":[]}", Type = EMetaType.WidgetAreaBySystem },
                new Meta { Id = 17, Key = "blog-after-post", Value = "{ \"id\":\"blog-after-post\",\"widgetIds\":[]}", Type = EMetaType.WidgetAreaBySystem },
                new Meta { Id = 18, Key = "blog-before-post-list", Value = "{ \"id\":\"blog-before-post-list\",\"widgetIds\":[]}", Type = EMetaType.WidgetAreaBySystem },
                new Meta { Id = 19, Key = "blog-after-post-list", Value = "{ \"id\":\"blog-after-post-list\",\"widgetIds\":[]}", Type = EMetaType.WidgetAreaBySystem },
                new Meta { Id = 20, Key = "footer1", Value = "{ \"id\":\"footer1\",\"widgetIds\":[]}", Type = EMetaType.WidgetAreaBySystem },
                new Meta { Id = 21, Key = "footer2", Value = "{ \"id\":\"footer2\",\"widgetIds\":[]}", Type = EMetaType.WidgetAreaBySystem },
                new Meta { Id = 22, Key = "footer3", Value = "{ \"id\":\"footer3\",\"widgetIds\":[]}", Type = EMetaType.WidgetAreaBySystem },
                new Meta { Id = 23, Key = "page-sidebar1", Value = "{ \"id\":\"page-sidebar1\",\"widgetIds\":[]}", Type = EMetaType.WidgetAreaBySystem },
                new Meta { Id = 24, Key = "page-sidebar2", Value = "{ \"id\":\"page-sidebar2\",\"widgetIds\":[]}", Type = EMetaType.WidgetAreaBySystem },
                new Meta { Id = 25, Key = "page-before-content", Value = "{ \"id\":\"page-before-content\",\"widgetIds\":[]}", Type = EMetaType.WidgetAreaBySystem },
                new Meta { Id = 26, Key = "page-after-content", Value = "{ \"id\":\"page-after-content\",\"widgetIds\":[]}", Type = EMetaType.WidgetAreaBySystem },
                new Meta { Id = 27, Key = "clarity", Value = "", Type = EMetaType.Theme },
                new Meta { Id = 28, Key = "clarity-my-area", Value = "{\"id\":\"my-area\",\"widgetIds\":[]}", Type = EMetaType.WidgetAreaByTheme },
            };

            return metas;
        }

        /// <summary>
        /// Returns a user.
        /// </summary>
        /// <returns></returns>
        private User GetUser()
        {
            return new User { Id = USER_ID, UserName = USER_NAME, DisplayName = "Ray Fan", Email = "admin@email.com" };
        }

        /// <summary>
        /// Returns a post associated with 1 category and 2 tags.
        /// </summary>
        private Post GetPostWith1Category2Tags()
        {
            var cat = new Category { Slug = CAT_SLUG, Title = CAT_TITLE };
            var tag1 = new Tag { Slug = TAG1_SLUG, Title = TAG1_TITLE };
            var tag2 = new Tag { Slug = TAG2_SLUG, Title = TAG2_TITLE };

            var post = new Post
            {
                Id = 1,
                Body = "A post body.",
                Category = cat,
                UserId = USER_ID,
                CreatedOn = new DateTimeOffset(POST_DATE), 
                Title = "A published post",
                Slug = POST_SLUG,
                Type = EPostType.BlogPost,
                Status = EPostStatus.Published,
            };

            post.PostTags = new List<PostTag> {
                    new PostTag { Post = post, Tag = tag1 },
                    new PostTag { Post = post, Tag = tag2 },
                };

            return post;
        }

        private List<Post> GetPageWith1Child()
        {
            var list = new List<Post>();

            var parent = new Post
            {
                Id = 2,
                ParentId = 0,
                Title = "About",
                Slug = "about",
                Body = "<h1>About Page</h1>",
                BodyMark = "# About",
                UserId = USER_ID,
                CreatedOn = new DateTimeOffset(new DateTime(2017, 01, 01), new TimeSpan(-7, 0, 0)),
                Type = EPostType.Page,
                Status = EPostStatus.Published,
            };

            var child = new Post
            {
                Id = 3,
                ParentId = 2,
                Title = "Ray",
                Slug = "ray",
                Body = "<h1>About Ray</h1>",
                BodyMark = "# About Ray",
                UserId = USER_ID,
                CreatedOn = new DateTimeOffset(new DateTime(2017, 01, 01), new TimeSpan(-7, 0, 0)),
                Type = EPostType.Page,
                Status = EPostStatus.Published,
            };

            list.Add(parent);
            list.Add(child);

            return list;
        }
    }
}
