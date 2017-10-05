using Fan.Enums;

namespace Fan.Models
{
    public class Media : Post
    {
        public new EPostType Type { get; } = EPostType.Media;
    }
}
