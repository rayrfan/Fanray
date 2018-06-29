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
        /// Name of the file with ext. 
        /// </summary>
        /// <remarks>
        /// For iamage, this is the title attribute, when you hover over the image.
        /// If filename exceeds <see cref="MediaService.MEDIA_FILENAME_MAXLEN"/>, it will be shortened.
        /// For blog the name part is slug formatted, for example "test pic.jpg" becomes "test-pic.jpg".
        /// For other apps, it could be a guid value [guid].jpg.
        /// </remarks>
        [Required]
        [StringLength(maximumLength: 256)]
        public string FileName { get; set; }

        /// <summary>
        /// Size of the file in bytes.
        /// </summary>
        public long Length { get; set; }

        /// <summary>
        /// Title of the media or alt text for image.
        /// </summary>
        /// <remarks>
        /// For image this is its alt text, since only image has alt attribute I don't dedicate a column for it.
        /// The <see cref="FileName"/> property will be used as title attribute instead.
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
        public string FileType { get; set; }

        /// <summary>
        /// Width of an image in px.
        /// </summary>
        public int Width { get; set; }

        /// <summary>
        /// Height of an image in px.
        /// </summary>
        public int Height { get; set; }

        /// <summary>
        /// True if the file is an image and the image size is over <see cref="MediaService.IMAGE_OPTIMIZED_SIZE"/>
        /// then the <see cref="MediaService"/> will resize it.  False otherwise.
        /// </summary>
        public bool Optimized { get; set; }
    }
}

