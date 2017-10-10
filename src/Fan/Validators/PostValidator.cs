using Fan.Helpers;
using Fan.Models;
using FluentValidation;

namespace Fan.Validators
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
            RuleFor(x => x.Title).NotEmpty().Length(1, Const.POST_TITLE_SLUG_MAXLEN);
        }
    }
}