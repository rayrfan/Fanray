using Fan.Enums;
using System.Collections.Generic;

namespace Fan.Models
{
    public class BlogPost : Post
    {
        public BlogPost()
        {
            Tags = new List<Tag>();
            TagTitles = new List<string>();
        }

        public new EPostType Type { get; } = EPostType.BlogPost;

        public string CategoryTitle { get; set; }

        public List<Tag> Tags { get; set; }

        public List<string> TagTitles { get; set; }

    }
}
