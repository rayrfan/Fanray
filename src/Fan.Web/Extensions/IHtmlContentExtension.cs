using System.IO;
using System.Text.Encodings.Web;

namespace Microsoft.AspNetCore.Html
{
    public static class IHtmlContentExtension
    {
        /// <summary>
        /// Converts IHtmlContent to string.
        /// </summary>
        /// <param name="content"></param>
        /// <returns></returns>
        /// <remarks>
        /// https://stackoverflow.com/a/38507454/32240
        /// </remarks>
        public static string GetString(this IHtmlContent content)
        {
            using (var writer = new StringWriter())
            {
                content.WriteTo(writer, HtmlEncoder.Default);
                return writer.ToString();
            }
        }
    }
}
