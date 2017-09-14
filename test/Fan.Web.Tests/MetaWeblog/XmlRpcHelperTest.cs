using Fan.Web.MetaWeblog;
using Fan.Web.MetaWeblog.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using Xunit;

namespace Fan.Web.Tests.MetaWeblog
{
    public class XmlRpcHelperTest
    {
        private IXmlRpcHelper _helper;
        public XmlRpcHelperTest()
        {
            var serviceProvider = new ServiceCollection().AddLogging().BuildServiceProvider();
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            _helper = new XmlRpcHelper(loggerFactory.CreateLogger<XmlRpcHelper>());
        }

        // -------------------------------------------------------------------- Request tests

        #region Xml Request Strings

        #region newPost

        private const string REQ_NEWPOST = @"<?xml version=""1.0"" encoding=""utf-8""?>
<methodCall>
 <methodName>metaWeblog.newPost</methodName>
 <params>
  <param>
   <value>
    <string>http://localhost:8000</string>
   </value>
  </param>
  <param>
   <value>
    <string>admin</string>
   </value>
  </param>
  <param>
   <value>
    <string>admin</string>
   </value>
  </param>
  <param>
   <value>
    <struct>
     <member>
      <name>title</name>
      <value>
       <string>Welcome to Fanray</string>
      </value>
     </member>
     <member>
      <name>description</name>
      <value>
       <string>&lt;p&gt;This is a test post.&lt;/p&gt;</string>
      </value>
     </member>
     <member>
      <name>mt_keywords</name>
      <value>
       <string>blogging, testing</string>
      </value>
     </member>
     <member>
      <name>wp_slug</name>
      <value>
       <string>welcome-to-fanray</string>
      </value>
     </member>
     <member>
      <name>mt_basename</name>
      <value>
       <string>welcome-to-fanray</string>
      </value>
     </member>
     <member>
      <name>categories</name>
      <value>
       <array>
        <data>
         <value>
          <string>Technology</string>
         </value>
        </data>
       </array>
      </value>
     </member>
     <member>
      <name>dateCreated</name>
      <value>
       <dateTime.iso8601>20170907T15:08:00</dateTime.iso8601>
      </value>
     </member>
     <member>
      <name>date_created_gmt</name>
      <value>
       <dateTime.iso8601>20170907T15:08:00</dateTime.iso8601>
      </value>
     </member>
     <member>
      <name>mt_excerpt</name>
      <value>
       <string>This is an excerpt.</string>
      </value>
     </member>
    </struct>
   </value>
  </param>
  <param>
   <value>
    <boolean>1</boolean>
   </value>
  </param>
 </params>
</methodCall>";

        #endregion

        #region editPost

        private const string XML_EDITPOST = @"<?xml version=""1.0"" encoding=""utf-8""?>
<methodCall>
 <methodName>metaWeblog.editPost</methodName>
 <params>
  <param>
   <value>
    <string>2</string>
   </value>
  </param>
  <param>
   <value>
    <string>admin</string>
   </value>
  </param>
  <param>
   <value>
    <string>admin</string>
   </value>
  </param>
  <param>
   <value>
    <struct>
     <member>
      <name>title</name>
      <value>
       <string>Welcome to Fanray</string>
      </value>
     </member>
     <member>
      <name>description</name>
      <value>
       <string>&lt;p&gt;This is an update to test post.&lt;/p&gt;</string>
      </value>
     </member>
     <member>
      <name>mt_keywords</name>
      <value>
       <string>blogging, testing</string>
      </value>
     </member>
     <member>
      <name>wp_slug</name>
      <value>
       <string>welcome-to-fanray</string>
      </value>
     </member>
     <member>
      <name>mt_basename</name>
      <value>
       <string>welcome-to-fanray</string>
      </value>
     </member>
     <member>
      <name>categories</name>
      <value>
       <array>
        <data>
         <value>
          <string>Technology</string>
         </value>
        </data>
       </array>
      </value>
     </member>
     <member>
      <name>dateCreated</name>
      <value>
       <dateTime.iso8601>20170907T15:08:00</dateTime.iso8601>
      </value>
     </member>
     <member>
      <name>date_created_gmt</name>
      <value>
       <dateTime.iso8601>20170907T15:08:00</dateTime.iso8601>
      </value>
     </member>
     <member>
      <name>mt_excerpt</name>
      <value>
       <string>This is an excerpt.</string>
      </value>
     </member>
    </struct>
   </value>
  </param>
  <param>
   <value>
    <boolean>1</boolean>
   </value>
  </param>
 </params>
</methodCall>";

