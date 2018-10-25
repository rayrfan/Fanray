namespace Fan.Blog.Models
{
    public class PostTag
    {
        public int TagId { get; set; }
        public int PostId { get; set; }

        public virtual Post Post { get; set; }
        public virtual Tag Tag { get; set; }
    }
}
