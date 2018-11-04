using AutoMapper;
using Fan.Blog.Models;
using Fan.Helpers;
using System.Collections.Generic;

namespace Fan.Blog.Helpers
{
    public class BlogUtil
    {
        /// <summary>
        /// Returns a valid slug for a category or tag.
        /// </summary>
        /// <param name="title">Category or tag's title.</param>
        /// <param name="maxlen">The max length input is allowed.</param>
        /// <param name="existingSlugs"></param>
        /// <remarks>
        /// This method makes sure the result slug
        /// - not to exceed max len;
        /// - if <see cref="Util.Slugify(string)"/> returns empty string, it generates a random one;
        /// - a unique value if its a duplicate with existings slugs;
        /// - if '#' char is present, I swap it to 's'
        /// </remarks>
        public static string SlugifyTaxonomy(string title, int maxlen, IEnumerable<string> existingSlugs = null)
        {
            // if user input exceeds max len, we trim
            if (title.Length > maxlen)
            {
                title = title.Substring(0, maxlen);
            }

            // preserve # as s before format to slug
            title = title.Replace('#', 's');

            // make slug
            var slug = Util.Slugify(title, randomCharCountOnEmpty: 6);

            // make sure slug is unique
            slug = Util.UniquefySlug(slug, existingSlugs);

            return slug;
        }

        /// <summary>
        /// Returns automapper mapping.
        /// </summary>
        /// <remarks>
        /// https://github.com/AutoMapper/AutoMapper/issues/1441
        /// </remarks>
        public static IMapper Mapper
        {
            get
            {
                return new MapperConfiguration(cfg =>
                {
                    cfg.CreateMap<Post, BlogPost>();
                    cfg.CreateMap<BlogPost, Post>();
                    cfg.CreateMap<Post, Page>();
                    cfg.CreateMap<Page, Post>();
                }).CreateMapper();
            }
        }
    }
}
