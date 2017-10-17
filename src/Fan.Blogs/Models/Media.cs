using Fan.Blogs.Enums;
using Fan.Enums;

namespace Fan.Blogs.Models
{
    public class Media : Post
    {
        public new EPostType Type { get; } = EPostType.Media;
    }
}
