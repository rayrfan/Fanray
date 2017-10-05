using Fan.Helpers;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fan.Models
{
    public class Category : Taxonomy
    {
        [NotMapped]
        public string RelativeLink => string.Format("/" + Const.CATEGORY_URL_TEMPLATE, Slug);
    }
}
