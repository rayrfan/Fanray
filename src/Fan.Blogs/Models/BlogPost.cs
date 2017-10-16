using Fan.Blogs.Enums;
using Fan.Blogs.Helpers;
using System.Collections.Generic;

namespace Fan.Blogs.Models
{
    public class BlogPost : Post
    {
        public BlogPost()
        {
            Tags = new List<Tag>();
            TagTitles = new List<string>();
        }

        public new EPostType Type { get; } = EPostType.BlogPost;

        public string RelativeLink => string.Format("/" + BlogConst.POST_URL_TEMPLATE, CreatedOn.Year, CreatedOn.Month, CreatedOn.Day, Slug);

        public string EditLink => string.Format("/" + BlogConst.POST_EDIT_URL_TEMPLATE, Id);

        public string CategoryTitle { get; set; }

        public List<Tag> Tags { get; set; }

        public List<string> TagTitles { get; set; }

    }
}
