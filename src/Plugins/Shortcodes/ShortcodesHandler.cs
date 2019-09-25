using Fan.Blog.Models.View;
using Fan.Plugins;
using Fan.Web.Events;
using MediatR;
using Shortcodes.Services;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Shortcodes
{
    public class ShortcodesHandler : INotificationHandler<ModelPreRender<PageVM>>, 
                                     INotificationHandler<ModelPreRender<BlogPostVM>>,
                                     INotificationHandler<ModelPreRender<BlogPostListVM>>
    {
        private readonly IShortcodeService shortcodeService;
        private readonly IPluginService pluginService;

        public ShortcodesHandler(IShortcodeService shortcodeService, IPluginService pluginService)
        {
            this.shortcodeService = shortcodeService;
            this.pluginService = pluginService;
        }

        public async Task Handle(ModelPreRender<PageVM> notification, CancellationToken cancellationToken)
        {
            if (!await IsPluginActiveAsync() || !(notification.Model is PageVM)) return;

            var body = ((PageVM)notification.Model).Body;
            ((PageVM)notification.Model).Body = shortcodeService.Parse(body);
        }

        public async Task Handle(ModelPreRender<BlogPostVM> notification, CancellationToken cancellationToken)
        {
            if (!await IsPluginActiveAsync() || !(notification.Model is BlogPostVM)) return;

            var body = ((BlogPostVM)notification.Model).Body;
            ((BlogPostVM)notification.Model).Body = shortcodeService.Parse(body);
        }

        public async Task Handle(ModelPreRender<BlogPostListVM> notification, CancellationToken cancellationToken)
        {
            if (!await IsPluginActiveAsync() || !(notification.Model is BlogPostListVM)) return;

            foreach (var postViewModel in ((BlogPostListVM)notification.Model).BlogPostViewModels)
            {
                postViewModel.Body = shortcodeService.Parse(postViewModel.Body);
            }
        }

        /// <summary>
        /// Returns true if active plugins contains "Shortcodes".
        /// </summary>
        /// <returns></returns>
        private async Task<bool> IsPluginActiveAsync()
        {
            var plugins = await pluginService.GetActivePluginsAsync();
            return plugins.Any(p => p.Folder == "Shortcodes");
        }
    }
}
