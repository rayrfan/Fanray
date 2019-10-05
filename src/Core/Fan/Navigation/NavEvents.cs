using MediatR;

namespace Fan.Navigation
{
    /// <summary>
    /// Raised by a <see cref="INavProvider"/> after a nav is updated.
    /// </summary>
    public class NavUpdated : INotification
    {
    }

    /// <summary>
    /// Raised by a <see cref="INavProvider" /> after a nav is deleted.
    /// </summary>
    public class NavDeleted : INotification
    {
        public int Id { get; set; }
        public ENavType Type { get; set; }
    }
}
