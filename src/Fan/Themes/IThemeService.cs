using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fan.Themes
{
    public interface IThemeService
    {
        /// <summary>
        /// Returns a list of <see cref="ThemeInfo"/> of the installed themes.
        /// </summary>
        /// <returns></returns>
        Task<List<ThemeInfo>> GetInstalledThemesInfoAsync();
    }
}
