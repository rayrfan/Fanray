using Fan.Blog.MetaWeblog.Models;
using Microsoft.Extensions.Logging;
using System;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Xml.Linq;

namespace Fan.Blog.MetaWeblog
{
    /// <summary>
    /// Helps parsing and composing of the xmlrpc request and response data.
    /// </summary>
    /// <remarks>
    /// References:
    /// http://xmlrpc.scripting.com/spec.html
    /// https://codex.wordpress.org/XML-RPC_MetaWeblog_API
    /// https://codex.wordpress.org/XML-RPC_wp
    /// </remarks>
    public class XmlRpcHelper : IXmlRpcHelper
    {
        private ILogger<XmlRpcHelper> _logger;

        public XmlRpcHelper(ILogger<XmlRpcHelper> logger)
        {
            _logger = logger;
        }

        // -------------------------------------------------------------------- Request

        /// <summary>
        /// Parses and returns <see cref="XmlRpcRequest"/> object given an xml input string from OLW.
        /// </summary>
        /// <param name="xml"></param>
        /// <returns></returns>
        public XmlRpcRequest ParseRequest(string xml)
        {
            var request = new XmlRpcRequest();

            XDocument doc = XDocument.Parse(xml);
            request.MethodName = doc.Element("methodCall").Element("methodName").Value;
            var paramList = doc.Element("methodCall").Element("params").Elements("param").ToList();

            switch (request.MethodName)
            {
                case "metaWeblog.newPost":
                    request.BlogId = paramList[0].Value;
                    request.UserName = paramList[1].Value;
                    request.Password = paramList[2].Value;
                    request.Post = ParseMetaPost(paramList[3]);
                    request.Publish = paramList[4].Value != "0" && paramList[4].Value != "false";
                    break;

                case "metaWeblog.editPost":
                    request.PostId = paramList[0].Value;
                    request.UserName = paramList[1].Value;
                    request.Password = paramList[2].Value;
                    request.Post = ParseMetaPost(paramList[3]);
                    request.Publish = paramList[4].Value != "0" && paramList[4].Value != "false";
                    break;

                case "metaWeblog.getPost":
                    request.PostId = paramList[0].Value;
                    request.UserName = paramList[1].Value;
                    request.Password = paramList[2].Value;
                    break;

                case "metaWeblog.getRecentPosts":
                case "wp.getPageList":
                case "wp.getPages":
                    request.BlogId = paramList[0].Value;
                    request.UserName = paramList[1].Value;
                    request.Password = paramList[2].Value;
                    request.NumberOfPosts = int.Parse(paramList[3].Value);
                    break;

                case "blogger.deletePost":
                    request.AppKey = paramList[0].Value;
                    request.PostId = paramList[1].Value;
                    request.UserName = paramList[2].Value;
                    request.Password = paramList[3].Value;
                    request.Publish = paramList[4].Value != "0" && paramList[4].Value != "false";
                    break;

                case "metaWeblog.newMediaObject":
                    request.BlogId = paramList[0].Value;
                    request.UserName = paramList[1].Value;
                    request.Password = paramList[2].Value;
                    request.MediaObject = ParseMetaMediaObject(paramList[3]);
                    break;

                case "metaWeblog.getCategories":
                case "wp.getCategories":
                case "wp.getAuthors":
                case "wp.getTags":
                    request.BlogId = paramList[0].Value;
                    request.UserName = paramList[1].Value;
                    request.Password = paramList[2].Value;
                    break;

                // https://codex.wordpress.org/XML-RPC_wp#wp.newCategory
                case "wp.newCategory": 
                    request.BlogId = paramList[0].Value;
                    request.UserName = paramList[1].Value;
                    request.Password = paramList[2].Value;
                    request.CategoryTitle = paramList[3].Element("value").Element("struct").Elements("member").First(e => e.Element("name").Value == "name").Element("value").Value;
                    break;

                case "blogger.getUsersBlogs":
                case "metaWeblog.getUsersBlogs":
                    request.AppKey = paramList[0].Value;
                    request.UserName = paramList[1].Value;
                    request.Password = paramList[2].Value;
                    break;

                default:
                    throw new MetaWeblogException(EMetaWeblogCode.UnknownMethod, $"Unknown Method. ({request.MethodName})");
            }

            return request;
        }

