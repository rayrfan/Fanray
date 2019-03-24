using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fan.Themes
{
    public interface IThemeService
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
        /// <summary>
        /// Returns a list of <see cref="ThemeInfo"/> of the installed themes.
        /// </summary>
        /// <returns></returns>
        Task<List<ThemeInfo>> GetInstalledThemesInfoAsync();     
    }
}
