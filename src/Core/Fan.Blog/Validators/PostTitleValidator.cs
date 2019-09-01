using Fan.Blog.Enums;
using Fan.Blog.Models;
using FluentValidation;

namespace Fan.Blog.Validators
{
    /// <summary>
    /// Validator for <see cref="BlogPost"/> and <see cref="Page"/>.
    /// </summary>
    public class PostTitleValidator : AbstractValidator<Post>
    {
        /// <summary>
        /// Max length for a post's title or slug is 250.
        /// </summary>
        public const int TITLE_MAXLEN = 250;

        /// <summary>
        /// Validates post title for 1. when post is not draft title is not allowed to be empty 
        /// 2. post title cannot exceed maxlen.
        /// </summary>
        public PostTitleValidator()
        {
            RuleFor(x => x.Title).NotEmpty().When(x => x.Status != EPostStatus.Draft).MaximumLength(TITLE_MAXLEN);
        }
    }
}