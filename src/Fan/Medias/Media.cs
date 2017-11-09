using Fan.Data;
using Fan.Models;
using System;
using System.ComponentModel.DataAnnotations;

namespace Fan.Medias
{
    /// <summary>
    /// Represents a file user uploads.
    /// </summary>
    /// <remarks>
    /// Once user uploads and inserts the media into post, the values are hardcoded in the post body
    /// as in the case of an img.
    /// Image settings ref https://en.support.wordpress.com/images/image-settings/
    /// </remarks>
    public class Media : Entity
    {
        /// <summary>
        /// The the app that uploaded the media file.
        /// </summary>
        public EAppType AppId { get; set; }

        /// <summary>
        /// Description of the media, alt text for image.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// Name of the file with ext, the name part is slug formatted, for example
        /// "test pic.jpg" becomes "test-pic.jpg".
        /// </summary>
        [Required]
        [StringLength(maximumLength: 256)]
        public string FileName { get; set; }

        /// <summary>
        /// Size of the file in bytes.
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

        /// <summary>
        /// Image, documents, audio, video.
        /// </summary>
        /// <remarks>
        /// https://en.support.wordpress.com/accepted-filetypes/
        /// </remarks>
        public EMediaType MediaType { get; set; }

        /// <summary>
        /// Upload date, used for yyyy/mm folder.
        /// </summary>
        public DateTimeOffset UploadedOn { get; set; }

        /// <summary>
        /// What type of client user uploaded the file from, metaweblog, browser, mobile etc.
        /// </summary>
        public EUploadedFrom UploadedFrom { get; set; }

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

