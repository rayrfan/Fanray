using Fan.Blogs.Enums;
using Fan.Models;
using FluentValidation;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fan.Blogs.Validators
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

        public TaxonomyValidator(IEnumerable<string> existingTitles, ETaxonomyType type)
        {
            RuleFor(c => c.Title)
                .NotEmpty()
                .Length(1, TAXONOMY_TITLE_SLUG_MAXLEN)
                .Must(title => !existingTitles.Contains(title, StringComparer.CurrentCultureIgnoreCase)) 
                .WithMessage(c => $"{type} '{c.Title}' is not available, please choose a different one.");
        }
    }
}
