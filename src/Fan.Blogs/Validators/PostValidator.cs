using Fan.Blogs.Helpers;
using Fan.Blogs.Models;
using FluentValidation;

namespace Fan.Blogs.Validators
{
    /// <summary>
    /// Validator for both <see cref="BlogPost"/> and <see cref="Page"/>.
    /// </summary>
    /// <remarks>
    /// The slug is derived from title and is made sure to be valid.
    /// </remarks>
    public class PostValidator : AbstractValidator<Post>
    {
        public PostValidator()
        {
            RuleFor(x => x.Title).NotEmpty().Length(1, BlogConst.POST_TITLE_SLUG_MAXLEN);
        }
    }
}