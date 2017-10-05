using Fan.Enums;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Fan.Models
{
    public class Post
    {
        public Post()
        {
            PostTags = new HashSet<PostTag>();
        }

        public int Id { get; set; }

        /// <summary>
        /// Post body.
        /// </summary>
        public string Body { get; set; }

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

        public ECommentStatus CommentStatus { get; set; }

        /// <summary>
        /// Post date. 
        /// </summary>
        /// <remarks>
        /// - This time is saved in Utc time.
        /// - When a published post is saved as draft, this maintains the post's CreatedOn.
        ///   When draft is published, unless user sets a new datetime, it maintains the original datetime.
        /// - When post is display to a user, we show the humanized version <see cref="CreatedOnDisplay"/>.
        /// </remarks>
        public DateTime CreatedOn { get; set; }

        /// <summary>
        /// User friendly time display, such as "yesterday".
        /// </summary>
        [NotMapped]
        public string CreatedOnDisplay { get; set; }

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
        /// Available only when Type is Media, null otherwise.
        /// </summary>
        [StringLength(maximumLength: 128)]
        public string MimeType { get; set; }

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
        /// <remarks>
        /// User is allowed to modify slug. If title is not given when saving post as draft 
        /// this will be the id of the post.
        /// </remarks>
        [Required]
        [StringLength(maximumLength: 256)]
        public string Slug { get; set; }

        /// <summary>
        /// Page order, null for blog post.
        /// </summary>
        public int? SortOrder { get; set; }

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
        /// For blog post and page, the datetime user last updated a draft, when post is published this value is null.
        /// </summary>
        public DateTime? UpdatedOn { get; set; }

        /// <summary>
        /// The username of the author.
        /// </summary>
        [Required]
        [StringLength(maximumLength: 256)]
        public string UserName { get; set; }

        public int ViewCount { get; set; }

        public virtual ICollection<PostTag> PostTags { get; set; }
    }
}
