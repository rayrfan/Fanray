using Fan.Data;
using Fan.Themes;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System;
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
        private readonly IDistributedCache cache;

        public NavigationService(IMetaRepository metaRepository,
            IEnumerable<INavProvider> navProviders,
            IDistributedCache cache)
        {
            this.metaRepository = metaRepository;
            this.navProviders = navProviders;
            this.cache = cache;
        }

        private const string KEY_MENU = "Menu_{0}";
        private static readonly TimeSpan Time_Menu = new TimeSpan(0, 10, 0);

        /// <summary>
        /// Returns a list of <see cref="Nav"/> for a site navigation menu.
        /// </summary>
        /// <param name="menuId">The menu id.</param>
        /// <param name="includeNavUrl">Whether to include nav url.</param>
        /// <returns></returns>
        /// <remarks>
        /// Only cached when <paramref name="includeNavUrl"/> is false as that is for the client.
        /// </remarks>
        public async Task<IList<Nav>> GetMenuAsync(EMenu menuId, bool includeNavUrl = false)
        {
            var key = string.Format(KEY_MENU, menuId);
            return includeNavUrl ?
                await cache.GetAsync(key, Time_Menu, async () => await QueryMenuAsync(menuId, includeNavUrl)) :
                await QueryMenuAsync(menuId, includeNavUrl);
        }

        public async Task AddNavToMenuAsync(EMenu menuId, int index, Nav nav)
        {
            var navList = await GetMenuAsync(menuId);
            navList.Insert(index, nav);
            await UpdateMetaAsync(menuId, navList);
        }

        public async Task SortNavInMenuAsync(EMenu menuId, int index, int oldIndex, Nav nav)
        {
            var navList = await GetMenuAsync(menuId);
            navList.RemoveAt(oldIndex);
            navList.Insert(index, nav);
            await UpdateMetaAsync(menuId, navList);
        }

        public async Task RemoveNavFromMenuAsync(EMenu menuId, int index)
        {
            var navList = await GetMenuAsync(menuId);
            navList.RemoveAt(index);
            await UpdateMetaAsync(menuId, navList);
        }

        public async Task UpdateNavInMenuAsync(EMenu menuId, int index, Nav nav)
        {
            var navList = await GetMenuAsync(menuId);
            navList[index] = nav;
            await UpdateMetaAsync(menuId, navList);
        }

        private async Task<IList<Nav>> QueryMenuAsync(EMenu menuId, bool includeNavUrl = false)
        {
            // meta
            var meta = await metaRepository.GetAsync(menuId.ToString().ToLower(), EMetaType.Menu);
            if (meta == null)
            {
                // if not found create a meta
                meta = await metaRepository.CreateAsync(new Meta
                {
                    Key = menuId.ToString().ToLower(),
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

        private async Task UpdateMetaAsync(EMenu menuId, IList<Nav> navList)
        {
            var meta = await metaRepository.GetAsync(menuId.ToString(), EMetaType.Menu);
            meta.Value = JsonConvert.SerializeObject(navList);
            await metaRepository.UpdateAsync(meta);

            // invalidate cache
            var key = string.Format(KEY_MENU, menuId);
            await cache.RemoveAsync(key);
        }
    }
}
