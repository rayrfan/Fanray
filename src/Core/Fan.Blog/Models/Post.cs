using Fan.Blog.Enums;
using Fan.Blog.Validators;
using Fan.Data;
using Fan.Exceptions;
using Fan.Membership;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;

namespace Fan.Blog.Models
{
    public class Post : Entity
    {
        public Post()
        {
            PostTags = new HashSet<PostTag>();
        }

        /// <summary>
        /// Validates a post object and throws <see cref="FanException"/> if validation fails.
        /// </summary>
        /// <returns></returns>
        public async Task ValidateTitleAsync()
        {
            var validator = new PostTitleValidator();
            var result = await validator.ValidateAsync(this);
            if (!result.IsValid)
            {
                throw new FanException($"{Type} title is not valid.", result.Errors);
            }
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
        /// Parent page id, null for parent page.
        /// </summary>
        public int? ParentId { get; set; }

        /// <summary>
        /// Root page id.
        /// </summary>
        /// <remarks>
        /// Not currently used.
        /// </remarks>
        public int? RootId { get; set; }

        /// <summary>
        /// The post slug.
        /// </summary>
        /// <remarks>
        /// Slug is searched upon and thus has an index on it. Slug allows null and is not unique, 
        /// the service layer ensures its uniqueness under their specific conditions.
        /// </remarks>
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
        /// TODO consider removing the maxlen requirement since there is no index on this field.
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
        /// <remarks>
        /// TODO: This actually should be called DraftSavedOn
        /// </remarks>
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

        /// <summary>
        /// The featured image id.
        /// </summary>
        //public int? FeaturedImageId { get; set; }

        /// <summary>
        /// A parent page's Table of Content.
        /// </summary>
        /// <remarks>
        /// Saved in markdown and converted to HTML when displayed to public.
        /// </remarks>
        public string Nav { get; set; }

        public byte? PageLayout { get; set; }
    }
}
