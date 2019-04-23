using System.ComponentModel.DataAnnotations;

namespace Fan.Data
{
    /// <summary>
    /// A key value pair record.
    /// </summary>
    /// <remarks>
    /// The Meta table stores settings and other json strings. It has a unique constrain on Type and Key pair.
    /// </remarks>
    public class Meta : Entity
    {
        /// <summary>
        /// The key is case sensitive, "key" and "Key" are considered two different values.
        /// </summary>
        /// <remarks>
        /// For all my records from settings, themes to widgets I use lower case for keys to overcome 
        /// case being sensitive.
        /// </remarks>
        [Required]
        [StringLength(maximumLength: 256)]
        public string Key { get; set; }

        /// <summary>
        /// Can be anything though mostly a deserialized object in which case the caller should 
        /// know how to deserialize it back an object.
        /// </summary>
        public string Value { get; set; }

        /// <summary>
        /// The type of meta.
        /// </summary>
        public EMetaType Type { get; set; }
    }
}