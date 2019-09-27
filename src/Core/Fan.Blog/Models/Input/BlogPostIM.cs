using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Fan.Blog.Models.Input
{
    /// <summary>
    /// Input model for blog post.
    /// </summary>
    public class BlogPostIM
    {
        public int Id { get; set; }
        [Required]
        public string Title { get; set; }
        public string Body { get; set; }
        public string PostDate { get; set; }
        public string Slug { get; set; }
        public string Excerpt { get; set; }
        public int CategoryId { get; set; }
        public List<string> Tags { get; set; } // titles, not slugs
        public bool Published { get; set; }
        public bool IsDraft { get; set; }
        public string DraftDate { get; set; }
    }
}
