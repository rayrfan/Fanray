using Fan.Plugins;

namespace ForkMeRibbon
{
    public class ForkMeRibbonPlugin : Plugin
    {
        public string Text { get; set; } = "Fork me on GitHub";
        public ERibbonPosition Postion { get; set; } = ERibbonPosition.RightBottom;

        public override string GetFooterViewName() => "Ribbon";
    }
}
