using AutoMapper;
using Fan.Blogs.Data;
using Fan.Blogs.Helpers;
using Fan.Blogs.MetaWeblog;
using Fan.Blogs.Services;
using Fan.Data;
using Fan.Emails;
using Fan.Medias;
using Fan.Models;
using Fan.Settings;
using Fan.Shortcodes;
using Fan.Web.Middlewares;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Fan.Web
{
    public class Startup
    {
        private ILogger<Startup> _logger;

        public Startup(IConfiguration configuration, IHostingEnvironment env, ILogger<Startup> logger)
        {
            HostingEnvironment = env;
            Configuration = configuration;
            _logger = logger;
        }

        public IConfiguration Configuration { get; }
        public IHostingEnvironment HostingEnvironment { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // Db 
            /**
             * AddDbContextPool is an EF Core 2.0 performance enhancement https://docs.microsoft.com/en-us/ef/core/what-is-new/
             * unfortunately it has limitations and cannot be used here.  
             * 1. It interferes with dbcontext implicit transactions when events are raised and event handlers call SaveChangesAsync
             * 2. Multiple dbcontexts will fail https://github.com/aspnet/EntityFrameworkCore/issues/9433
             * 3. To use AddDbContextPool, FanDbContext can only have a single public constructor accepting a single parameter of type DbContextOptions
             */
            services.AddDbContext<FanDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            // Identity
            services.AddIdentity<User, Role>(options =>
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
            services.AddSingleton(BlogUtil.Mapper);

            // Mediatr
            services.AddMediatR();

            // Repos & Services
            services.AddScoped<IMetaRepository, SqlMetaRepository>();
            services.AddScoped<IMediaRepository, SqlMediaRepository>();
            services.AddScoped<IPostRepository, SqlPostRepository>();
            services.AddScoped<ICategoryRepository, SqlCategoryRepository>();
            services.AddScoped<ITagRepository, SqlTagRepository>();
            services.AddScoped<ISettingService, SettingService>();
            services.AddScoped<IMediaService, MediaService>();
            services.AddScoped<IEmailSender, EmailSender>();
            services.AddScoped<IBlogService, BlogService>();
            services.AddScoped<IXmlRpcHelper, XmlRpcHelper>();
            services.AddScoped<IMetaWeblogService, MetaWeblogService>();
            services.AddScoped<IHttpWwwRewriter, HttpWwwRewriter>();
            var appSettingsConfigSection = Configuration.GetSection("AppSettings");
            services.Configure<AppSettings>(appSettingsConfigSection);
            var appSettings = appSettingsConfigSection.Get<AppSettings>();
            if (appSettings.MediaStorageType == EMediaStorageType.AzureBlob)
                services.AddScoped<IStorageProvider, AzureBlobStorageProvider>();
            else
                services.AddScoped<IStorageProvider, FileSysStorageProvider>();
            var shortcodeService = new ShortcodeService();
            shortcodeService.Add<SourceCodeShortcode>(tag: "code");
            shortcodeService.Add<YouTubeShortcode>(tag: "youtube");
            services.AddSingleton<IShortcodeService>(shortcodeService);
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // Mvc
            services.AddMvc();

            // AppInsights
            services.AddApplicationInsightsTelemetry(Configuration);
        }

        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
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

            app.UseHsts();
            app.UseHttpWwwRewrite();
            app.MapWhen(context => context.Request.Path.ToString().Equals("/olw"), appBuilder => appBuilder.UseMetablog());
            app.UseStatusCodePagesWithReExecute("/Home/ErrorCode/{0}"); // needs to be after hsts and rewrite
            app.UseStaticFiles();
            app.UseAuthentication();
            app.UseMvc(routes => RegisterRoutes(routes, app));

            using (var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope())
            {
                var db = serviceScope.ServiceProvider.GetService<FanDbContext>();
                db.Database.Migrate();
            }
        }

        private void RegisterRoutes(IRouteBuilder routes, IApplicationBuilder app)
        {
            routes.MapRoute("Home", "", new { controller = "Blog", action = "Index" });
            routes.MapRoute("Setup", "setup", new { controller = "Home", action = "Setup" });
            routes.MapRoute("About", "about", new { controller = "Home", action = "About" });
            routes.MapRoute("Contact", "contact", new { controller = "Home", action = "Contact" });
            routes.MapRoute("Admin", "admin", new { controller = "Home", action = "Admin" });

            BlogRoutes.RegisterRoutes(routes);

            routes.MapRoute(name: "Default", template: "{controller=Home}/{action=Index}/{id?}");
        }
    }
}
