namespace Fan.Models
{
    /// <summary>
    /// Settings for the blog application.
    /// </summary>
    public class BlogSettings
    {
        /// <summary>
        /// Number of blog posts to show. Default 10.
        /// </summary>
        public int PageSize { get; set; } = 10; 
        /// <summary>
        /// There must be one default category. Default 1.
        /// </summary>
        public int DefaultCategoryId { get; set; } = 1;
        /// <summary>
        /// How many words to extract into excerpt from body. Default 55.
        /// </summary>
        public int ExcerptWordLimit { get; set; } = 55;
        /// <summary>
        /// Should blog show a list of excerpt instead of body. Default false.
        /// </summary>
        public bool ShowExcerpt { get; set; } 
    }
}
