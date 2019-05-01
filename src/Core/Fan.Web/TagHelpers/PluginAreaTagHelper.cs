using Fan.Plugins;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Microsoft.AspNetCore.Razor.TagHelpers;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fan.Web.TagHelpers
{
    /// <summary>
    /// Renders a plugin area to output plugin visual elements.
    /// </summary>
    [HtmlTargetElement("plugin-area", Attributes = nameof(Type))]
    public class PluginAreaTagHelper : TagHelper
    {
        private readonly IViewComponentHelper viewComponentHelper;
        private readonly IPluginService pluginService;

        public PluginAreaTagHelper(IViewComponentHelper viewComponentHelper,
            IPluginService pluginService)
        {
            this.viewComponentHelper = viewComponentHelper;
            this.pluginService = pluginService;
        }

        public EPluginAreaType Type { get; set; }

        /// <summary>
        /// Initializes the ViewContext of the executing page.
        /// </summary>
        [ViewContext]
        [HtmlAttributeNotBound]
        public ViewContext ViewContext { get; set; }

        /// <summary>
        /// Outputs plugin area html.
        /// </summary>
        /// <param name="context"></param>
        /// <param name="output"></param>
        /// <returns></returns>
        public override async Task ProcessAsync(TagHelperContext context, TagHelperOutput output)
        {
            output.TagName = null;

            ((IViewContextAware)viewComponentHelper).Contextualize(ViewContext);

            var plugins = await pluginService.GetActivePluginsAsync();
            if (plugins.IsNullOrEmpty()) return;

            foreach (var plugin in plugins)
            {
                if (!plugin.GetFooterViewName().IsNullOrEmpty())
                {
                    var content = await viewComponentHelper.InvokeAsync(plugin.GetFooterViewName(), plugin);
                    output.Content.AppendHtml(content.GetString());
                }
            }
        }
    }

    /// <summary>
    /// Locations on the layout to place plugin visual elements.
    /// </summary>
    public enum EPluginAreaType
    {
        Header,
        Footer
    }
}
