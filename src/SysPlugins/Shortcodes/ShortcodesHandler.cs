using Fan.Blog.Models.View;
using Fan.Blog.Services;
using Fan.Plugins;
using Fan.Web.Events;
using MediatR;
using Shortcodes.Services;
using System.Threading;
using System.Threading.Tasks;

namespace Shortcodes
{
    /// <summary>
    /// Handler for parsing shortcodes in Page or Post body content.
    /// </summary>
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

        /// <summary>
        /// Handles <see cref="PageVM"/> by parsing shortcodes then nav links.
        /// </summary>
        /// <param name="notification"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <remarks>
        /// NavLinks are a special code with double brackets [[Page Title]].
        /// TODO the logic of parsing NavLinks should not be here, it should be in its own INotificationHandler.
        /// </remarks>
        public Task Handle(ModelPreRender<PageVM> notification, CancellationToken cancellationToken)
        {
            if (!(notification.Model is PageVM)) return Task.CompletedTask;

            var pageVM = (PageVM)notification.Model;
            ((PageVM)notification.Model).Body = PageService.ParseNavLinks(shortcodeService.Parse(pageVM.Body), pageVM.Slug);

            return Task.CompletedTask;
        }

        /// <summary>
        /// Handles <see cref="BlogPostVM"/> by parsing any shortcodes in its body.
        /// </summary>
        /// <param name="notification"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task Handle(ModelPreRender<BlogPostVM> notification, CancellationToken cancellationToken)
        {
            if (!(notification.Model is BlogPostVM)) return Task.CompletedTask;

            var body = ((BlogPostVM)notification.Model).Body;
            ((BlogPostVM)notification.Model).Body = shortcodeService.Parse(body);
         
            return Task.CompletedTask;
        }

        /// <summary>
        /// Handles <see cref="BlogPostListVM"/> by parsing any shortcodes in the body of the list of posts.
        /// </summary>
        /// <param name="notification"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public Task Handle(ModelPreRender<BlogPostListVM> notification, CancellationToken cancellationToken)
        {
            if (!(notification.Model is BlogPostListVM)) return Task.CompletedTask;

            foreach (var postViewModel in ((BlogPostListVM)notification.Model).BlogPostViewModels)
            {
                postViewModel.Body = shortcodeService.Parse(postViewModel.Body);
            }
         
            return Task.CompletedTask;
        }
    }
}
