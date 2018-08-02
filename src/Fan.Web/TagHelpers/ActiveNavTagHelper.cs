using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Linq;

namespace Fan.Web.TagHelpers
{
    /// <summary>
    /// Helps Admin Side Nav to have current page selected, see Pages/Admin/_Layout.cshtml.
    /// </summary>
    /// <remarks>
    /// TODO make the attr set on the parent nav tag with a value of the selected css class being
    /// passed in.
    /// </remarks>
    [HtmlTargetElement(Attributes = "is-active-nav")]
    public class ActiveNavTagHelper : TagHelper
    {
        /// <summary>
        /// https://docs.microsoft.com/en-us/aspnet/core/mvc/views/tag-helpers/built-in/anchor-tag-helper?view=aspnetcore-2.1#asp-page
        /// </summary>
        private const string PageAttributeName = "asp-page";

        /// <summary>
        /// Gets or sets the <see cref="Rendering.ViewContext"/> for the current request.
        /// </summary>
        [HtmlAttributeNotBound]
        [ViewContext]
        public ViewContext ViewContext { get; set; }

        /// <summary>
        /// The name of the action method, e.g. asp-page="/admin/index"
        /// </summary>
        /// <remarks>
        /// Must be <c>null</c> if <see cref="Page"/> is non-<c>null</c>.
        /// </remarks>
        [HtmlAttributeName(PageAttributeName)]
        public string Page { get; set; } 


        public override void Process(TagHelperContext context, TagHelperOutput output)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            if (output == null)
            {
                throw new ArgumentNullException(nameof(output));
            }

            if (ShouldBeActive())
            {
                MakeActive(output, "mdc-list-item--selected");
            }
        }

        /// <summary>
        /// Returns true if <see cref="Page"/> matches current page, otherwise false.
        /// </summary>
        /// <returns></returns>
        private bool ShouldBeActive()
        {
            string currentPage = ViewContext.RouteData.Values["page"].ToString();

            // if Page matches currentPage, then it should be active
            if (!Page.IsNullOrWhiteSpace() && Page.Equals(currentPage, StringComparison.InvariantCultureIgnoreCase))
            {
                return true;
            }

            return false;
        }

        private void MakeActive(TagHelperOutput output, string cssClassName)
        {
            var classAttr = output.Attributes.FirstOrDefault(a => a.Name == "class");
            if (classAttr == null)
            {
                classAttr = new TagHelperAttribute("class", cssClassName);
                output.Attributes.Add(classAttr);
            }
            else if (classAttr.Value == null || classAttr.Value.ToString().IndexOf(cssClassName) < 0)
            {
                output.Attributes.SetAttribute("class", classAttr.Value == null
                    ? cssClassName
                    : classAttr.Value.ToString() + " " + cssClassName);
            }
        }
    }
}
