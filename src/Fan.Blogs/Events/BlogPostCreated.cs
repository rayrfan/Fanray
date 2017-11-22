using Fan.Blogs.Models;
using MediatR;

namespace Fan.Blogs.Events
{
    public class BlogPostCreated : INotification
    {
        public BlogPost BlogPost { get; set; }
    }
}
