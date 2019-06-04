namespace Fan.Blog.MetaWeblog
{
    public class MetaMediaObject
    {
        /// <summary>
        /// Filename.
        /// </summary>
        /// <remarks>
        /// OLW has extra path info in addition to filename e.g. "Open-Live-Writer/Test-post_5F5F/pic.jpg".
        /// </remarks>
        public string Name { get; set; }
        /// <summary>
        /// Content type e.g. "image/jpeg".
        /// </summary>
        public string Type { get; set; }
        /// <summary>
        /// File byte array.
        /// </summary>
        public byte[] Bits { get; set; }
    }
}