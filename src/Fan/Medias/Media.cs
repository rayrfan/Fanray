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
    /// When user insert a media into a post by uploading, the values are hardcoded in the post body
    /// as in the case of an img. 
    /// 
    /// For image settings ref https://en.support.wordpress.com/images/image-settings/
    /// 
    /// When user uploads medias first, then insert them into the post from browser editor, that is
    /// when properties like Description and Title will be used.
    /// </remarks>
    public class Media : Entity
    {
        /// <summary>
        /// The the app that uploaded the media file.
        /// </summary>
        public EAppType AppId { get; set; }

        /// <summary>
        /// Description of the media.
        /// </summary>
        /// <remarks>
        /// For image its the alt text.
        /// </remarks>
        public string Description { get; set; }

        /// <summary>
        /// Name of the file with ext. 
        /// </summary>
        /// <remarks>
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
        /// Title of the media.
        /// </summary>
        /// <remarks>
        /// The original filename; html title attribute shows as tooltip.
        /// TODO should I have the string length limit on title?
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

