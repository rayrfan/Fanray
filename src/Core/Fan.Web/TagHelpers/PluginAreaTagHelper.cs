using Fan.Plugins;
using Microsoft.AspNetCore.Html;
using Microsoft.AspNetCore.Mvc;
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
    [HtmlTargetElement("plugin-area", Attributes = nameof(Id))]
    public class PluginAreaTagHelper : AreaTagHelper
    {
        private readonly IPluginService pluginService;

        public PluginAreaTagHelper(IViewComponentHelper viewComponentHelper, IPluginService pluginService) 
            : base(viewComponentHelper)
        {
            this.pluginService = pluginService;
        }

        [HtmlAttributeName("id")]
        public EPluginAreaId Id { get; set; }

        /// <summary>
        /// Outputs plugin visual elements.
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
                if (Id == EPluginAreaId.Styles && !plugin.GetStylesViewName().IsNullOrEmpty())
                {
                    var content = await viewComponentHelper.InvokeAsync(plugin.GetStylesViewName(), plugin);
                    output.Content.AppendHtml(content.GetString());
                }
                if (Id == EPluginAreaId.FootContent && !plugin.GetFootContentViewName().IsNullOrEmpty())
                {
                    var content = await viewComponentHelper.InvokeAsync(plugin.GetFootContentViewName(), plugin);
                    output.Content.AppendHtml(content.GetString());
                }
                if (Id == EPluginAreaId.FootScripts && !plugin.GetFootScriptsViewName().IsNullOrEmpty())
                {
                    var content = await viewComponentHelper.InvokeAsync(plugin.GetFootScriptsViewName(), plugin);
                    output.Content.AppendHtml(content.GetString());
                }
            }
        }
    }

    /// <summary>
    /// Locations on the layout to place plugin visual elements.
    /// </summary>
    public enum EPluginAreaId
    {
        /// <summary>
        /// Styles placed before closing head tag.
        /// </summary>
        Styles,
        /// <summary>
        /// Html place after footer.
        /// </summary>
        FootContent,
        /// <summary>
        /// Js place before closing body tag.
        /// </summary>
        FootScripts,
    }
}
