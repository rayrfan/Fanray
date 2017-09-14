using Fan.Data;
using Fan.Enums;
using Fan.Helpers;
using Fan.Models;
using Fan.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Threading.Tasks;

namespace Fan.Web
{
    public class SeedData
    {
        public static async Task InitializeAsync(IServiceProvider serviceProvider)
        {
            using (var serviceScope = serviceProvider.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var db = serviceScope.ServiceProvider.GetService<FanDbContext>();

                if (await db.Database.EnsureCreatedAsync())
                {
                    await InsertData(serviceProvider, db);
                }
            }
        }

        private static async Task InsertData(IServiceProvider provider, FanDbContext db)
        {
            // svc
            var metaRepo = new SqlMetaRepository(db);
            var postRepo = new SqlPostRepository(db);
            var catRepo = new SqlCategoryRepository(db);
            var tagRepo = new SqlTagRepository(db);
            var userManager = provider.GetService<UserManager<User>>();
            var serviceProvider = new ServiceCollection().AddMemoryCache().AddLogging().BuildServiceProvider();
            var memCacheOptions = serviceProvider.GetService<IOptions<MemoryDistributedCacheOptions>>();
            var cache = new MemoryDistributedCache(memCacheOptions);
            var loggerFactory = provider.GetService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger<BlogService>();
            var blogSvc = new BlogService(catRepo, metaRepo, postRepo, tagRepo, cache, logger, Config.Mapper);
            var data = provider.GetService<IOptions<SeedData>>().Value;

            // data
            string userName = "admin";
            string password = "admin";
            string email = "admin@notset.com";
            string categoryTitle = "Uncategorized";
            bool seedWelcomePost = true;
            var postTitle = "Welcome to Fanray";
            var postBody = @"<p>To start posting</p><ul><li>Install <a href=""http://openlivewriter.org"" target=""_blank"">Open Live Writer</a></li><li>Open OLW &gt; Add blog account... &gt; Other services, type in</li><ul><li>Web address of your blog</li><li>User name</li><li>Password</li></ul></ul>";

            logger.LogInformation("Seeding initial data begins ...");

            // BlogSettings
            await blogSvc.CreateSettingsAsync(new BlogSettings());
            logger.LogInformation("BlogSettings created.");

            // User
            await userManager.CreateAsync(user: new User { UserName = userName, Email = email }, password: password); // AccountController Login and LoginViewModel
            logger.LogInformation($"User '{userName}' created.");

            // Post / Category 
            if (seedWelcomePost)
            {
                await blogSvc.CreatePostAsync(new BlogPost
                {
                    CategoryTitle = categoryTitle,
                    TagTitles = null,
                    Title = postTitle,
                    Body = postBody,
                    UserName = userName,
                    Status = EPostStatus.Published,
                    CommentStatus = ECommentStatus.AllowComments,
                    CreatedOn = DateTime.Now,
                });
                logger.LogInformation($"Default category '{categoryTitle}' created.");
            }
            else
            {
                await blogSvc.CreateCategoryAsync(new Category { Title = categoryTitle });
            }
            logger.LogInformation($"BlogPost '{postTitle}' created.");
            logger.LogInformation("Seeding initial data completes ...");
        }
    }
}
