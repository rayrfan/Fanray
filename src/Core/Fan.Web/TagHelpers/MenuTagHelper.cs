using Fan.Navigation;
using Fan.Themes;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Threading.Tasks;

namespace Fan.Web.TagHelpers
{
    [HtmlTargetElement("menu", Attributes = "id")]
    public class MenuTagHelper : AreaTagHelper
    {
        private readonly INavigationService navigationService;

        public MenuTagHelper(IViewComponentHelper viewComponentHelper, 
            INavigationService navigationService)
            : base(viewComponentHelper)
        {
            this.navigationService = navigationService;
        }

        /// <summary>
        /// The menu id, <see cref="ENavMenu"/>.
        /// </summary>
        [HtmlAttributeName("id")]
        public EMenu Id { get; set; }

        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = null;

            ((IViewContextAware)this.viewComponentHelper).Contextualize(ViewContext);

            var navList = await navigationService.GetMenuAsync(Id, includeNavUrl: true);
            var content = await viewComponentHelper.InvokeAsync("Menu", (Id, navList));
            output.Content.AppendHtml(content.GetString());
        }
    }
}
