using System.ComponentModel.DataAnnotations;

namespace Fan.Models
{
    public class Meta
    {
        public int Id { get; set; }
        [Required]
        [StringLength(maximumLength: 256)]
        public string Key { get; set; }
        public string Value { get; set; }
    }
}
