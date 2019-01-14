using AutoMapper;
using Fan.Blog.Helpers;
using Fan.Blog.Services.Interfaces;
using Fan.Data;
using Fan.Membership;
using Fan.Settings;
using Fan.Web.Infrastructure;
using MediatR;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Razor;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;
using Scrutor;

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
            // GDPR support
            services.Configure<CookiePolicyOptions>(options =>
            {
                // This lambda determines whether user consent for non-essential cookies is needed for a given request.
                options.CheckConsentNeeded = context => true;
                options.MinimumSameSitePolicy = SameSiteMode.None;
            });

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
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireLowercase = false;
                options.User.RequireUniqueEmail = true;
            })
            .AddEntityFrameworkStores<FanDbContext>()
            .AddDefaultTokenProviders();

            // LoginPath https://github.com/aspnet/Identity/issues/1414#issuecomment-328185754
            services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/login";
                options.AccessDeniedPath = "/denied";
            });

            // Caching
            services.AddDistributedMemoryCache();

            // Mapper
            services.AddAutoMapper();
            services.AddSingleton(BlogUtil.Mapper);

            // Mediatr
            services.AddMediatR();

            // Storage
            services.AddStorageProvider(Configuration);

            // Shortcodes
            services.AddShortcodes();

            // Fan and Fan.Blog services and repos, see https://bit.ly/2AtPmLn
            services.Scan(scan => scan
              .FromAssembliesOf(typeof(ISettingService), typeof(IBlogPostService))
              .AddClasses()
              .UsingRegistrationStrategy(RegistrationStrategy.Skip) // prevent added to add again
              .AsImplementedInterfaces()
              .WithScopedLifetime());

            // Preferred Domain
            services.AddScoped<IPreferredDomainRewriter, PreferredDomainRewriter>();

            // HttpContext
            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            // Mvc and Razor Pages

            // theme
            services.Configure<RazorViewEngineOptions>(options =>
            {
                options.ViewLocationExpanders.Add(new ThemeViewLocationExpander());
            });

            // if you update the roles and find the app not working, try logout then login https://stackoverflow.com/a/48177723/32240
            services.AddAuthorization(options =>
            {
                options.AddPolicy("AdminRoles", policy => policy.RequireRole("Administrator", "Editor"));
            });
             
            services.AddMvc()
                .SetCompatibilityVersion(CompatibilityVersion.Version_2_2)
                .AddSessionStateTempDataProvider()
                .AddJsonOptions(options => {
                    options.SerializerSettings.ContractResolver = new CamelCasePropertyNamesContractResolver();
                    options.SerializerSettings.ReferenceLoopHandling = ReferenceLoopHandling.Ignore;
                })
                .AddRazorPagesOptions(options =>
                {
                    options.Conventions.AuthorizeFolder("/Admin", "AdminRoles");
                });

            services.AddSession(); // for TempData only

            // https://stackoverflow.com/q/50472962/32240
            JsonConvert.DefaultSettings = () => new JsonSerializerSettings
            {
                ContractResolver = new CamelCasePropertyNamesContractResolver(),
                ReferenceLoopHandling = ReferenceLoopHandling.Ignore
            };

            // To make ajax work with razor pages, without this ajax post will get 404 
            // http://www.talkingdotnet.com/handle-ajax-requests-in-asp-net-core-razor-pages/
            services.AddAntiforgery(o => o.HeaderName = "XSRF-TOKEN");

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
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UsePreferredDomain();
            app.UseSetup();
            app.MapWhen(context => context.Request.Path.ToString().Equals("/olw"), appBuilder => appBuilder.UseMetablog());
            app.UseStatusCodePagesWithReExecute("/Home/ErrorCode/{0}"); // needs to be after hsts and rewrite
            app.UseStaticFiles();
            app.UseAuthentication(); // UseIdentity is obsolete, UseAuth is recommended
            app.UseCookiePolicy();
            app.UseSession(); // for TempData only
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
            BlogRoutes.RegisterRoutes(routes);
            routes.MapRoute(name: "Default", template: "{controller=Home}/{action=Index}/{id?}");
        }
    }
}
