using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fan.Blog.Models
{
    public interface ITaxonomy
    {
        /// <summary>
        /// Text of the term.
        /// </summary>
        [Required]
        [StringLength(maximumLength: 256)]
        string Title { get; set; }

        /// <summary>
        /// Friendly url of the term.
        /// </summary>
        [Required]
        [StringLength(maximumLength: 256)]
        string Slug { get; set; }

        /// <summary>
        /// The description of the category.
        /// </summary>
        /// <remarks>
        /// No html allowed, this field is HtmlEncoded.
        /// </remarks>
        string Description { get; set; }

        /// <summary>
        /// Published post count.
        /// </summary>
        [NotMapped]
        int Count { get; set; }
    }
}
