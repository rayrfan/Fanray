using Fan.Blog.Enums;
using Fan.Medias;
using System;
using System.Collections.Generic;

namespace Fan.Blog.Services
{
    /// <summary>
    /// The constants and configurable elements for blog service.
    /// </summary>
    public partial class BlogService
    {
        /// <summary>
        /// "Blog"
        /// </summary>
        public const string BLOG_APP_NAME = "Blog";

        // -------------------------------------------------------------------- Cache

        /// <summary>
        /// By default show 10 posts per page.
        /// </summary>
        public const int DEFAULT_PAGE_SIZE = 10;
        public const int DEFAULT_PAGE_INDEX = 1;
        public const string CACHE_KEY_ALL_CATS = "BlogCategories";
        public const string CACHE_KEY_ALL_TAGS = "BlogTags";
        public const string CACHE_KEY_POSTS_INDEX = "BlogPostsIndex";
        public const string CACHE_KEY_ALL_ARCHIVES = "BlogArchives";
        public const string CACHE_KEY_POST_COUNT = "BlogPostCount";
        public static TimeSpan CacheTime_PostsIndex = new TimeSpan(0, 10, 0);
        public static TimeSpan CacheTime_AllCats = new TimeSpan(0, 10, 0);
        public static TimeSpan CacheTime_AllTags = new TimeSpan(0, 10, 0);
        public static TimeSpan CacheTime_Archives = new TimeSpan(0, 10, 0);
        public static TimeSpan CacheTime_PostCount = new TimeSpan(0, 10, 0);

        // -------------------------------------------------------------------- Posts

        /// <summary>
        /// How many words to extract into excerpt from body. Default 55.
        /// </summary>
        public const int EXCERPT_WORD_LIMIT = 55;

        // -------------------------------------------------------------------- Images

        /// <summary>
        /// Blog accepted image types: .jpg .jpeg .png .gif
        /// </summary>
        /// <remarks>
        /// Got the idea from WP https://en.support.wordpress.com/images/
        /// For accepted file types https://en.support.wordpress.com/accepted-filetypes/
        /// </remarks>
        public static readonly string[] Accepted_Image_Types = { ".jpg", ".jpeg", ".gif", ".png" };

        /// <summary>
        /// The separator used in image paths is '/'.
        /// </summary>
        /// <remarks>
        /// All <see cref="IStorageProvider"/> implementations should take this separator and replace 
        /// it with your specific one.
        /// </remarks>
        public const char IMAGE_PATH_SEPARATOR = '/';

        /// <summary>
        /// Large image size 1200 pixel.
        /// </summary>
        public const int LARGE_IMG_SIZE = 1200;

        /// <summary>
        /// Medium image size 800 pixel.
        /// </summary>
        public const int MEDIUM_IMG_SIZE = 800;

        /// <summary>
        /// Small image size 400 pixel.
        /// </summary>
        public const int SMALL_IMG_SIZE = 400;

        /// <summary>
        /// The different image resizes per image upload.
        /// </summary>
        /// <param name="uploadedOn"></param>
        /// <returns></returns>
        public static List<ImageResizeInfo> GetImageResizeList(DateTimeOffset uploadedOn)
        {
           return new List<ImageResizeInfo> {
                new ImageResizeInfo {
                    Pixel = int.MaxValue,
                    Path = GetImagePath(uploadedOn, EImageSize.Original),
                    PathSeparator = IMAGE_PATH_SEPARATOR,
                },
                new ImageResizeInfo {
                    Pixel = LARGE_IMG_SIZE,
                    Path = GetImagePath(uploadedOn, EImageSize.Large),
                    PathSeparator = IMAGE_PATH_SEPARATOR,
                },
                new ImageResizeInfo {
                    Pixel = MEDIUM_IMG_SIZE,
                    Path = GetImagePath(uploadedOn, EImageSize.Medium),
                    PathSeparator = IMAGE_PATH_SEPARATOR,
                },
                new ImageResizeInfo {
                    Pixel = SMALL_IMG_SIZE,
                    Path = GetImagePath(uploadedOn, EImageSize.Small),
                    PathSeparator = IMAGE_PATH_SEPARATOR,
                },
            };
        }

        /// <summary>
        /// Returns the stored image path.
        /// </summary>
        /// <param name="uploadedOn"></param>
        /// <param name="size"></param>
        /// <returns></returns>
        public static string GetImagePath(DateTimeOffset uploadedOn, EImageSize size)
        {
            var app = BLOG_APP_NAME.ToLowerInvariant();
            var year = uploadedOn.Year.ToString();
            var month = uploadedOn.Month.ToString("d2");
            var sizePath = "";

            switch (size)
            {
                case EImageSize.Large:
                    sizePath = "lg";
                    break;
                case EImageSize.Medium:
                    sizePath = "md";
                    break;
                case EImageSize.Small:
                    sizePath = "sm";
                    break;
                default:
                    sizePath = null;
                    break;
            }

            return size == EImageSize.Original ? $"{app}/{year}/{month}" : $"{app}/{year}/{month}/{sizePath}";
        }
    }
}
