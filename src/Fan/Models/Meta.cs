using System.ComponentModel.DataAnnotations;

namespace Fan.Models
{
    public class Meta : BaseEntity
    {
        /// <summary>
        /// Must be unique.
        /// </summary>
        [Required]
        [StringLength(maximumLength: 256)]
        public string Key { get; set; }

        /// <summary>
        /// Can be anything though mostly a deserialized object in which case the caller should 
        /// know how to deserialize it back an object.
        /// </summary>
        public string Value { get; set; }
    }
}
