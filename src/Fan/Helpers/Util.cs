using HtmlAgilityPack;
using Humanizer;
using System;
using System.Globalization;
using System.Linq;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using TimeZoneConverter;

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
        /// Returns a random lowercase alpha + numeric chars of a certain length.
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
        /// Removes all html tags from content.
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        public static string CleanHtml(string content)
        {
            if (content.IsNullOrEmpty()) return content;
            HtmlDocument document = new HtmlDocument();
            document.LoadHtml(content);
            return document.DocumentNode.InnerText;
        }

        /// <summary>
        /// Converts a time from the server to a user's local time with his specified timezone.
        /// </summary>
        /// <param name="serverTime"></param>
        /// <param name="timeZoneId">The timezone to convert server time to.</param>
        /// <returns></returns>
        /// <remarks>
        /// Server saves all posts with DateTimeOffset.UtcNow, when a post is shown in browser it's 
        /// shows either a humanized string if the post was published within 2 days, or an actual
        /// date time string converted in <see cref="CoreSettings.TimeZoneId"/> timezone.
        /// </remarks>
        public static DateTimeOffset ConvertTime(DateTimeOffset serverTime, string timeZoneId)
        {
            var userTimeZone = TZConvert.GetTimeZoneInfo(timeZoneId);
            return TimeZoneInfo.ConvertTime(serverTime, userTimeZone);
        }

        /// <summary>
        /// A util that uses regular expression to verify if a string is in valid email format.
        /// </summary>
        /// <remarks>
        /// https://docs.microsoft.com/en-us/dotnet/standard/base-types/how-to-verify-that-strings-are-in-valid-email-format
        /// </remarks>
        public static bool IsValidEmail(string strIn)
        {
            if (String.IsNullOrEmpty(strIn))
                return false;

            // Use IdnMapping class to convert Unicode domain names.
            try
            {
                strIn = Regex.Replace(strIn, @"(@)(.+)$", DomainMapper,
                                      RegexOptions.None, TimeSpan.FromMilliseconds(200));
                if (string.IsNullOrEmpty(strIn))
                    return false;
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
            // Return true if strIn is in valid email format.
            try
            {
                return Regex.IsMatch(strIn,
                      @"^(?("")("".+?(?<!\\)""@)|(([0-9a-z]((\.(?!\.))|[-!#\$%&'\*\+/=\?\^`\{\}\|~\w])*)(?<=[0-9a-z])@))" +
                      @"(?(\[)(\[(\d{1,3}\.){3}\d{1,3}\])|(([0-9a-z][-0-9a-z]*[0-9a-z]*\.)+[a-z0-9][\-a-z0-9]{0,22}[a-z0-9]))$",
                      RegexOptions.IgnoreCase, TimeSpan.FromMilliseconds(250));
            }
            catch (RegexMatchTimeoutException)
            {
                return false;
            }
        }

        private static string DomainMapper(Match match)
        {
            // IdnMapping class with default property values.
            IdnMapping idn = new IdnMapping();

            string domainName = match.Groups[2].Value;
            try
            {
                domainName = idn.GetAscii(domainName);
            }
            catch (ArgumentException)
            {
                return null;
            }
            return match.Groups[1].Value + domainName;
        }
    }
}
