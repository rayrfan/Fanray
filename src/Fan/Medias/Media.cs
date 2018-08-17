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
    /// https://en.support.wordpress.com/images/image-settings/
    /// </remarks>
    public class Media : Entity
    {
        /// <summary>
        /// The app that uploaded the media file.
        /// </summary>
        public EAppType AppType { get; set; }

        /// <summary>
        /// Description of the media.
        /// </summary>
        public string Description { get; set; }

        /// <summary>
        /// File name.
        /// </summary>
        /// <remarks>
        /// This is the unique, slugged with ext filename. 
        /// </remarks>
        [Required]
        [StringLength(maximumLength: 256)]
        public string FileName { get; set; }

        /// <summary>
        /// Size of the file in bytes.
        /// </summary>
        public long Length { get; set; }

        /// <summary>
        /// Title attribute.
        /// </summary>
        /// <remarks>
        /// https://stackoverflow.com/q/872389/32240
        /// </remarks>
        [StringLength(maximumLength: 256)]
        public string Title { get; set; }

        /// <summary>
        /// Image, documents etc.
        /// </summary>
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

        /// <summary>
        /// Caption of the media
        /// </summary>
        public string Caption { get; set; }

        /// <summary>
        /// File type of the media, e.g. jpg, png.
        /// </summary>
        /// <remarks>
        /// https://en.support.wordpress.com/accepted-filetypes/
        /// </remarks>
        [Required]
        [StringLength(maximumLength: 256)]
        public string ContentType { get; set; }

        /// <summary>
        /// Width of an image in px.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Height of an image in px.
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// Image alt attribute.
        /// </summary>
        public string Alt { get; set; }

        /// <summary>
        /// Number of resizes for an image.
        /// </summary>
        public int ResizeCount { get; set; }
    }
}

