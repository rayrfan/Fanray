using Fan.Blog.Models;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fan.Blog.Validators
{
    /// <summary>
    /// Validator for <see cref="Category"/> and <see cref="Tag"/>.
    /// </summary>
    /// <remarks>
    /// Both Category and Tag need validation on its title, the slug is derived from title and is made sure
    /// to be valid.
    /// </remarks>
    public class TaxonomyValidator : AbstractValidator<ITaxonomy>
    {
        /// <summary>
        /// Max length for a category and tag's title or slug is 24.
        /// </summary>
        /// <remarks>
        /// I'm treating a taxonomy's title and slug with same length requirement.
        /// </remarks>
        public const int TAXONOMY_TITLE_SLUG_MAXLEN = 24;

        /// <summary>
        /// Validates title to be required, length and not among the existing titles.
        /// </summary>
        /// <param name="existingTitles"></param>
        public TaxonomyValidator(IEnumerable<string> existingTitles)
        {
            RuleFor(c => c.Title)
                .NotEmpty()
                .Length(1, TAXONOMY_TITLE_SLUG_MAXLEN)
                .Must(title => !existingTitles.Contains(title, StringComparer.CurrentCultureIgnoreCase)) 
                .WithMessage(c => $"'{c.Title}' already exists.");
        }
    }
}
