namespace Fan.Blog.Enums
{
    /// <summary>
    /// Comment setting for an individual post, default is to allow comments.
    /// </summary>
    /// <remarks>
    /// This can be set both from browser or olw.
    /// </remarks>
    public enum ECommentStatus : byte
    {
        NoComments = 0,
        AllowComments,
        AllowCommentsWithApproval,
    }
}
