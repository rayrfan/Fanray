using Fan.Web.Events;

namespace MediatR
{
    public static class IMediatorExtensions
    {
        /// <summary>
        /// Raises a <see cref="ModelPreRender{T}"/> event.
        /// </summary>
        /// <typeparam name="T">Type of the model.</typeparam>
        /// <param name="mediator">IMediator.</param>
        /// <param name="model">The model.</param>
        public static void OnModelPreRender<T>(this IMediator mediator, T model)
        {
            mediator.Publish(new ModelPreRender<T>(model));
        }
    }
}
