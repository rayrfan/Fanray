using Fan.Blog.MetaWeblog.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace Fan.Blog.MetaWeblog
{
    /// <summary>
    /// The middleware for MetaWeblog.
    /// </summary>
    public class MetaWeblogMiddleware
    {
        private ILoggerFactory _loggerFactory;
        private ILogger<MetaWeblogMiddleware> _logger;

        public MetaWeblogMiddleware(RequestDelegate next, ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
            _logger = loggerFactory.CreateLogger<MetaWeblogMiddleware>();
        }

        /// <summary>
        /// Processes XmlRpc request and output back response.
        /// </summary>
        /// <remarks>
        /// Inject any non-singleton dependencies here instead of at the constructor since core 2.
        /// </remarks>
        public async Task Invoke(HttpContext context, IMetaWeblogService service, IXmlRpcHelper helper) 
        {
            try
            {
                // get xml from request
                var sr = new StreamReader(context.Request.Body);
                var xml = await sr.ReadToEndAsync();

                // get req and resp ready
                var request = helper.ParseRequest(xml);
                var response = new XmlRpcResponse();
                var rootUrl = $"{context.Request.Scheme}://{context.Request.Host}"; 

                _logger.LogDebug("{@RpcMethod} {@RpcReqXml}", request.MethodName, xml);
                switch (request.MethodName)
                {
                    case "blogger.getUsersBlogs":
                    case "metaWeblog.getUsersBlogs":
                        response.BlogInfos = await service.GetUsersBlogsAsync(request.AppKey, request.UserName, request.Password, rootUrl);
                        break;

                    case "metaWeblog.newPost":
                        response.PostId = await service.NewPostAsync(request.BlogId, request.UserName, request.Password, request.Post, request.Publish);
                        break;

                    case "metaWeblog.editPost":
                        response.Completed = await service.EditPostAsync(request.PostId, request.UserName, request.Password, request.Post, request.Publish);
                        break;

                    case "blogger.deletePost":
                        response.Completed = await service.DeletePostAsync(request.AppKey, request.PostId, request.UserName, request.Password);
                        break;

                    case "metaWeblog.getPost":
                        response.Post = await service.GetPostAsync(request.PostId, request.UserName, request.Password, rootUrl);
                        break;

                    case "metaWeblog.getRecentPosts":
                        response.Posts = await service.GetRecentPostsAsync(request.BlogId, request.UserName, request.Password, request.NumberOfPosts, rootUrl);
                        break;

                    case "metaWeblog.newMediaObject":
                        response.MediaInfo = await service.NewMediaObjectAsync(request.BlogId, request.UserName, request.Password, request.MediaObject, context);
                        break;

                    case "metaWeblog.getCategories":
                        response.Categories = await service.GetCategoriesAsync(request.BlogId, request.UserName, request.Password, rootUrl);
                        break;

                    case "wp.newCategory":
                        response.CategoryId = await service.CreateCategoryAsync(request.CategoryTitle, request.UserName, request.Password);
                        break;

                    case "wp.getTags":
                        response.Keywords = await service.GetKeywordsAsync(request.BlogId, request.UserName, request.Password);
                        break;

                    default:
                        throw new MetaWeblogException(EMetaWeblogCode.UnknownMethod, $"Unknown Method. ({request.MethodName})");
                }

                string output = helper.BuildOutput(request.MethodName, response);
                _logger.LogDebug("{@RpcMethod} {@RpcRespXml}", request.MethodName, output);

                context.Response.ContentType = "text/xml";
                await context.Response.WriteAsync(output, Encoding.UTF8);
            }
            catch (Exception ex)
            {
                if (ex is MetaWeblogException metaEx)
                {
                    _logger.LogError("MetaWeblog <{EventId}> " + metaEx.Message, metaEx.Code);
                    string output = helper.BuildFaultOutput(metaEx);
                    await context.Response.WriteAsync(output, Encoding.UTF8);
                }
                else {
                    _logger.LogError("MetaWeblog <{EventId}> " + ex.Message, EMetaWeblogCode.UnknownCause);
                    string output = helper.BuildFaultOutput((int)EMetaWeblogCode.UnknownCause, ex.Message);
                    await context.Response.WriteAsync(output, Encoding.UTF8);
                }
            }
        }
    }
}
