using MediatR;

namespace Fan.Navigation
{
    /// <summary>
    /// Raised by a <see cref="INavProvider"/> after a nav is updated.
    /// </summary>
    public class NavUpdated : INotification
    {
        public int Id { get; set; }
        public ENavType Type { get; set; }
        public bool IsDraft { get; set; }
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
