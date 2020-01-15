using AutoMapper;
using Fan.Blog.Helpers;
using Fan.Blog.Models;
using Fan.Blog.Services;
using Fan.Data;
using Fan.Membership;
using Fan.Navigation;
using Fan.Settings;
using Fan.Web.Controllers;
using Fan.Web.Helpers;
using Fan.Web.Middlewares;
using Fan.Web.Theming;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Scrutor;
using System.IO;
using System.Linq;

namespace Fan.WebApp
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IWebHostEnvironment env)
        {
            Env = env;
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        public IWebHostEnvironment Env { get; }

        public void ConfigureServices(IServiceCollection services)
        {
            // GDPR support
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

            // DbCtx
            services.AddDbContext<FanDbContext>(options => options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            // Identity
            services.AddIdentity<User, Role>(options =>
            {
                options.Password.RequireDigit = false;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<FanDbContext>()
            .AddDefaultTokenProviders();

            // Cookie https://bit.ly/2FNyPnr
            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/login";
                options.AccessDeniedPath = "/denied";
            });

            // Caching
            services.AddDistributedMemoryCache();

            // AutoMapper
            services.AddAutoMapper(typeof(BlogPost));
            services.AddSingleton(BlogUtil.Mapper);

            // Mediatr
            services.AddMediatR(typeof(BlogPost));

            // Storage
            services.AddStorageProvider(Configuration);

            // Plugins
            services.AddPlugins(Env);

            // Scrutor https://bit.ly/2AtPmLn 
            services.Scan(scan => scan
              .FromAssembliesOf(typeof(ISettingService), typeof(BlogPost), typeof(IHomeHelper))
              .AddClasses()
              .UsingRegistrationStrategy(RegistrationStrategy.Skip) // prevent added to add again
              .AsImplementedInterfaces()
              .WithScopedLifetime());

            services.AddScoped<INavProvider, PageService>();
            services.AddScoped<INavProvider, CategoryService>();

            // Preferred Domain
            services.AddScoped<IPreferredDomainRewriter, PreferredDomainRewriter>();

            // HttpContext
            services.AddHttpContextAccessor();

            // Theme
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Add(new ThemeViewLocationExpander());
            });

            // Authorization
            // if you update the roles and find the app not working, try logout then login https://stackoverflow.com/a/48177723/32240
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminRoles", policy => policy.RequireRole("Administrator", "Editor"));
            });
                 
            // MVC, Razor Pages, TempData, Json.net
            var builder = services.AddMvc() // https://bit.ly/2XTLFZB
                .AddApplicationPart(typeof(HomeController).Assembly) // https://bit.ly/2Zbbe8I
                .AddSessionStateTempDataProvider()
                .AddNewtonsoftJson(options => {
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                })
                .AddRazorPagesOptions(options =>
                {
                    options.RootDirectory = "/Manage";
                    options.Conventions.AuthorizeFolder("/Admin", "AdminRoles");
                    options.Conventions.AuthorizeFolder("/Plugins", "AdminRoles");
                    options.Conventions.AuthorizeFolder("/Widgets", "AdminRoles");
                });

            services.AddSession(); // for TempData only

            // JsonConvert https://stackoverflow.com/a/50473267/32240
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            // Antiforgery https://bit.ly/2Tn7EaZ 
            // To make ajax work with razor pages
            services.AddAntiforgery(o => o.HeaderName = "XSRF-TOKEN");

            // AppInsights
            services.AddApplicationInsightsTelemetry(Configuration);

#if DEBUG
            // RCLs to monitor
            if (Env.IsDevelopment())
            {
                string[] extDirs = { "Plugins", "SysPlugins", "Themes", "Widgets" };
                string[] extPaths = { };

                foreach (var extDir in extDirs)
                {
                    var dirPath = Directory.GetDirectories(Path.GetFullPath(Path.Combine(Env.ContentRootPath, "..", "..", extDir)));
                    extPaths = extPaths.Concat(dirPath).ToArray();
                }

                builder.AddRazorRuntimeCompilation(options =>
                {
                    foreach (var path in extPaths)
                    {
                        options.FileProviders.Add(new PhysicalFileProvider(path));
                    }
                });
            }
#endif
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
#if DEBUG
                app.UseBrowserLink();
#endif
                app.UseDatabaseErrorPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UsePreferredDomain();
            app.UseSetup();
            app.MapWhen(context => context.Request.Path.ToString().Equals("/olw"), appBuilder => appBuilder.UseMetablog());
            app.UseStatusCodePagesWithReExecute("/Home/ErrorCode/{0}"); // needs to be after hsts and rewrite
            app.UseStaticFiles();
            app.UseRouting();
            app.UseAuthentication(); 
            app.UseAuthorization();
            app.UseCookiePolicy();
            app.UseSession(); // for TempData only
            app.UsePlugins(env);

            app.UseEndpoints(endpoints => 
            {
                endpoints.MapControllerRoute("Home", "", new { controller = "Home", action = "Index" });
                BlogRoutes.RegisterRoutes(endpoints);
                endpoints.MapControllerRoute(name: "Default", pattern: "{controller=Home}/{action=Index}/{id?}");
                endpoints.MapRazorPages();
            });

            using var serviceScope = app.ApplicationServices.GetRequiredService<IServiceScopeFactory>().CreateScope();
            var db = serviceScope.ServiceProvider.GetService<FanDbContext>();
            if (!db.Database.ProviderName.Equals("Microsoft.EntityFrameworkCore.InMemory"))
                db.Database.Migrate();
        }
    }
}