        #endregion

        #region getPost

        private const string XML_GETPOST = @"<?xml version=""1.0"" encoding=""utf-8""?>
<methodCall>
 <methodName>metaWeblog.getPost</methodName>
 <params>
  <param>
   <value>
    <string>2</string>
   </value>
  </param>
  <param>
   <value>
    <string>admin</string>
   </value>
  </param>
  <param>
   <value>
    <string>admin</string>
   </value>
  </param>
 </params>
</methodCall>";

        #endregion

        #region getRecentPosts

        private const string XML_GETRECENTPOSTS = @"<?xml version=""1.0"" encoding=""utf-8""?>
<methodCall>
 <methodName>metaWeblog.getRecentPosts</methodName>
 <params>
  <param>
   <value>
    <string>http://localhost:8000</string>
   </value>
  </param>
  <param>
   <value>
    <string>admin</string>
   </value>
  </param>
  <param>
   <value>
    <string>admin</string>
   </value>
  </param>
  <param>
   <value>
    <int>50</int>
   </value>
  </param>
 </params>
</methodCall>";

        #endregion

        #region deletePost

        public const string XML_DELETEPOST = @"<?xml version=""1.0"" encoding=""utf-8""?>
<methodCall>
 <methodName>blogger.deletePost</methodName>
 <params>
  <param>
   <value>
    <string>0123456789ABCDEF</string>
   </value>
  </param>
  <param>
   <value>
    <string>2</string>
   </value>
  </param>
  <param>
   <value>
    <string>admin</string>
   </value>
  </param>
  <param>
   <value>
    <string>admin</string>
   </value>
  </param>
  <param>
   <value>
    <boolean>1</boolean>
   </value>
  </param>
 </params>
</methodCall>";

        #endregion

        #region getTags

        private const string XML_GETTAGS = @"<?xml version=""1.0"" encoding=""utf-8""?>
<methodCall>
 <methodName>wp.getTags</methodName>
 <params>
  <param>
   <value>
    <string>http://localhost:8000</string>
   </value>
  </param>
  <param>
   <value>
    <string>admin</string>
   </value>
  </param>
  <param>
   <value>
    <string>admin</string>
   </value>
  </param>
 </params>
</methodCall>";
        #endregion

        #region getUsersBlogs

        private const string XML_GETUSERSBLOGS = @"<?xml version=""1.0"" encoding=""utf-8""?>
<methodCall>
 <methodName>blogger.getUsersBlogs</methodName>
 <params>
  <param>
   <value>
    <string>0123456789ABCDEF</string>
   </value>
  </param>
  <param>
   <value>
    <string>admin</string>
   </value>
  </param>
  <param>
   <value>
    <string>admin</string>
   </value>
  </param>
 </params>
</methodCall>";
        #endregion

        #region newCategory

        private const string XML_NEWCATEGORY = @"<?xml version=""1.0"" encoding=""utf-8""?>
<methodCall>
 <methodName>wp.newCategory</methodName>
 <params>
  <param>
   <value>
    <string>http://localhost:8000</string>
   </value>
  </param>
  <param>
   <value>
    <string>admin</string>
   </value>
  </param>
  <param>
   <value>
    <string>admin</string>
   </value>
  </param>
  <param>
   <value>
    <struct>
     <member>
      <name>name</name>
      <value>
       <string>New Cat</string>
      </value>
     </member>
     <member>
      <name>parent_id</name>
      <value>
       <int>0</int>
      </value>
     </member>
    </struct>
   </value>
  </param>
 </params>
</methodCall>";

