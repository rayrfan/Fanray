using System.Collections.Generic;

namespace Fan.Web.MetaWeblog.Models
{
    public class XmlRpcResponse
    {
        public XmlRpcResponse()
        {
            Blogs = new List<MetaBlogInfo>();
            Categories = new List<MetaCategory>();
            Keywords = new List<string>();
            Posts = new List<MetaPost>();
            Authors = new List<MetaAuthor>();
        }

        public MetaFault Fault { get; set; }

        public bool Completed { get; set; }

        public MetaMediaInfo MediaInfo { get; set; }

        public MetaPost Post { get; set; }

        public string PostId { get; set; }

        public int CategoryId { get; set; }

        public List<MetaPost> Posts { get; set; }

        public List<MetaAuthor> Authors { get; set; }

        public List<MetaBlogInfo> Blogs { get; set; }

        public List<MetaCategory> Categories { get; set; }

        public List<string> Keywords { get; set; }
    }
}
