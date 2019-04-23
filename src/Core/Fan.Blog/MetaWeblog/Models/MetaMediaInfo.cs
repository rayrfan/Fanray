namespace Fan.Blog.MetaWeblog
{
    /// <summary>
    /// The media info returned by <see cref="IMetaWeblogService.NewMediaObjectAsync(string, string, string, MetaMediaObject, Microsoft.AspNetCore.Http.HttpContext)"/>
    /// given a <see cref="MetaMediaObject"/> object.
    /// </summary>
    public class MetaMediaInfo
    {
        public string Url { get; set; }
    }
}