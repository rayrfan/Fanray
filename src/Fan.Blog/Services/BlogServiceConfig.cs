using Fan.Blog.Enums;
using Fan.Medias;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fan.Blog.Services
{
    /// <summary>
    /// The constants and configurable elements for blog service.
    /// </summary>
    public partial class BlogService
    {
        // -------------------------------------------------------------------- Posts

        /// <summary>
        /// By default show 10 posts per page.
        /// </summary>
        public const int DEFAULT_PAGE_SIZE = 10;
        public const int DEFAULT_PAGE_INDEX = 1;

        /// <summary>
        /// How many words to extract into excerpt from body. Default 55.
        /// </summary>
        public const int EXCERPT_WORD_LIMIT = 55;
    }
}
