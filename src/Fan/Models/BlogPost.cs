using Fan.Enums;
using Fan.Helpers;
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

        public string RelativeLink => string.Format("/" + Const.POST_URL_TEMPLATE, CreatedOn.Year, CreatedOn.Month, CreatedOn.Day, Slug);

        public string EditLink => string.Format("/" + Const.POST_EDIT_URL_TEMPLATE, Id);

        public string CategoryTitle { get; set; }

        public List<Tag> Tags { get; set; }

        public List<string> TagTitles { get; set; }

    }
}
