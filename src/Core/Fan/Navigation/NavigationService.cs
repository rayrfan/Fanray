using Fan.Data;
using Fan.Exceptions;
using Fan.Settings;
using Fan.Themes;
using MediatR;
using Microsoft.Extensions.Caching.Distributed;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace Fan.Navigation
{
    /// <summary>
    /// The site navigation service.
    /// </summary>
    public class NavigationService : INavigationService,
                                     INotificationHandler<NavUpdated>,
                                     INotificationHandler<NavDeleted>
    {
        private readonly IMetaRepository metaRepository;
        private readonly IEnumerable<INavProvider> navProviders;
        private readonly ISettingService settingService;
        private readonly IDistributedCache cache;

        public NavigationService(IMetaRepository metaRepository,
            IEnumerable<INavProvider> navProviders,
            ISettingService settingService,
            IDistributedCache cache)
        {
            this.metaRepository = metaRepository;
            this.navProviders = navProviders;
            this.settingService = settingService;
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

        public async Task SortNavInMenuAsync(EMenu menuId, int index, int oldIndex)
        {
            var navList = await GetMenuAsync(menuId);
            var nav = navList[oldIndex];
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

        public async Task SetNavAsHome(int navId, ENavType navType)
        {
            var coreSettings = await settingService.GetSettingsAsync<CoreSettings>();
            coreSettings.Home.Id = navId;
            coreSettings.Home.Type = navType;
            await settingService.UpsertSettingsAsync(coreSettings);
        }

        // -------------------------------------------------------------------- event handlers

        /// <summary>
        /// Handles <see cref="NavUpdated"/> event raised by an <see cref="INavProvider"/>, it 
        /// invalidates caches for all menus.
        /// </summary>
        /// <param name="notification"></param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        public async Task Handle(NavUpdated notification, CancellationToken cancellationToken)
        {
            // if nav updated was a page and it is now a draft
            if (notification.Type == ENavType.Page && notification.IsDraft)
            {
                // 1. update home
                await HandleHomeAsync(notification.Id, notification.Type);

                // 2. remove it from any menus that contain it
                await HandleMenusAsync(notification.Id, notification.Type);
            }

            // invalidate all menus cache
            var menuMetas = await metaRepository.GetListAsync(EMetaType.Menu);
            foreach (var meta in menuMetas)
            {
                await InvalidateMenuCacheAsync(meta.Key);
            }
        }

        /// <summary>
        /// Handles <see cref="NavDeleted"/> event raised by an <see cref="INavProvider"/>.
        /// </summary>
        /// <param name="notification">The <see cref="NavDeleted"/> notification.</param>
        /// <param name="cancellationToken"></param>
        /// <returns></returns>
        /// <remarks>
        /// When a nav is delete, this method handles it by going through all menus and remove the 
        /// deleted <see cref="Nav"/>. If the deleted nav is used for home, then blog app will be 
        /// used as default for home.
        /// </remarks>
        public async Task Handle(NavDeleted notification, CancellationToken cancellationToken)
        {
            // in case the deleted nav was set home
            await HandleHomeAsync(notification.Id, notification.Type);

            // remove the deleted nav from all menus that contains it
            await HandleMenusAsync(notification.Id, notification.Type);
        }

        /// <summary>
        /// When a nav is updated / deleted, site home may need to be updated by setting blog app
        /// as home.
        /// </summary>
        /// <returns></returns>
        private async Task HandleHomeAsync(int navId, ENavType navType)
        {
            var coreSettings = await settingService.GetSettingsAsync<CoreSettings>();
            if (coreSettings.Home.Id == navId && coreSettings.Home.Type == navType)
            {
                coreSettings.Home.Id = App.BLOG_APP_ID;
                coreSettings.Home.Type = ENavType.App;
                await settingService.UpsertSettingsAsync(coreSettings);
            }
        }

        /// <summary>
        /// When a nav is updated / deleted, menus may need to remove the <see cref="Nav"/>.
        /// </summary>
        /// <param name="navId"></param>
        /// <param name="navType"></param>
        private async Task HandleMenusAsync(int navId, ENavType navType)
        {
            var menuMetas = await metaRepository.GetListAsync(EMetaType.Menu);
            foreach (var meta in menuMetas) // List<Meta>
            {
                var navList = JsonConvert.DeserializeObject<IList<Nav>>(meta.Value);

                foreach (var nav in navList)
                {
                    // if the menu contains the deleted nav, remove it
                    if (nav.Id == navId && nav.Type == navType)
                    {
                        navList.Remove(nav);

                        // if menu got nav removed, update it
                        meta.Value = JsonConvert.SerializeObject(navList);
                        await metaRepository.UpdateAsync(meta);

                        // invalidate the menu cache
                        await InvalidateMenuCacheAsync(meta.Key);

                        // once removed break out
                        break;
                    }
                }
            }
        }

        // -------------------------------------------------------------------- private methods

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
                try
                {
                    if (nav.Type == ENavType.CustomLink || !includeNavUrl) continue;

                    var navProvider = navProviders.SingleOrDefault(p => p.CanProvideNav(nav.Type));
                    nav.Url = await navProvider.GetNavUrlAsync(nav.Id);
                    if (!nav.Url.StartsWith('/')) nav.Url = $"/{nav.Url}";
                }
                catch(FanException ex) when (ex.ExceptionType == EExceptionType.ResourceNotFound)
                {
                    continue;
                }
            }

            return navList;
        }

        private async Task UpdateMetaAsync(EMenu menuId, IList<Nav> navList)
        {
            var meta = await metaRepository.GetAsync(menuId.ToString(), EMetaType.Menu);
            meta.Value = JsonConvert.SerializeObject(navList);
            await metaRepository.UpdateAsync(meta);
            await InvalidateMenuCacheAsync(menuId);
        }

        private async Task InvalidateMenuCacheAsync(EMenu menuId)
        {
            var key = string.Format(KEY_MENU, menuId);
            await cache.RemoveAsync(key);
        }

        private async Task InvalidateMenuCacheAsync(string metaKey)
        {
            // convert "menu1" to enum ignoreCase
            Enum.TryParse(metaKey, true, out EMenu menuId);
            if (menuId <= 0) return;

            await InvalidateMenuCacheAsync(menuId);
        }
    }
}
