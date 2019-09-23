using Fan.Themes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fan.Navigation
{
    public interface INavigationService
    {
        Task<IList<Nav>> GetMenu(EMenu menu, bool includeNavUrl = false);
        Task AddNavToMenuAsync(EMenu menuId, int index, Nav nav);
        Task SortNavInMenuAsync(EMenu menuId, int index, int oldIndex, Nav nav);
        Task RemoveNavFromMenuAsync(EMenu menuId, int index);
        Task UpdateNavInMenuAsync(EMenu menuId, int index, Nav nav);
    }
}