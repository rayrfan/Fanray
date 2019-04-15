using Fan.Web.Events;
using Fan.Web.Models.Blog;
using MediatR;
using System.Threading;
using System.Threading.Tasks;

namespace Fan.Plugins.Shortcodes
{
    public class ShortcodesHandler : INotificationHandler<ModelPreRender<BlogPostViewModel>>,
                                     INotificationHandler<ModelPreRender<BlogPostListViewModel>>
    {
        private readonly IShortcodeService shortcodeService;

        public ShortcodesHandler(IShortcodeService shortcodeService)
        {
            this.shortcodeService = shortcodeService;
        }

        public Task Handle(ModelPreRender<BlogPostViewModel> notification, CancellationToken cancellationToken)
        {
            if (!(notification.Model is BlogPostViewModel)) return Task.CompletedTask;

            var body = ((BlogPostViewModel)notification.Model).Body;
            ((BlogPostViewModel)notification.Model).Body = shortcodeService.Parse(body);
            return Task.CompletedTask;
        }

        public Task Handle(ModelPreRender<BlogPostListViewModel> notification, CancellationToken cancellationToken)
        {
            if (!(notification.Model is BlogPostListViewModel)) return Task.CompletedTask;

            foreach (var postViewModel in ((BlogPostListViewModel)notification.Model).BlogPostViewModels)
            {
                postViewModel.Body = shortcodeService.Parse(postViewModel.Body);
            }
            return Task.CompletedTask;
        }
    }
}
