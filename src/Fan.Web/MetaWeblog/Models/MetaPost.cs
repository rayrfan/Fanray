using System;
using System.Collections.Generic;

namespace Fan.Web.MetaWeblog
{
    public class MetaPost
    {
        public MetaPost()
        {
            Categories = new List<string>();
            Tags = new List<string>();
        }

        public string PostId { get; set; }
        public string Author { get; set; }
        public string CommentPolicy { get; set; }
        public string Title { get; set; }
        public string Slug { get; set; }
        public string Description { get; set; }
        public string Excerpt { get; set; }
        public string Link { get; set; }
        public DateTime PostDate { get; set; }
        public bool Publish { get; set; }
        public List<string> Categories { get; set; }
        public List<string> Tags { get; set; }
    }
}