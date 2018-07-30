using System;
using System.Collections.Generic;

namespace Fan.Blog.MetaWeblog
{
    public class MetaPost
    {
        public MetaPost()
        {
            Categories = new List<string>();
            Tags = new List<string>();
        }

        public string PostId { get; set; }
        public string AuthorId { get; set; }
        /// <summary>
        /// Comments setting in OLW Post Properties.
        /// </summary>
        /// <remarks>
        /// OLW has 4 different possible options in the dropdown and the values given to this 
        /// property respectively: (Default) -> null, None -> "0", Open -> "1", Close -> "2".
        /// 
        /// I'm interprest Default as to be the value of <see cref="BlogSettings.AllowCommentsOnBlogPost"/>,
        /// None means no comments, Open means allow comments, Close means the post was allowing
        /// comments but now it's being closed.
        /// </remarks>
        public string CommentPolicy { get; set; }
        public string Title { get; set; }
        public string Slug { get; set; }
        public string Description { get; set; }
        public string Excerpt { get; set; }
        public string Link { get; set; }
        public DateTimeOffset PostDate { get; set; }
        public bool Publish { get; set; }
        public List<string> Categories { get; set; }
        public List<string> Tags { get; set; }
    }
}