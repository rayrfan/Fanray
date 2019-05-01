using Fan.Extensibility;
using System.Threading.Tasks;

namespace Fan.Themes
{
    /// <summary>
    /// The theme service interface.
    /// </summary>
    public interface IThemeService : IExtensibleService<ThemeInfo, Theme>
    {
        /// <summary>
        /// Activates a theme.
        /// </summary>
        /// <param name="folderName">Theme's folder name.</param>
        /// <returns></returns>
        /// <remarks>
        /// It registers theme and the widget areas used by the theme.
        /// </remarks>
        Task ActivateThemeAsync(string folderName);
    }
}
