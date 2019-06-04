using Fan.Plugins;
using Fan.Web.Events;
using Fan.Web.Models.Blog;
using MediatR;
using Shortcodes.Services;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Shortcodes
{
    public class ShortcodesHandler : INotificationHandler<ModelPreRender<BlogPostViewModel>>,
                                     INotificationHandler<ModelPreRender<BlogPostListViewModel>>
    {
        private readonly IShortcodeService shortcodeService;
        private readonly IPluginService pluginService;

        public ShortcodesHandler(IShortcodeService shortcodeService, IPluginService pluginService)
        {
            this.shortcodeService = shortcodeService;
            this.pluginService = pluginService;
        }

        public async Task Handle(ModelPreRender<BlogPostViewModel> notification, CancellationToken cancellationToken)
        {
            if (!await IsPluginActiveAsync() || !(notification.Model is BlogPostViewModel)) return;

            var body = ((BlogPostViewModel)notification.Model).Body;
            ((BlogPostViewModel)notification.Model).Body = shortcodeService.Parse(body);
        }

        public async Task Handle(ModelPreRender<BlogPostListViewModel> notification, CancellationToken cancellationToken)
        {
            if (!await IsPluginActiveAsync() || !(notification.Model is BlogPostListViewModel)) return;

            foreach (var postViewModel in ((BlogPostListViewModel)notification.Model).BlogPostViewModels)
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
