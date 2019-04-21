using Fan.Plugins;
using Fan.Web.Events;
using Fan.Web.Models.Blog;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Shortcodes.Services;

namespace Shortcodes
{
    public class ShortcodesPlugin : Plugin
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<INotificationHandler<ModelPreRender<BlogPostViewModel>>, ShortcodesHandler>();
            services.AddScoped<INotificationHandler<ModelPreRender<BlogPostListViewModel>>, ShortcodesHandler>();

            var shortcodeService = new ShortcodeService();
            shortcodeService.Add<SourceCodeShortcode>(tag: "code");
            shortcodeService.Add<YouTubeShortcode>(tag: "youtube");
            services.AddSingleton<IShortcodeService>(shortcodeService);
        }

        public override string GetInfoUrl()
        {
            throw new System.NotImplementedException();
        }
    }
}