        #endregion

        #region getCategories

        private const string XML_GETCATEGORIES = @"<?xml version=""1.0"" encoding=""utf-8""?>
<methodCall>
 <methodName>metaWeblog.getCategories</methodName>
 <params>
  <param>
   <value>
    <string>http://localhost:8000</string>
   </value>
  </param>
  <param>
   <value>
    <string>admin</string>
   </value>
  </param>
  <param>
   <value>
    <string>admin</string>
   </value>
  </param>
 </params>
</methodCall>";
        #endregion

        #region newMediaObject

        // omitting the base64 content
        public const string XML_NEWMEDIAOBJECT = @"<?xml version=""1.0"" encoding=""utf-8""?>
<methodCall>
 <methodName>metaWeblog.newMediaObject</methodName>
 <params>
  <param>
   <value>
    <string>http://localhost:8000</string>
   </value>
  </param>
  <param>
   <value>
    <string>admin</string>
   </value>
  </param>
  <param>
   <value>
    <string>admin</string>
   </value>
  </param>
  <param>
   <value>
    <struct>
     <member>
      <name>name</name>
      <value>
       <string>Open-Live-Writer/Welcome-to-Fanray_9925/20140822_201430_2.jpg</string>
      </value>
     </member>
     <member>
      <name>type</name>
      <value>
       <string>image/jpeg</string>
      </value>
     </member>
     <member>
      <name>bits</name>
      <value>
       <base64></base64>
      </value>
     </member>
    </struct>
   </value>
  </param>
 </params>
</methodCall>";

        #endregion

        #endregion

        /// <summary>
        /// metaWeblog.newPost
        /// </summary>
        [Fact]
        public void ParseRequest_Parses_metaWeblog_newPost_Method()
        {
            // Act
            var request = _helper.ParseRequest(REQ_NEWPOST);

            // Assert
            Assert.Equal("metaWeblog.newPost", request.MethodName);
            Assert.Equal("http://localhost:8000", request.BlogId);
            Assert.Equal("admin", request.UserName);
            Assert.Equal("admin", request.Password);

            Assert.Equal(null, request.Post.PostId);
            Assert.Equal("Welcome to Fanray", request.Post.Title);
            Assert.Equal("welcome-to-fanray", request.Post.Slug);
            Assert.Equal("Technology", request.Post.Categories[0]);
            Assert.Equal("blogging", request.Post.Tags[0]);
            Assert.Equal("testing", request.Post.Tags[1]);
            Assert.Equal(new DateTime(2017, 9, 7, 8, 8, 0), request.Post.PostDate); // 20170907T15:08:00

            Assert.Equal(true, request.Publish);
        }

        /// <summary>
        /// metaWeblog.editPost
        /// </summary>
        [Fact]
        public void ParseRequest_Parses_metaWeblog_editPost_Method()
        {
            var request = _helper.ParseRequest(XML_EDITPOST);

            Assert.Equal("metaWeblog.editPost", request.MethodName);
            Assert.Equal("2", request.PostId);
            Assert.Equal("admin", request.UserName);
            Assert.Equal("admin", request.Password);

            Assert.Equal(null, request.Post.PostId); // postId is set outside of post
            Assert.Equal("Welcome to Fanray", request.Post.Title);
            Assert.Equal("welcome-to-fanray", request.Post.Slug);
            Assert.Equal("Technology", request.Post.Categories[0]);
            Assert.Equal("blogging", request.Post.Tags[0]);
            Assert.Equal("testing", request.Post.Tags[1]);
            Assert.Equal(new DateTime(2017, 9, 7, 8, 8, 0), request.Post.PostDate); // 20170907T15:08:00

            Assert.Equal(true, request.Publish);
        }

        /// <summary>
        /// metaWeblog.getPost
        /// </summary>
        [Fact]
        public void ParseRequest_Parses_metaWeblog_getPost_Method()
        {
            var request = _helper.ParseRequest(XML_GETPOST);

            Assert.Equal("metaWeblog.getPost", request.MethodName);
            Assert.Equal("2", request.PostId);
            Assert.Equal("admin", request.UserName);
            Assert.Equal("admin", request.Password);
        }

