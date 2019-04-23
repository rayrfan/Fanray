namespace Fan.Data
{
    /// <summary>
    /// The different meta type.
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
        /// A widget instance.
        /// </summary>
        Widget = 4,
    }
}