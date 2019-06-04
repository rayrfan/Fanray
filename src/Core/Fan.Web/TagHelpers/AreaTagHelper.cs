using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;

namespace Fan.Web.TagHelpers
{
    /// <summary>
    /// Base tag helper for plugins and widgets.
    /// </summary>
    public class AreaTagHelper : TagHelper
    {
        /// <summary>
        /// Helper used to invoke ViewComponent.
        /// </summary>
        /// <remarks>
        /// This injected helper is "neutral", not specific for our view, so we have to "contextualize" it 
        /// for the current view ViewContext before using.
        /// https://docs.microsoft.com/en-us/aspnet/core/mvc/views/view-components?view=aspnetcore-2.2#perform-synchronous-work
        /// https://github.com/aspnet/Mvc/issues/5504#issuecomment-258671545
        /// </remarks>
        protected readonly IViewComponentHelper viewComponentHelper;

        public AreaTagHelper(IViewComponentHelper viewComponentHelper)
        {
            this.viewComponentHelper = viewComponentHelper;
        }

        /// <summary>
        /// Initializes the ViewContext of the executing page.
        /// </summary>
        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }
    }
}
