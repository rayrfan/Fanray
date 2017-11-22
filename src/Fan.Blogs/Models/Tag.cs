using Fan.Blogs.Helpers;
using Fan.Data;
using Fan.Models;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fan.Blogs.Models
{
    public class Tag : Entity, ITaxonomy
    {
        public Tag()
        {
            Color = "#666";
            PostTags = new HashSet<PostTag>();
        }

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

        [StringLength(32)]
        public string Color { get; set; }

        [NotMapped]
        public string RelativeLink => string.Format("/" + BlogRoutes.TAG_URL_TEMPLATE, Slug);

        public virtual ICollection<PostTag> PostTags { get; set; }
    }
}
