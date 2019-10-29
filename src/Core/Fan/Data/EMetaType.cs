namespace Fan.Data
{
    /// <summary>
    /// The different meta types.
    /// </summary>
    public enum EMetaType
    {
        /// <summary>
        /// A setting.
        /// </summary>
        Setting = 0,
        /// <summary>
        /// A theme.
        /// </summary>
        Theme = 1,
        /// <summary>
        /// A system defined widget area.
        /// </summary>
        WidgetAreaBySystem = 2,
        /// <summary>
        /// A theme defined widget area.
        /// </summary>
        WidgetAreaByTheme = 3,
        /// <summary>
        /// Widget settings and default values.
        /// </summary>
        WidgetSettings = 4,
        /// <summary>
        /// A widget.
        /// </summary>
        Widget = 5,
        /// <summary>
        /// A plugin.
        /// </summary>
        Plugin = 6,
        /// <summary>
        /// A navigation menu.
        /// </summary>
        Menu = 7,
    }
}