using FluentValidation;

namespace Fan.Blog.Models
{
    /// <summary>
    /// Validator for both <see cref="BlogPost"/> and <see cref="Page"/>.
    /// </summary>
    /// <remarks>
    /// The slug is derived from title and is made sure to be valid.
    /// </remarks>
    public class PostValidator : AbstractValidator<Post>
    {
        /// <summary>
        /// Max length for a post's title or slug is 256.
        /// </summary>
        /// <remarks>
        /// I'm treating a post's title and slug with same length requirement.
        /// </remarks>
        public const int POST_TITLE_SLUG_MAXLEN = 256;

        public PostValidator()
        {
            RuleFor(x => x.Title).NotEmpty().Length(1, POST_TITLE_SLUG_MAXLEN);
        }
    }
}