using Fan.Plugins;
using System;
using System.ComponentModel.DataAnnotations;

namespace ForkMeRibbon
{
    /// <summary>
    /// The fork me ribbon plugin.
    /// </summary>
    /// <remarks>
    /// <seealso cref="https://github.com/simonwhitaker/github-fork-ribbon-css"/>
    /// </remarks>
    public class ForkMeRibbonPlugin : Plugin
    {
        [Required]
        public string Text { get; set; } = "Fork me on GitHub";
        [Required]
        public string Url { get; set; } = "https://github.com/FanrayMedia/Fanray";
        public ERibbonPosition Position { get; set; } = ERibbonPosition.RightBottom;
        public bool Sticky { get; set; } = true;

        public string GetPositionString()
        {
            var str = Position.ToString().ToLower();
            var idx = str.StartsWith("left") ? 4 : 5;
            return str.Insert(idx, "-");
        }

        public override string GetFootContentViewName() => "Ribbon";
        public override string GetStylesViewName() => "RibbonStyles";

        public override string DetailsUrl => "https://github.com/FanrayMedia/Fanray/wiki/ForkMeRibbon-Plugin";
        public override string SettingsUrl =>
            (Folder.IsNullOrEmpty()) ? "" : $"/{PluginService.PLUGIN_DIR}/{Folder}Settings?pluginId={Id}";
    }
}
