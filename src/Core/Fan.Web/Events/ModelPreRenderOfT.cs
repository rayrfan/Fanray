using MediatR;

namespace Fan.Web.Events
{
    /// <summary>
    /// Occurs before a view renders a view model.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class ModelPreRender<T> : INotification
    {
        public ModelPreRender(T model)
        {
            Model = model;
        }

        public T Model { get; }
    }
}
