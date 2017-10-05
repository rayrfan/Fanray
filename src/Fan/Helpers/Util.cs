using AutoMapper;
using Fan.Models;
using HtmlAgilityPack;
using Humanizer;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Text;

namespace Fan.Helpers
{
    public static class Util
    {
        /// <summary>
        /// Produces optional, URL-friendly version of a title, "like-this-one". 
        /// hand-tuned for speed, reflects performance refactoring contributed by John Gietzen (user otac0n) 
        /// </summary>
        /// <remarks>
        /// http://stackoverflow.com/questions/25259/how-does-stackoverflow-generate-its-seo-friendly-urls
        /// </remarks>
        public static string FormatSlug(string title)
        {
            if (title == null) return "";

            const int maxlen = 80;
            int len = title.Length;
            bool prevdash = false;
            var sb = new StringBuilder(len);
            char c;

            for (int i = 0; i < len; i++)
            {
                c = title[i];
                if ((c >= 'a' && c <= 'z') || (c >= '0' && c <= '9'))
                {
                    sb.Append(c);
                    prevdash = false;
                }
                else if (c >= 'A' && c <= 'Z')
                {
                    // tricky way to convert to lowercase            
                    sb.Append((char)(c | 32));
                    prevdash = false;
                }
                else if (c == ' ' || c == ',' || c == '.' || c == '/' || c == '\\' || c == '-' || c == '_' || c == '=')
                {
                    if (!prevdash && sb.Length > 0)
                    { sb.Append('-'); prevdash = true; }
                }
                else if ((int)c >= 128)
                {
                    int prevlen = sb.Length;
                    sb.Append(RemapInternationalCharToAscii(c));
                    if (prevlen != sb.Length) prevdash = false;
                }
                if (i == maxlen) break;
            }

            if (prevdash)
                return sb.ToString().Substring(0, sb.Length - 1);
            else
                return sb.ToString();
        }

        /// <summary>
        /// Returns an ascii char for an international char.
        /// </summary>
        /// <param name="c"></param>
        /// <returns></returns>
        /// <remarks>
        /// https://meta.stackexchange.com/questions/7435/non-us-ascii-characters-dropped-from-full-profile-url/7696#7696
        /// </remarks>
        public static string RemapInternationalCharToAscii(char c)
        {
            string s = c.ToString().ToLowerInvariant();
            if ("àåáâäãåa".Contains(s))
            {
                return "a";
            }
            else if ("èéêëe".Contains(s))
            {
                return "e";
            }
            else if ("ìíîïi".Contains(s))
            {
                return "i";
            }
            else if ("òóôõöøo".Contains(s))
            {
                return "o";
            }
            else if ("ùúûü".Contains(s))
            {
                return "u";
            }
            else if ("çcc".Contains(s))
            {
                return "c";
            }
            else if ("zzž".Contains(s))
            {
                return "z";
            }
            else if ("ssš".Contains(s))
            {
                return "s";
            }
            else if ("ñn".Contains(s))
            {
                return "n";
            }
            else if ("ýŸ".Contains(s))
            {
                return "y";
            }
            else if (c == 'l')
            {
                return "l";
            }
            else if (c == 'd')
            {
                return "d";
            }
            else if (c == 'ß')
            {
                return "ss";
            }
            else if (c == 'g')
            {
                return "g";
            }
            else if (c == 'Þ')
            {
                return "th";
            }
            else
            {
                return "";
            }
        }

        /// <summary>
        /// Returns a random alphanumeric of a certain length.
        /// </summary>
        /// <param name="length"></param>
        /// <remarks>
        /// https://stackoverflow.com/questions/1344221/how-can-i-generate-random-alphanumeric-strings-in-c
        /// https://stackoverflow.com/a/9995910/32240
        /// https://stackoverflow.com/a/1344242/32240
        /// </remarks>
        public static string RandomString(int length)
        {
            Random random = new Random();
            string input = "abcdefghijklmnopqrstuvwxyz0123456789";
            var chars = Enumerable.Repeat(0, length) // Range would give the same end result
                                   .Select(x => input[random.Next(0, input.Length)]);
            return new string(chars.ToArray());
        }

        /// <summary>
        /// Returns excerpt give body of a post. Returns empty string if body is null or operation
        /// fails. The returned string 
        /// </summary>
        /// <param name="body"></param>
        /// <param name="wordsLimit"></param>
        /// <returns></returns>
        /// <remarks>
        /// - I noticed flipboard on the web uses cleaned up exerpts
        /// - Stripping all html tags with Html Agility Pack http://stackoverflow.com/a/3140991/32240
        /// </remarks>
        public static string GetExcerpt(string body, int wordsLimit)
        {
            if (string.IsNullOrEmpty(body) || wordsLimit <= 0) return "";

            try
            {
                HtmlDocument document = new HtmlDocument();
                document.LoadHtml(body);
                body = document.DocumentNode.InnerText?.Trim(); // should be clean text by now
                if (body.IsNullOrEmpty()) return "";

                // html entities https://stackoverflow.com/a/10971380/32240
                body = WebUtility.HtmlDecode(body); 

                return body.Truncate(wordsLimit, Truncator.FixedNumberOfWords);
            }
            catch (Exception e)
            {
                return body;
            }
        }

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
            if (input.Length > Const.TAXONOMY_TITLE_SLUG_MAXLEN)
            {
                input = input.Substring(0, Const.TAXONOMY_TITLE_SLUG_MAXLEN);
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
                    cfg.CreateMap<Post, Media>();
                    cfg.CreateMap<Media, Post>();
                }).CreateMapper();
            }
        }
    }
}
