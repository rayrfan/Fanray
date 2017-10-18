using AutoMapper;
using Fan.Blogs.Models;
using Fan.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fan.Blogs.Helpers
{
    public class BlogUtil
    {
        /// <summary>
        /// Returns a valid slug for a category or tag, given either a title or user inputed slug. 
        /// </summary>
        /// <param name="input">Either title or slug</param>
        /// <remarks>
        /// This method makes sure the result slug
        /// - not to exceed max len;
        /// - if <see cref="Util.FormatSlug(string)"/> returns empty string, it generates a random one;
        /// - a unique value if its a duplicate with existings slugs;
        /// - if '#' char is present, I swap it to 's'
        /// </remarks>
        public static string FormatTaxonomySlug(string input, IEnumerable<string> existingSlugs = null)
        {
            // if user input exceeds max len, we trim
            if (input.Length > BlogConst.TAXONOMY_TITLE_SLUG_MAXLEN)
            {
                input = input.Substring(0, BlogConst.TAXONOMY_TITLE_SLUG_MAXLEN);
            }

            input = input.Replace('#', 's'); // preserve # as s before format to slug
            var slug = Util.FormatSlug(input); // remove/replace odd char, lower case etc

            // slug from title could be empty, e.g. the title is in Chinese
            // then we generate a random string of 6 chars
            if (slug.IsNullOrEmpty())
            {
                slug = Util.RandomString(6);
            }

            // make sure slug is unique
            int i = 2;
            if (existingSlugs != null)
            {
                while (existingSlugs.Contains(slug))
                {
                    slug = $"{slug}-{i}";
                    i++;
                }
            }

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
