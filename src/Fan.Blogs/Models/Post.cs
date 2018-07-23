using Fan.Blogs.Enums;
using Fan.Data;
using Fan.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fan.Blogs.Models
{
    public class Post : Entity
    {
        public Post()
        {
            PostTags = new HashSet<PostTag>();
        }

        /// <summary>
        /// Post body in html.
        /// </summary>
        public string Body { get; set; }

        /// <summary>
        /// Post body in markdown.
        /// </summary>
        public string BodyMark { get; set; }

        /// <summary>
        /// Category for a blog post, null for page.
        /// </summary>
        public Category Category { get; set; }

        /// <summary>
        /// Category id for blog post, null for a page.
        /// </summary>
        /// <remarks>
        /// Nullable FK to Category. NOTE: Sql Server will not create cascade delete for a nullable FK.
        /// Therefore deleting a category by user won't delete its associated posts, 
        /// <see cref="Fan.Data.SqlCategoryRepository.DeleteAsync(int, int)"/> for more details.
        /// </remarks>
        public int? CategoryId { get; set; }

        public int CommentCount { get; set; }

        /// <summary>
        /// Gets and sets if the post allows, needs approval of or no comments.
        /// </summary>
        public ECommentStatus CommentStatus { get; set; }

        /// <summary>
        /// Post date. 
        /// </summary>
        /// <remarks>
        /// - This time is saved in Utc time.
        /// - When a published post is saved as draft, this maintains the post's CreatedOn.
        ///   When draft is published, unless user sets a new datetime, it maintains the original value.
        /// - When post is display to a user, we show the humanized version <see cref="CreatedOnDisplay"/>
        ///   or a date time string converted to the <see cref="CoreSettings.TimeZoneId"/>.
        /// </remarks>
        public DateTimeOffset CreatedOn { get; set; }

        /// <summary>
        /// CreatedOn used for time display, such as "yesterday".
        /// </summary>
        [NotMapped]
        public string CreatedOnDisplay { get; set; }

        /// <summary>
        /// UpdatedOn used for display.
        /// </summary>
        /// <remarks>
        /// This is used in the admin console to show the last updated date in user's local timezone.
        /// </remarks>
        [NotMapped]
        public string UpdatedOnDisplay { get; set; }

        /// <summary>
        /// The post excerpt.
        /// </summary>
        /// <remarks>
        /// This is intended to be used in search results, html meta description or when blog is set 
        /// to show excerpt.
        /// 
        /// This is manually inputted by user, if user didn't put an excerpt this field will be null.  
        /// When this field is not available and blog setting ShowExcerpt is set to true, the excerpt 
        /// will be calculated on the fly.
        /// </remarks>
        public string Excerpt { get; set; }

        /// <summary>
        /// Parent page id for child page, 0 for root page, null for blog post.
        /// </summary>
        public int? ParentId { get; set; }

        /// <summary>
        /// Root page id for child page, 0 for root page, null for blog post.
        /// </summary>
        public int? RootId { get; set; }

        /// <summary>
        /// The post slug.
        /// </summary>
        [StringLength(maximumLength: 256)]
        public string Slug { get; set; }

        /// <summary>
        /// Published or Draft.
        /// </summary>
        public EPostStatus Status { get; set; }

        /// <summary>
        /// Post title.
        /// </summary>
        /// <remarks>
        /// I decided not to make it required in db, though I implemented in BLL making it required.
        /// </remarks>
        [StringLength(maximumLength: 256)]
        public string Title { get; set; }

        /// <summary>
        /// Blog post or Page.
        /// </summary>
        public EPostType Type { get; set; }

        /// <summary>
        /// When user last updated a draft, when the post is published this value is null.
        /// </summary>
        public DateTimeOffset? UpdatedOn { get; set; }

        /// <summary>
        /// The author.
        /// </summary>
        public User User { get; set; }

        /// <summary>
        /// The author id.
        /// </summary>
        public int UserId { get; set; }

        public int ViewCount { get; set; }

        public virtual ICollection<PostTag> PostTags { get; set; }
    }
}
