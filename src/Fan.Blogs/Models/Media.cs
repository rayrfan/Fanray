using Fan.Blogs.Enums;
using Fan.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace Fan.Blogs.Models
{
    /// <summary>
    /// Media files user can upload.
    /// </summary>
    /// <remarks>
    /// Once user uploads and inserts the media into post, the values are hardcoded in the post body
    /// as in the case of an img.
    /// https://en.support.wordpress.com/images/image-settings/
    /// </remarks>
    public class Media : Entity
    {
        /// <summary>
        /// Description of the media, alt text for image.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Name of the file, the name part is slug formatted, for example
        /// "test pic.jpg" becomes "test-pic.jpg".
        /// </summary>
        [Required]
        [StringLength(maximumLength: 256)]
        public string FileName { get; set; }

        /// <summary>
        /// Size of the file
        /// </summary>
        public long Length { get; set; }

        /// <summary>
        /// Title of the media.
        /// </summary>
        /// <remarks>
        /// The original filename; html title attribute shows as tooltip.
        /// </remarks>
        [StringLength(maximumLength: 256)]
        public string Title { get; set; }

        public EMediaType Type { get; set; }

        /// <summary>
        /// Upload date, used for yyyy/mm folder.
        /// </summary>
        public DateTimeOffset UploadedOn { get; set; }

        /// <summary>
        /// The user who uploaded this media.
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// The id of the user who uploaded this media.
        /// </summary>
        public int UserId { get; set; }
    }
}

