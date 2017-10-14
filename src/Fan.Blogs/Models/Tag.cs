using Fan.Blogs.Helpers;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fan.Blogs.Models
{
    public class Tag : Taxonomy
    {
        public Tag()
        {
            Color = "#666";
            PostTags = new HashSet<PostTag>();
        }

        [StringLength(32)]
        public string Color { get; set; }

        [NotMapped]
        public string RelativeLink => string.Format("/" + BlogConst.TAG_URL_TEMPLATE, Slug);

        public virtual ICollection<PostTag> PostTags { get; set; }
    }
}
