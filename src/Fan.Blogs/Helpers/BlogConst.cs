namespace Fan.Blogs.Helpers
{
    public class BlogConst
    {
        // -------------------------------------------------------------------- Url templates

        public const string POST_RELATIVE_URL_TEMPLATE = "post/{0}/{1}/{2}/{3}";
        public const string POST_PERMA_URL_TEMPLATE = "post/{0}";
        public const string POST_EDIT_URL_TEMPLATE = "admin/post/edit/{0}";
        public const string MEDIA_URL_TEMPLATE = "uploads/{0}";
        public const string CATEGORY_URL_TEMPLATE = "category/{0}";
        public const string CATEGORY_RSS_URL_TEMPLATE = "category/{0}/feed";
        public const string TAG_URL_TEMPLATE = "tag/{0}";

        // -------------------------------------------------------------------- Field length

        /// <summary>
        /// Max length for a category and tag's title or slug is 24.
        /// </summary>
        /// <remarks>
        /// I'm treating a taxonomy's title and slug with same length requirement.
        /// </remarks>
        public const int TAXONOMY_TITLE_SLUG_MAXLEN = 24;

        /// <summary>
        /// Max length for a post's title or slug is 256.
        /// </summary>
        /// <remarks>
        /// I'm treating a post's title and slug with same length requirement.
        /// </remarks>
        public const int POST_TITLE_SLUG_MAXLEN = 256;

        /// <summary>
        /// Max len for a media filename is 128.
        /// </summary>
        public const int MEDIA_FILENAME_MAXLEN = 128;

        /// <summary>
        /// Folder to save uploaded media is called "uploads".
        /// </summary>
        public const string MEDIA_UPLOADS_FOLDER = "uploads";

        /// <summary>
        /// Accepted image types: .jpg .jpeg .png .gif
        /// </summary>
        /// <remarks>
        /// Getting this idea from WP https://en.support.wordpress.com/images/
        /// </remarks>
        public static readonly string[] Accepted_Image_Types = { ".jpg", ".jpeg", ".gif", ".png" };

        // -------------------------------------------------------------------- Initial data

        public const string DEFAULT_CATEGORY = "Uncategorized";
        public const string WELCOME_POST_TITLE = "Welcome to Fanray";
        public const string WELCOME_POST_BODY = @"<p>To start posting</p><ul><li>Install <a href=""http://openlivewriter.org"" target=""_blank"">Open Live Writer</a></li><li>Open OLW &gt; Add blog account... &gt; Other services, type in</li><ul><li>Web address of your blog</li><li>User name</li><li>Password</li></ul></ul>";
    }
}
