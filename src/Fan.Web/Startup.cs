using AutoMapper;
using Fan.Data;
using Fan.Helpers;
using Fan.Models;
using Fan.Services;
using Fan.Web.MetaWeblog;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System.IO;

namespace Fan.Web
{
    public class Startup
    {
        private ILogger<Startup> _logger;

        public Startup(IConfiguration configuration, IHostingEnvironment env, ILoggerFactory loggerFactory)
        {
            HostingEnvironment = env;
            Configuration = configuration;
            _logger = loggerFactory.CreateLogger<Startup>();
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment HostingEnvironment { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // Db
            services.AddDbContext<FanDbContext>(builder =>
            {
                bool.TryParse(Configuration["Database:UseSqLite"], out bool useSqLite);
                if (useSqLite)
                {
                    builder.UseSqlite("Data Source=" + Path.Combine(HostingEnvironment.ContentRootPath, "Fanray.sqlite"));
                    _logger.LogInformation("Using SQLite database.");
                }
                else
                {
                    builder.UseSqlServer(Configuration.GetConnectionString("DefaultConnection"));
                    _logger.LogInformation("Using SQL Server database.");
                }
            });

            // Identity
            services.AddIdentity<User, IdentityRole>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 4;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
            })
            .AddEntityFrameworkStores<FanDbContext>()
            .AddDefaultTokenProviders();

            // Caching
            services.AddDistributedMemoryCache();

            // Mapper
            services.AddAutoMapper();
            services.AddSingleton(Util.Mapper);

            // Repos / Services
            services.AddScoped<IPostRepository, SqlPostRepository>();
            services.AddScoped<IMetaRepository, SqlMetaRepository>();
            services.AddScoped<ICategoryRepository, SqlCategoryRepository>();
            services.AddScoped<ITagRepository, SqlTagRepository>();
            services.AddScoped<IEmailSender, EmailSender>();
            services.AddScoped<IBlogService, BlogService>();
            services.AddScoped<IXmlRpcHelper, XmlRpcHelper>();
            services.AddScoped<IMetaWeblogService, MetaWeblogService>();
            services.Configure<AppSettings>(Configuration.GetSection("AppSettings"));

            // Mvc
            services.AddMvc();
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            // OLW
            app.MapWhen(context => context.Request.Path.ToString().Equals("/olw"), appBuilder => appBuilder.UseMetablog());

            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseBrowserLink();
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
            }

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseMvc(routes => RegisterRoutes(routes, app));

            //SeedData.InitializeAsync(app.ApplicationServices).Wait();
            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var db = serviceScope.ServiceProvider.GetService<FanDbContext>();
                // you can create db here or you can click apply migrations when site launches
                db.Database.EnsureCreated();
            }
        }

        private void RegisterRoutes(IRouteBuilder routes, IApplicationBuilder app)
        {
            routes.MapRoute("Home", "", new { controller = "Blog", action = "Index" });
            routes.MapRoute("Setup", "setup", new { controller = "Blog", action = "Setup" });
            routes.MapRoute("About", "about", new { controller = "Home", action = "About" });
            routes.MapRoute("Contact", "contact", new { controller = "Home", action = "Contact" });
            routes.MapRoute("Admin", "admin", new { controller = "Home", action = "Admin" });

            routes.MapRoute("RSD", "rsd", new { controller = "Blog", action = "Rsd" });

            routes.MapRoute("BlogPost", string.Format(Const.POST_URL_TEMPLATE, "{year}", "{month}", "{day}", "{slug}"),
                new { controller = "Blog", action = "Post", year = 0, month = 0, day = 0, slug = "" },
                new { year = @"^\d+$", month = @"^\d+$", day = @"^\d+$" });

            routes.MapRoute("BlogCategory", string.Format(Const.CATEGORY_URL_TEMPLATE, "{slug}"), 
                new { controller = "Blog", action = "Category", slug = "" });

            routes.MapRoute("BlogTag", string.Format(Const.TAG_URL_TEMPLATE, "{slug}"), 
                new { controller = "Blog", action = "Tag", slug = "" });

            routes.MapRoute(name: "Default", template: "{controller=Home}/{action=Index}/{id?}");
        }
    }
}
