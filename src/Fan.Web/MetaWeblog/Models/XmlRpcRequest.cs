namespace Fan.Web.MetaWeblog.Models
{
    public class XmlRpcRequest
    {
        public string MethodName { get; set; }
        public string AppKey { get; set; } // not used
        public string BlogId { get; set; } // not used
        public string UserName { get; set; }
        public string Password { get; set; }
        public string PostId { get; set; }
        public bool Publish { get; set; }
        public int NumberOfPosts { get; set; }
        public string CategoryTitle { get; set; } // wp.newCategory only
        public MetaPost Post { get; set; }
        public MetaMediaObject MediaObject { get; set; }
    }
}
