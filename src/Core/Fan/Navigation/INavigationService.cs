using Fan.Themes;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Fan.Navigation
{
    public interface INavigationService
    {
        Task<IList<Nav>> GetMenuAsync(EMenu menuId, bool includeNavUrl = false);
        Task AddNavToMenuAsync(EMenu menuId, int index, Nav nav);
        Task SortNavInMenuAsync(EMenu menuId, int index, int oldIndex);
        Task RemoveNavFromMenuAsync(EMenu menuId, int index);
        Task UpdateNavInMenuAsync(EMenu menuId, int index, Nav nav);
        Task SetNavAsHome(int navId, ENavType navType);
    }
}