using Fan.Blog.Helpers;
using Fan.Data;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fan.Blog.Models
{
    public class Category : Entity, ITaxonomy
    {
        /// <summary>
        /// Text of the term.
        /// </summary>
        [Required]
        [StringLength(maximumLength: 256)]
        public string Title { get; set; }

        /// <summary>
        /// Friendly url of the term.
        /// </summary>
        [Required]
        [StringLength(maximumLength: 256)]
        public string Slug { get; set; }

        /// <summary>
        /// The description of the category.
        /// </summary>
        /// <remarks>
        /// No html allowed, this field is HtmlEncoded.
        /// </remarks>
        public string Description { get; set; }

        /// <summary>
        /// Published post count.
        /// </summary>
        [NotMapped]
        public int Count { get; set; }

        [NotMapped]
        public string RelativeLink => BlogRoutes.GetCategoryRelativeLink(Slug);

        [NotMapped]
        public string RssRelativeLink => BlogRoutes.GetCategoryRssRelativeLink(Slug);
    }
}
