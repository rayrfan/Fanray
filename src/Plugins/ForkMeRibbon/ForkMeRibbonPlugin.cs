using Fan.Plugins;

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
        public string Text { get; set; } = "Fork me on GitHub";
        public ERibbonPosition Postion { get; set; } = ERibbonPosition.RightBottom;

        public override string GetFootContentViewName() => "Ribbon";
        public override string GetStylesViewName() => "RibbonStyles";
    }
}
