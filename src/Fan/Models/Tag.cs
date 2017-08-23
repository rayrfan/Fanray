using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Fan.Models
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

        public virtual ICollection<PostTag> PostTags { get; set; }
    }
}