        /// <summary>
        /// Returns <see cref="MetaPost"/> given the param that holds the list of members.
        /// </summary>
        /// <param name="ele"></param>
        /// <returns></returns>
        private MetaPost ParseMetaPost(XElement ele)
        {
            var post = new MetaPost();

            var memberList = ele.Element("value").Element("struct").Elements("member");

            // title
            var title = memberList.SingleOrDefault(m => m.Element("name").Value == "title");
            post.Title = title?.Element("value").Value;

            // description
            var description = memberList.SingleOrDefault(m => m.Element("name").Value == "description");
            post.Description = description?.Element("value").Value;

            // link
            var link = memberList.SingleOrDefault(m => m.Element("name").Value == "link");
            post.Link = link?.Element("value").Value;

            // allowComments
            var allowComments = memberList.SingleOrDefault(m => m.Element("name").Value == "mt_allow_comments");
            post.CommentPolicy = allowComments?.Element("value").Value;

            // excerpt
            var excerpt = memberList.SingleOrDefault(m => m.Element("name").Value == "mt_excerpt");
            post.Excerpt = excerpt?.Element("value").Value;

            // slug
            var slug = memberList.SingleOrDefault(m => m.Element("name").Value == "wp_slug");
            post.Slug = slug?.Element("value").Value;

            // author
            var authorId = memberList.SingleOrDefault(m => m.Element("name").Value == "wp_author_id");
            post.AuthorId = authorId?.Element("value").Value;

            // categories
            var categories = memberList.SingleOrDefault(m => m.Element("name").Value == "categories");
            if (categories != null)
            {
                var str = categories.Descendants("string").FirstOrDefault();
                if (str!=null) post.Categories.Add(str.Value);
            }

            // postDate 
            try
            {
                var dateCreated = memberList.SingleOrDefault(m => m.Element("name").Value == "dateCreated");
                var pubDate = memberList.SingleOrDefault(m => m.Element("name").Value == "pubDate");

                if (dateCreated != null)
                {
                    post.PostDate = DateTimeOffset.ParseExact(dateCreated.Element("value").Value,
                            "yyyyMMdd'T'HH':'mm':'ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
                }
                else if (pubDate != null)
                {
                    post.PostDate = DateTimeOffset.ParseExact(pubDate.Element("value").Value,
                            "yyyyMMdd'T'HH':'mm':'ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
                }
            }
            catch (Exception ex)
            {
                post.PostDate = DateTimeOffset.MinValue;
                _logger.LogError(ex.Message);
            }

            // tags
            var keywords = memberList.SingleOrDefault(m => m.Element("name").Value == "mt_keywords");
            if (keywords != null)
            {
                var tagsList = keywords.Element("value").Value; // comma-delimited

                foreach (var item in
                    tagsList.Split(',').Where(item => string.IsNullOrEmpty(post.Tags.Find(t => t.Equals(item.Trim(), StringComparison.OrdinalIgnoreCase)))))
                {
                    post.Tags.Add(item.Trim());
                }
            }

            _logger.LogDebug("ParseMetaPost {@MetaPost}", post);
            return post;
        }

        /// <summary>
        /// Returns a <see cref="MetaMediaObject"/> given the XElement that contains the data.
        /// </summary>
        private MetaMediaObject ParseMetaMediaObject(XElement ele)
        {
            var memberList = ele.Element("value").Element("struct").Elements("member");

            var name = memberList.SingleOrDefault(m => m.Element("name").Value == "name");
            var type = memberList.SingleOrDefault(m => m.Element("name").Value == "type");
            var bits = memberList.SingleOrDefault(m => m.Element("name").Value == "bits");

            var mediaObj = new MetaMediaObject
            {
                Name = name?.Element("value").Value,
                Type = type?.Element("value").Value,
                Bits = Convert.FromBase64String(bits == null ? string.Empty : bits.Element("value").Value)
            };

            _logger.LogDebug("ParseMetaMediaObject {@MetaMediaObject}", mediaObj);
            return mediaObj;
        }

        // -------------------------------------------------------------------- Response

        public string BuildFaultOutput(MetaWeblogException ex)
        {
            return BuildFaultOutput((int) ex.Code, ex.Message);   
        }

        public string BuildFaultOutput(int faultCode, string faultString)
        {
            var doc = new XDocument(
                new XElement("methodResponse",
                    new XElement("fault",
                        new XElement("value",
                            new XElement("struct",
                                new XElement("member",
                                    new XElement("name", "faultCode"),
                                    new XElement("value", new XElement("int", faultCode))),
                                new XElement("member",
                                    new XElement("name", "faultString"),
                                    new XElement("value", new XElement("string", faultString))))))));

            var sw = new StringWriter();
            doc.Save(sw);
            return sw.ToString();
        }

        public string BuildOutput(string methodName, XmlRpcResponse response)
        {            
            XElement ele = null;
            switch (methodName)
            {
                case "metaWeblog.newPost":
                    ele = BuildNewPost(response);
                    break;

                case "metaWeblog.editPost":
                case "blogger.deletePost":
                    ele = BuildBool(response);
                    break;

                case "metaWeblog.getPost":
                    ele = BuildGetPost(response);
                    break;

                case "metaWeblog.newMediaObject":
                    ele = BuildNewMediaInfo(response);
                    break;

                case "metaWeblog.getCategories":
                case "wp.getCategories":
                    ele = BuildGetCategories(response);
                    break;

                case "wp.newCategory":
                    ele = BuildNewCategory(response);
                    break;

                case "metaWeblog.getRecentPosts":
                    ele = BuildRecentPosts(response);
                    break;

                case "blogger.getUsersBlogs":
                case "metaWeblog.getUsersBlogs":
                    ele = BuildGetUsersBlogs(response);
                    break;

                case "wp.getTags":
                    ele = BuildGetTags(response);
                    break;

                default:
                    throw new MetaWeblogException(EMetaWeblogCode.UnknownMethod, $"Unknown Method ({methodName}).");
            }

            var doc = new XDocument(new XElement("methodResponse",
                new XElement("params",
                    new XElement("param",
                        new XElement("value", ele)))));

            var sw = new StringWriter();
            doc.Save(sw);
            return sw.ToString();
        }

        /// <summary>
        /// wp.getTags
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        private XElement BuildGetTags(XmlRpcResponse response)
        {
            var data = new XElement("data");

            if (response.Keywords != null)
            {
                foreach (var tag in response.Keywords)
                {
                    data.Add(new XElement("value",
                                new XElement("struct",
                                    new XElement("member",
                                        new XElement("name", "name"),
                                            new XElement("value",
                                                new XElement("string", tag))))));
                }
            }

            return new XElement("array", data); 
        }

        /// <summary>
        /// metaWeblog.editPost, blogger.deletePost
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        private XElement BuildBool(XmlRpcResponse response)
        {
            return new XElement("boolean", response.Completed ? "1" : "0");
        }

        /// <summary>
        /// metaWeblog.getUsersBlogs
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        private XElement BuildGetUsersBlogs(XmlRpcResponse response)
        {
            XElement data = new XElement("data");

            if (response.BlogInfos != null)
            {
                foreach (var blog in response.BlogInfos)
                {
                    data.Add(new XElement("value",
                        new XElement("struct",
                            new XElement("member",
                                new XElement("name", "url"),
                                new XElement("value", blog.Url)),
                            new XElement("member",
                                new XElement("name", "blogid"),
                                new XElement("value", blog.BlogId)),
                            new XElement("member",
                                new XElement("name", "blogName"),
                                new XElement("value", blog.BlogName))
                            )));
                }
            }

            return new XElement("array", data);
        }

        /// <summary>
        /// metaWeblog.getRecentPosts
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        private XElement BuildRecentPosts(XmlRpcResponse response)
        {
            XElement data = new XElement("data");

            foreach (var post in response.Posts)
            {
                data.Add(new XElement("value", BuildPost(post)));
            }

            return new XElement("array", data);
        }

        /// <summary>
        /// metaWeblog.getCategories
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        /// <remarks>
        /// Looks like wp and metaweblog returns different types back, I'm returning 
        /// without explicit type of string or int and see if it works.
        /// https://codex.wordpress.org/XML-RPC_MetaWeblog_API#metaWeblog.getCategories
        /// https://codex.wordpress.org/XML-RPC_wp#wp.getCategories
        /// </remarks>
        private XElement BuildGetCategories(XmlRpcResponse response)
        {
            XElement data = new XElement("data");

            if (response.Categories != null)
            {
                foreach (var cat in response.Categories)
                {
                    data.Add(new XElement("value",
                                new XElement("struct",
                                    new XElement("member",
                                        new XElement("name", "description"),
                                        new XElement("value", cat.Description)),
                                    new XElement("member",
                                        new XElement("name", "categoryid"),
                                        new XElement("value", cat.Id)),
                                    new XElement("member",
                                        new XElement("name", "title"),
                                        new XElement("value", cat.Title)),
                                    new XElement("member",
                                        new XElement("name", "htmlUrl"),
                                        new XElement("value", cat.HtmlUrl)),
                                    new XElement("member",
                                        new XElement("name", "rssUrl"),
                                        new XElement("value", cat.RssUrl)))));
                }
            }

            return new XElement("array", data);
        }

        /// <summary>
        /// metaWeblog.newMediaObject
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        private XElement BuildNewMediaInfo(XmlRpcResponse response)
        {
            return new XElement("struct", 
                        new XElement("member",
                            new XElement("name", "url"),
                            new XElement("value",
                                new XElement("string", response.MediaInfo.Url))));
        }

        /// <summary>
        /// metaWeblog.getPost
        /// </summary>
        /// <param name="response"></param>
        /// <returns></returns>
        private XElement BuildGetPost(XmlRpcResponse response)
        {
            return BuildPost(response.Post);
        }

        /// <summary>
        /// metaWeblog.newPost
        /// </summary>
        /// <param name="response"></param>
        /// <returns>string postid</returns>
        /// <remarks>
        /// https://codex.wordpress.org/XML-RPC_MetaWeblog_API#metaWeblog.newPost
        /// </remarks>
        private XElement BuildNewPost(XmlRpcResponse response)
        {
            return new XElement("string", response.PostId);
        }

        /// <summary>
        /// wp.newCategory
        /// </summary>
        /// <param name="response"></param>
        /// <returns>int category_id</returns>
        /// <remarks>
        /// https://codex.wordpress.org/XML-RPC_wp#wp.newCategory
        /// </remarks>
        private XElement BuildNewCategory(XmlRpcResponse response)
        {
            return new XElement("int", response.CategoryId);
        }

        /// <summary>
        /// Convert date to ISO8601 format.
        /// </summary>
        private string ConvertDatetoISO8601(DateTimeOffset date)
        {
            return string.Format("{0}{1}{2}T{3}:{4}:{5}",
                                date.Year,
                                date.Month.ToString().PadLeft(2, '0'),
                                date.Day.ToString().PadLeft(2, '0'),
                                date.Hour.ToString().PadLeft(2, '0'),
                                date.Minute.ToString().PadLeft(2, '0'),
                                date.Second.ToString().PadLeft(2, '0'));
        }

        private XElement BuildPost(MetaPost post)
        {
            var stru = new XElement("struct");

            stru.Add(new XElement("member",
                    new XElement("name", "postid"),
                    new XElement("value",
                        new XElement("string", post.PostId))));

            stru.Add(new XElement("member",
                    new XElement("name", "title"),
                    new XElement("value",
                        new XElement("string", post.Title))));

            stru.Add(new XElement("member",
                    new XElement("name", "description"),
                    new XElement("value",
                        new XElement("string", post.Description))));

            stru.Add(new XElement("member",
                new XElement("name", "link"),
                    new XElement("value",
                        new XElement("string", post.Link))));

            stru.Add(new XElement("member",
                new XElement("name", "wp_slug"),
                    new XElement("value",
                        new XElement("string", post.Slug))));

            stru.Add(new XElement("member",
                new XElement("name", "mt_excerpt"),
                    new XElement("value",
                        new XElement("string", post.Excerpt))));

            stru.Add(new XElement("member",
                new XElement("name", "mt_allow_comments"),
                    new XElement("value",
                        new XElement("int", post.CommentPolicy))));

            stru.Add(new XElement("member",
                new XElement("name", "dateCreated"),
                    new XElement("value",
                        new XElement("dateTime.iso8601", ConvertDatetoISO8601(post.PostDate)))));

            stru.Add(new XElement("member",
                new XElement("name", "publish"),
                    new XElement("value",
                        new XElement("boolean", post.Publish ? "1" : "0"))));

            if (post.Tags != null && post.Tags.Count > 0)
            {
                stru.Add(new XElement("member",
                   new XElement("name", "mt_keywords"),
                       new XElement("value",
                           new XElement("string", string.Join(",", post.Tags)))));
            }

            if (post.Categories != null && post.Categories.Count > 0)
            {
                XElement data = new XElement("data");
                foreach (var cat in post.Categories)
                {
                    data.Add(new XElement("value", new XElement("string", cat)));
                }

                stru.Add(new XElement("member",
                   new XElement("name", "categories"),
                       new XElement("value",
                           new XElement("array", data))));
            }

            return stru;
        }
    }
}
