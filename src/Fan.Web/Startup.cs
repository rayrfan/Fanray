using Fan.Data;
using Fan.Helpers;
using Fan.Models;
using Fan.Services;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Routing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Fan.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddDbContext<FanDbContext>(options =>
                options.UseSqlServer(Configuration.GetConnectionString("DefaultConnection")));

            services.AddIdentity<User, IdentityRole>()
                .AddEntityFrameworkStores<FanDbContext>()
                .AddDefaultTokenProviders();

            // Add application services.
            services.AddTransient<IEmailSender, EmailSender>();

            services.AddMvc();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
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

            app.UseStaticFiles();

            app.UseAuthentication();

            app.UseMvc(routes => RegisterRoutes(routes, app));
        }

        private void RegisterRoutes(IRouteBuilder routes, IApplicationBuilder app)
        {
            routes.MapRoute("Home", "", new { controller = "Home", action = "Index" });
            routes.MapRoute("Setup", "setup", new { controller = "Home", action = "Setup" });
            routes.MapRoute("About", "about", new { controller = "Home", action = "About" });
            routes.MapRoute("Admin", "admin", new { controller = "Home", action = "Admin" });

            routes.MapRoute("RSD", "rsd", new { controller = "Blog", action = "Rsd" });

            routes.MapRoute("BlogPost", string.Format(Const.POST_URL_TEMPLATE, "year", "month", "day", "slug"),
                new { controller = "Blog", action = "ViewPost", year = 0, month = 0, day = 0, slug = "" },
                new { year = @"^\d+$", month = @"^\d+$", day = @"^\d+$" });

            routes.MapRoute("BlogCategory", string.Format(Const.CATEGORY_URL_TEMPLATE, "slug"), 
                new { controller = "Blog", action = "ViewCategory", slug = "" });

            routes.MapRoute("BlogTag", string.Format(Const.TAG_URL_TEMPLATE, "slug"), 
                new { controller = "Blog", action = "ViewTag", slug = "" });

            routes.MapRoute(name: "Default", template: "{controller=Home}/{action=Index}/{id?}");
        }
    }
}
