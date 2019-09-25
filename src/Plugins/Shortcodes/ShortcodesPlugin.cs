using Fan.Blog.Models.View;
using Fan.Plugins;
using Fan.Web.Events;
using MediatR;
using Microsoft.Extensions.DependencyInjection;
using Shortcodes.Services;

namespace Shortcodes
{
    public class ShortcodesPlugin : Plugin
    {
        public override void ConfigureServices(IServiceCollection services)
        {
            services.AddScoped<INotificationHandler<ModelPreRender<PageVM>>, ShortcodesHandler>();
            services.AddScoped<INotificationHandler<ModelPreRender<BlogPostVM>>, ShortcodesHandler>();
            services.AddScoped<INotificationHandler<ModelPreRender<BlogPostListVM>>, ShortcodesHandler>();

            var shortcodeService = new ShortcodeService();
            shortcodeService.Add<SourceCodeShortcode>(tag: "code");
            shortcodeService.Add<YouTubeShortcode>(tag: "youtube");
            services.AddSingleton<IShortcodeService>(shortcodeService);
        }

        public override string DetailsUrl => "https://github.com/FanrayMedia/Fanray/wiki/Shortcodes-Plugin";
        public override string GetFootScriptsViewName() => "ShortcodesScripts";
        public override string GetStylesViewName() => "ShortcodesStyles";
    }
}
