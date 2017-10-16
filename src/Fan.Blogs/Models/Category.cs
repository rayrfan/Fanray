using Fan.Blogs.Helpers;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fan.Blogs.Models
{
    public class Category : Taxonomy
    {
        [NotMapped]
        public string RelativeLink => string.Format("/" + BlogConst.CATEGORY_URL_TEMPLATE, Slug);
    }
}