        /// <summary>
        /// metaWeblog.getCategories
        /// </summary>
        [Fact]
        public void ParseRequest_Parses_metaWeblog_getCategories_Method()
        {
            var request = _helper.ParseRequest(XML_GETCATEGORIES);

            Assert.Equal("metaWeblog.getCategories", request.MethodName);
            Assert.Equal("http://localhost:8000", request.BlogId);
            Assert.Equal("admin", request.UserName);
            Assert.Equal("admin", request.Password);
        }

        /// <summary>
        /// wp.newCategory
        /// </summary>
        [Fact]
        public void ParseRequest_Parses_wp_newCategory_Method()
        {
            var request = _helper.ParseRequest(XML_NEWCATEGORY);

            Assert.Equal("wp.newCategory", request.MethodName);
            Assert.Equal("http://localhost:8000", request.BlogId);
            Assert.Equal("admin", request.UserName);
            Assert.Equal("admin", request.Password);
            Assert.Equal("New Cat", request.CategoryTitle);
        }

        /// <summary>
        /// metaWeblog.getRecentPosts
        /// </summary>
        [Fact]
        public void ParseRequest_Parses_metaWeblog_getRecentPosts_Method()
        {
            var request = _helper.ParseRequest(XML_GETRECENTPOSTS);

            Assert.Equal("metaWeblog.getRecentPosts", request.MethodName);
            Assert.Equal("http://localhost:8000", request.BlogId);
            Assert.Equal("admin", request.UserName);
            Assert.Equal("admin", request.Password);
            Assert.Equal(50, request.NumberOfPosts);
        }

        /// <summary>
        /// blogger.getUsersBlogs
        /// </summary>
        [Fact]
        public void ParseRequest_Parses_blogger_getUsersBlogs_Method()
        {
            var request = _helper.ParseRequest(XML_GETUSERSBLOGS);

            Assert.Equal("blogger.getUsersBlogs", request.MethodName);
            Assert.Equal("0123456789ABCDEF", request.AppKey);
            Assert.Equal("admin", request.UserName);
            Assert.Equal("admin", request.Password);
        }

        /// <summary>
        /// blogger.deletePost
        /// </summary>
        [Fact]
        public void ParseRequest_Parses_blogger_deletePost_Method()
        {
            var request = _helper.ParseRequest(XML_DELETEPOST);

            Assert.Equal("blogger.deletePost", request.MethodName);
            Assert.Equal("0123456789ABCDEF", request.AppKey);
            Assert.Equal("2", request.PostId);
            Assert.Equal("admin", request.UserName);    
            Assert.Equal("admin", request.Password);
            Assert.Equal(true, request.Publish);
        }

        /// <summary>
        /// metaWeblog.newMediaObject
        /// </summary>
        [Fact]
        public void ParseRequest_Parses_metaWeblog_newMediaObject_Method()
        {
            var request = _helper.ParseRequest(XML_NEWMEDIAOBJECT);

            Assert.Equal("metaWeblog.newMediaObject", request.MethodName);
            Assert.Equal("http://localhost:8000", request.BlogId);
            Assert.Equal("admin", request.UserName);
            Assert.Equal("admin", request.Password);

            Assert.Equal("Open-Live-Writer/Welcome-to-Fanray_9925/20140822_201430_2.jpg", request.MediaObject.Name);
            Assert.Equal("image/jpeg", request.MediaObject.Type);
            Assert.Equal(new byte[0], request.MediaObject.Bits);
        }

        // -------------------------------------------------------------------- Response tests

        #region Xml Response Strings

        private const string RESP_FAULT = @"<?xml version=""1.0"" encoding=""utf-16""?>
<methodResponse>
  <fault>
    <value>
      <struct>
        <member>
          <name>faultCode</name>
          <value>
            <int>1001</int>
          </value>
        </member>
        <member>
          <name>faultString</name>
          <value>
            <string>Invalid request.</string>
          </value>
        </member>
      </struct>
    </value>
  </fault>
</methodResponse>";

