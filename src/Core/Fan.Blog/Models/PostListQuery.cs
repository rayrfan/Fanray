using Fan.Blog.Enums;

namespace Fan.Blog.Models
{
    /// <summary>
    /// Query for a list of posts.
    /// </summary>
    public class PostListQuery
    {
        public PostListQuery(EPostListQueryType queryType)
        {
            QueryType = queryType;
        }

        public EPostListQueryType QueryType { get; set; }
        public int PageIndex { get; set; }
        public int PageSize { get; set; } 
        public string CategorySlug { get; set; }
        public string TagSlug { get; set; }
        public int Year { get; set; }
        public int? Month { get; set; }
    }
}