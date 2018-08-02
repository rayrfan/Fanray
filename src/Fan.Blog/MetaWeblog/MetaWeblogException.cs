using System;

namespace Fan.Blog.MetaWeblog
{
    public enum EMetaWeblogCode
    {
        UnknownCause = 1000,
        InvalidRequest,
        UnknownMethod,
        AuthenticationFailed,
        GetUsersBlogs,
        GetPost,
        GetRecentPosts,
        NewPost,
        EditPost,
        DeletePost,
        GetCategories,
        CreateCategory,
        GetKeywords,
        NewMediaObject,
    }

    public class MetaWeblogException : Exception
    {
        public MetaWeblogException(EMetaWeblogCode code, string message)
            : base(message)
        {
            Code = code;
        }

        public EMetaWeblogCode Code { get; private set; }
    }
}