        private const string RESP_NEWPOST = @"<?xml version=""1.0"" encoding=""utf-16""?>
<methodResponse>
  <params>
    <param>
      <value>
        <string>2</string>
      </value>
    </param>
  </params>
</methodResponse>";

        private const string RESP_GETPOST = @"<?xml version=""1.0"" encoding=""utf-16""?>
<methodResponse>
  <params>
    <param>
      <value>
        <struct>
          <member>
            <name>postid</name>
            <value>
              <string>2</string>
            </value>
          </member>
          <member>
            <name>title</name>
            <value>
              <string>Test Post</string>
            </value>
          </member>
          <member>
            <name>description</name>
            <value>
              <string>Body of post.</string>
            </value>
          </member>
          <member>
            <name>link</name>
            <value>
              <string />
            </value>
          </member>
          <member>
            <name>wp_slug</name>
            <value>
              <string>test-post</string>
            </value>
          </member>
          <member>
            <name>mt_excerpt</name>
            <value>
              <string />
            </value>
          </member>
          <member>
            <name>mt_allow_comments</name>
            <value>
              <int />
            </value>
          </member>
          <member>
            <name>dateCreated</name>
            <value>
              <dateTime.iso8601>20170908T13:34:05</dateTime.iso8601>
            </value>
          </member>
          <member>
            <name>publish</name>
            <value>
              <boolean>1</boolean>
            </value>
          </member>
          <member>
            <name>mt_keywords</name>
            <value>
              <string>C#,Asp.net</string>
            </value>
          </member>
          <member>
            <name>categories</name>
            <value>
              <array>
                <data>
                  <value>
                    <string>Technology</string>
                  </value>
                </data>
              </array>
            </value>
          </member>
        </struct>
      </value>
    </param>
  </params>
</methodResponse>";

        private const string RESP_NEWMEDIA = @"<?xml version=""1.0"" encoding=""utf-16""?>
<methodResponse>
  <params>
    <param>
      <value>
        <struct>
          <member>
            <name>url</name>
            <value>
              <string>http://localhost/path/to/image.jpg</string>
            </value>
          </member>
        </struct>
      </value>
    </param>
  </params>
</methodResponse>";

        private const string RESP_GETCATEGORIES = @"<?xml version=""1.0"" encoding=""utf-16""?>
<methodResponse>
  <params>
    <param>
      <value>
        <array>
          <data>
            <value>
              <struct>
                <member>
                  <name>description</name>
                  <value>Test</value>
                </member>
                <member>
                  <name>categoryid</name>
                  <value>2</value>
                </member>
                <member>
                  <name>title</name>
                  <value>Test</value>
                </member>
                <member>
                  <name>htmlUrl</name>
                  <value>https://localhost:44381/category/test</value>
                </member>
                <member>
                  <name>rssUrl</name>
                  <value></value>
                </member>
              </struct>
            </value>
            <value>
              <struct>
                <member>
                  <name>description</name>
                  <value>Uncategorized</value>
                </member>
                <member>
                  <name>categoryid</name>
                  <value>1</value>
                </member>
                <member>
                  <name>title</name>
                  <value>Uncategorized</value>
                </member>
                <member>
                  <name>htmlUrl</name>
                  <value>https://localhost:44381/category/uncategorized</value>
                </member>
                <member>
                  <name>rssUrl</name>
                  <value></value>
                </member>
              </struct>
            </value>
          </data>
        </array>
      </value>
    </param>
  </params>
</methodResponse>";

        private const string RESP_GETTAGS = @"<?xml version=""1.0"" encoding=""utf-16""?>
<methodResponse>
  <params>
    <param>
      <value>
        <array>
          <data>
            <value>
              <struct>
                <member>
                  <name>name</name>
                  <value>
                    <string>C#</string>
                  </value>
                </member>
              </struct>
            </value>
            <value>
              <struct>
                <member>
                  <name>name</name>
                  <value>
                    <string>Asp.net</string>
                  </value>
                </member>
              </struct>
            </value>
          </data>
        </array>
      </value>
    </param>
  </params>
</methodResponse>";

