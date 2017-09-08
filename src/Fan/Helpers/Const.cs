namespace Fan.Helpers
{
    public class Const
    {
        // -------------------------------------------------------------------- Url templates

        public const string CATEGORY_URL_TEMPLATE = "category/{0}";
        public const string CATEGORY_RSS_URL_TEMPLATE = "category/rss/{0}";
        public const string TAG_URL_TEMPLATE = "tag/{0}";
        public const string POST_URL_TEMPLATE = "post/{0}/{1}/{2}/{3}";
        public const string POST_EDIT_URL_TEMPLATE = "admin/post/edit/{0}";
        public const string MEDIA_URL_TEMPLATE = "uploads/{0}";

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

        public const string MEDIA_UPLOADS_FOLDER = "uploads";

    }
}
