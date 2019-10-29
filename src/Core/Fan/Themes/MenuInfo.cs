namespace Fan.Themes
{
    /// <summary>
    /// Represents an item on the "menus" section of the "theme.json" file.
    /// </summary>
    public class MenuInfo
    {
        public EMenu Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
    }
}
