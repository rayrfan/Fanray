using Fan.Data;
using Fan.Themes;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Fan.Navigation
{
    /// <summary>
    /// The site navigation service.
    /// </summary>
    public class NavigationService : INavigationService
    {
        private readonly IMetaRepository metaRepository;
        private readonly IEnumerable<INavProvider> navProviders;

        public NavigationService(IMetaRepository metaRepository,
            IEnumerable<INavProvider> navProviders)
        {
            this.metaRepository = metaRepository;
            this.navProviders = navProviders;
        }

        /// <summary>
        /// TODO caching
        /// </summary>
        /// <param name="menu"></param>
        /// <returns></returns>
        public async Task<IList<Nav>> GetMenu(EMenu menu, bool includeNavUrl = false)
        {
            // meta
            var meta = await metaRepository.GetAsync(menu.ToString().ToLower(), EMetaType.Menu);
            if (meta == null)
            {
                // if not found create a meta
                meta = await metaRepository.CreateAsync(new Meta
                {
                    Key = menu.ToString().ToLower(),
                    Value = JsonConvert.SerializeObject(new List<Nav>()),
                    Type = EMetaType.Menu,
                });
            }

            // navList
            var navList = JsonConvert.DeserializeObject<IList<Nav>>(meta.Value);

            // lookup nav
            foreach (var nav in navList)
            {
                if (nav.Type == ENavType.CustomLink || !includeNavUrl) continue;

                var navProvider = navProviders.SingleOrDefault(p => p.CanProvideNav(nav.Type));
                nav.Url = await navProvider.GetNavUrlAsync(nav.Id);
                if (!nav.Url.StartsWith('/')) nav.Url = $"/{nav.Url}";
            }

            return navList;
        }

        public async Task AddNavToMenuAsync(EMenu menuId, int index, Nav nav)
        {
            var navList = await GetMenu(menuId);
            navList.Insert(index, nav);
            await UpdateMetaAsync(menuId, navList);
        }

        public async Task SortNavInMenuAsync(EMenu menuId, int index, int oldIndex, Nav nav)
        {
            var navList = await GetMenu(menuId);
            navList.RemoveAt(oldIndex);
            navList.Insert(index, nav);
            await UpdateMetaAsync(menuId, navList);
        }

        public async Task RemoveNavFromMenuAsync(EMenu menuId, int index)
        {
            var navList = await GetMenu(menuId);
            navList.RemoveAt(index);
            await UpdateMetaAsync(menuId, navList);
        }

        public async Task UpdateNavInMenuAsync(EMenu menuId, int index, Nav nav)
        {
            var navList = await GetMenu(menuId);
            navList[index] = nav;
            await UpdateMetaAsync(menuId, navList);
        }

        private async Task UpdateMetaAsync(EMenu menuId, IList<Nav> navList)
        {
            var meta = await metaRepository.GetAsync(menuId.ToString(), EMetaType.Menu);
            meta.Value = JsonConvert.SerializeObject(navList);
            await metaRepository.UpdateAsync(meta);
        }
    }
}
