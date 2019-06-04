using Fan.Widgets;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System.Threading.Tasks;

namespace Fan.Web.TagHelpers
{
    /// <summary>
    /// Renders a widget area, it requires a valid area id.
    /// </summary>
    /// <remarks>
    /// TODO there should be tag attributes that allow user to specify what html tag to surround the
    /// area, right now I'm hard coding a div. Also a css class to attach to the area.
    /// </remarks>
    [HtmlTargetElement("widget-area", Attributes = "id")]
    public class WidgetAreaTagHelper : AreaTagHelper
    {
        private readonly IWidgetService widgetService;

        public WidgetAreaTagHelper(IViewComponentHelper viewComponentHelper, IWidgetService widgetService) 
            : base(viewComponentHelper)
        {
            this.widgetService = widgetService;
        }

        /// <summary>
        /// The area id, bound to tag attribute.
        /// </summary>
        [HtmlAttributeName("id")]
        public string Id { get; set; }

        /// <summary>
        /// Outputs widget area html or nothing if the given widget area id is not found.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagMode = TagMode.StartTagAndEndTag;
            output.TagName = "div";
            output.Attributes.SetAttribute("class", "widgets");

            ((IViewContextAware)this.viewComponentHelper).Contextualize(ViewContext);

            var area = await widgetService.GetAreaAsync(Id);
            if (area == null) return;

            for (int i = 0; i < area.WidgetIds.Length; i++)
            {
                var widget = area.Widgets[i];

                var content = await viewComponentHelper.InvokeAsync(widget.Folder, widget);
                output.Content.AppendHtml(content.GetString());
            }
        }
    }
}