        #endregion

        /// <summary>
        /// Test <see cref="XmlRpcHelper.BuildFaultOutput(MetaWeblogException)"/> method.
        /// </summary>
        [Fact]
        public void BuildFaultOutput_With_Exception()
        {
            // Arrange
            var ex = new MetaWeblogException(EMetaWeblogCode.InvalidRequest, "Invalid request.");

            // Act
            var output = _helper.BuildFaultOutput(ex);

            // Assert
            Assert.Equal(RESP_FAULT, output);
        }

        /// <summary>
        /// Test <see cref="XmlRpcHelper.BuildOutput(string, XmlRpcResponse)"/> for metaWeblog.newPost.
        /// </summary>
        [Fact]
        public void BuildOutput_newPost()
        {
            // Arrange
            var resp = new XmlRpcResponse
            {
                PostId = "2"
            };

            // Act
            var output = _helper.BuildOutput("metaWeblog.newPost", resp);

            // Assert
            Assert.Equal(RESP_NEWPOST, output);
        }

        /// <summary>
        /// Test <see cref="XmlRpcHelper.BuildOutput(string, XmlRpcResponse)"/> for getPost.
        /// </summary>
        [Fact]
        public void BuildOutput_getPost()
        {
            // Arrange
            var resp = new XmlRpcResponse
            {
                Post = new MetaPost
                {
                    PostId = "2",
                    Categories = new List<string> { "Technology" },
                    Tags = new List<string> { "C#", "Asp.net" },
                    Title = "Test Post",
                    Slug = "test-post",
                    Description = "Body of post.",
                    Publish = true,
                    PostDate = new DateTime(2017, 9, 8, 13, 34, 05)
                },
            };

            // Act
            var output = _helper.BuildOutput("metaWeblog.getPost", resp);

            // Assert
            Assert.Equal(RESP_GETPOST, output);
        }

        /// <summary>
        /// Test <see cref="XmlRpcHelper.BuildOutput(string, XmlRpcResponse)"/> for metaWeblog.getCategories.
        /// </summary>
        [Fact]
        public void BuildOutput_getCategories()
        {
            // Arrange
            var resp = new XmlRpcResponse();
            resp.Categories.Add(new MetaCategory
            {
                Description = "Test",
                Id = "2",
                Title = "Test",
                HtmlUrl = "https://localhost:44381/category/test",
                RssUrl = ""
            });
            resp.Categories.Add(new MetaCategory
            {
                Description = "Uncategorized",
                Id = "1",
                Title = "Uncategorized",
                HtmlUrl = "https://localhost:44381/category/uncategorized",
                RssUrl = ""
            });

            // Act
            var output = _helper.BuildOutput("metaWeblog.getCategories", resp);

            // Assert
            Assert.Equal(RESP_GETCATEGORIES, output);
        }

        /// <summary>
        /// Test <see cref="XmlRpcHelper.BuildOutput(string, XmlRpcResponse)"/> for wp.getTags.
        /// </summary>
        [Fact]
        public void BuildOutput_getTags()
        {
            // Arrange
            var resp = new XmlRpcResponse();
            resp.Keywords.Add("C#");
            resp.Keywords.Add("Asp.net");

            // Act
            var output = _helper.BuildOutput("wp.getTags", resp);

            // Assert
            Assert.Equal(RESP_GETTAGS, output);
        }

        /// <summary>
        /// Test <see cref="XmlRpcHelper.BuildOutput(string, XmlRpcResponse)"/> for metaWeblog.newMediaObject.
        /// </summary>
        [Fact]
        public void BuildOutput_newMediaObject()
        {
            // Arrange
            var resp = new XmlRpcResponse
            {
                MediaInfo = new MetaMediaInfo
                {
                    Url = "http://localhost/path/to/image.jpg"
                }
            };

            // Act
            var output = _helper.BuildOutput("metaWeblog.newMediaObject", resp);

            // Assert
            Assert.Equal(RESP_NEWMEDIA, output);
        }
    }
}
