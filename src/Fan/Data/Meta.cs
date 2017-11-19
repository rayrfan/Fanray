using System.ComponentModel.DataAnnotations;

namespace Fan.Data
{
    /// <summary>
    /// A key value pair record.
    /// </summary>
    /// <remarks>
    /// It's used to store settings or any other kind of data.  For example a plugin could save 
    /// data in a json serialized string here.
    /// </remarks>
    public class Meta : Entity
    {
        /// <summary>
        /// The key upon which to get the value, it must be unique.
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