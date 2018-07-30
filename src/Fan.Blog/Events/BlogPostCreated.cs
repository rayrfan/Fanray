using Fan.Blog.Models;
using MediatR;

namespace Fan.Blog.Events
{
    public class BlogPostCreated : INotification
    {
        public BlogPost BlogPost { get; set; }
    }
}
