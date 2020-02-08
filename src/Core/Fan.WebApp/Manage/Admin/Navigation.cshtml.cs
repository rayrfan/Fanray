using Fan.Blog.Models;
using Fan.Blog.Services.Interfaces;
using Fan.Navigation;
using Fan.Settings;
using Fan.Themes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Threading.Tasks;

namespace Fan.WebApp.Manage.Admin
{
    public class NavigationModel : PageModel
    {
        private readonly IThemeService themeService;
        private readonly INavigationService navigationService;
        private readonly ISettingService settingService;
        private readonly IPageService pageService;
        private readonly ICategoryService categoryService;

        public NavigationModel(
            IThemeService themeService,
            INavigationService navigationService,
            ISettingService settingService,
            IPageService pageService,
            ICategoryService categoryService)
        {
            this.categoryService = categoryService;
            this.themeService = themeService;
            this.navigationService = navigationService;
            this.settingService = settingService;
            this.pageService = pageService;
        }

        public const string NAV_SETTINGS_URL = "/Admin/Shared/NavSettings?menuId={0}&index={1}";
        public string PagesJson { get; private set; }
        public string CatsJson { get; private set; }
        public string AppsJson { get; private set; }
        public string HomeJson { get; private set; }
        public string MenusJson { get; private set; }
        public string MenuPanelsJson { get; private set; }
        public int SelectedMenuId { get; private set; }

        /// <summary>
        /// Initializes page.
        /// </summary>
        /// <returns></returns>
        public async Task OnGetAsync()
        {
            // pages
            var pages = await pageService.GetParentsAsync(withChildren: false);
            PagesJson = JsonConvert.SerializeObject(
                from page in pages
                where page.Status != Blog.Enums.EPostStatus.Draft
                select new { page.Id, Text = page.Title, Type = ENavType.Page }
            );

            // blog cats
            var cats = await categoryService.GetAllAsync();
            CatsJson = JsonConvert.SerializeObject(
                from cat in cats
                select new { cat.Id, Text = cat.Title, Type = ENavType.BlogCategory }
            );

            // apps
            AppsJson = JsonConvert.SerializeObject(App.AppNavs);

            // home
            var coreSettings = await settingService.GetSettingsAsync<CoreSettings>();
            var nav = coreSettings.Home;
            nav.Text = GetOriginalNavTitle(nav, pages, cats);
            HomeJson = JsonConvert.SerializeObject(nav);

            // menus
            var currentTheme = (await themeService.GetManifestsAsync())
                   .Single(t => t.Name.Equals(coreSettings.Theme, StringComparison.OrdinalIgnoreCase));
            var menus = currentTheme.Menus;
            var menuVMs = new List<MenuVM>();
            foreach (var menu in menus)
            {
                var navList = await navigationService.GetMenuAsync(menu.Id);
                menuVMs.Add(new MenuVM(menu, navList, pages, cats));
            }
            MenusJson = JsonConvert.SerializeObject(menuVMs);

            // selected menu
            SelectedMenuId = menus.Length > 0 ? (int) menus[0].Id : 0;

            // menu panels
            var menuPanels = new bool[menus.Length];
            for (int i = 0; i < menuPanels.Length; i++)
            {
                menuPanels[i] = true;
            }
            MenuPanelsJson = JsonConvert.SerializeObject(menuPanels);
        }

        /// <summary>
        /// Adds a nav to menu.
        /// </summary>
        /// <param name="im">Input model of the nav being added.</param>
        /// <returns>
        /// A <see cref="NavVM"/> with SettingsUrl.
        /// </returns>
        public async Task<IActionResult> OnPostAddAsync([FromBody]AddNavIM im)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid form values submitted.");

            // from a menu since menu id is an int
            if (int.TryParse(im.FromId, out int fromId))
            {
                // remove the item from the old menu
                await navigationService.RemoveNavFromMenuAsync((EMenu)fromId, im.OldIndex);
            }

            var nav = new Nav
            {
                Id = im.Id,
                Text = im.Text.Trim(),
                Type = im.Type,
            };
            await navigationService.AddNavToMenuAsync(im.MenuId, im.Index, nav);

            var navVM = new NavVM(nav) {
                SettingsUrl = string.Format(NAV_SETTINGS_URL, im.MenuId, im.Index)
            };

            return new JsonResult(navVM);
        }

        /// <summary>
        /// Sorts a menu.
        /// </summary>
        /// <param name="im"></param>
        /// <returns></returns>
        public async Task OnPostSortAsync([FromBody]SortNavIM im) =>
            await navigationService.SortNavInMenuAsync(im.MenuId, im.Index, im.OldIndex);

        /// <summary>
        /// Sets a nav as home.
        /// </summary>
        /// <param name="nav">The nav being set as home.</param>
        /// <returns></returns>
        public async Task OnPostHomeAsync([FromBody]Nav nav)
        {
            await navigationService.SetNavAsHome(nav.Id, nav.Type);
        }

        /// <summary>
        /// Removes a nav from menu.
        /// </summary>
        /// <param name="menuId"></param>
        /// <param name="index"></param>
        /// <returns></returns>
        public async Task OnDeleteAsync(EMenu menuId, int index) =>
            await navigationService.RemoveNavFromMenuAsync(menuId, index);

        public async Task<IActionResult> OnPostCustomLinkAsync([FromBody]AddNavIM im)
        {
            if (!ModelState.IsValid)
                return BadRequest("Invalid form values submitted.");

            var nav = new Nav
            {
                Id = 0,
                Text = im.Text.Trim(),
                Type = ENavType.CustomLink,
                Url = im.Url?.Trim(),
            };

            await navigationService.AddNavToMenuAsync(im.MenuId, im.Index, nav);

            var navVM = new NavVM(nav)
            {
                SettingsUrl = string.Format(NAV_SETTINGS_URL, im.MenuId, im.Index),
                Type = ENavType.CustomLink,
            };

            return new JsonResult(navVM);
        }

        public static string GetOriginalNavTitle(Nav nav, IList<Blog.Models.Page> pages, IList<Category> cats)
        {
            switch (nav.Type)
            {
                case ENavType.Page:
                    var page = pages.Single(p => p.Id == nav.Id);
                    return page.Title;
                case ENavType.App:
                    var app = App.AppNavs.Single(a => a.Id == nav.Id);
                    return app.Text;
                case ENavType.BlogCategory:
                    var cat = cats.Single(c => c.Id == nav.Id);
                    return cat.Title;
                default:
                    return null;
            }
        }
    }

    public class MenuVM : MenuInfo
    {
        public MenuVM(MenuInfo info, IList<Nav> navList, IList<Blog.Models.Page> pages, IList<Category> cats)
        {
            Id = info.Id;
            Name = info.Name;
            Description = info.Description;
            Navs = new List<NavVM>();

            for (int i = 0; i < navList.Count; i++)
            {
                var navVM = new NavVM(navList[i])
                {
                    OrigNavName = NavigationModel.GetOriginalNavTitle(navList[i], pages, cats),
                    SettingsUrl = string.Format(NavigationModel.NAV_SETTINGS_URL, Id, i),
                };
                Navs.Add(navVM);
            }
        }
        public IList<NavVM> Navs { get; private set; }
    }

    public class NavVM : Nav
    {
        public NavVM(Nav nav)
        {
            Id = nav.Id;
            Text = nav.Text;
            Title = nav.Title;
            Type = nav.Type;
            Url = nav.Url;
        }

        public string OrigNavName { get; set; }
        public string SettingsUrl { get; set; }
    }

    public class AddNavIM
    {
        public string FromId { get; set; }
        public EMenu MenuId { get; set; } // which menu
        public int Index { get; set; } // nav sort order
        public int OldIndex { get; set; }
        public int Id { get; set; }
        [Required]
        public string Text { get; set; }
        public ENavType Type { get; set; }
        public string Url { get; set; } // custom link
    }

    public class UpdateNavIM : AddNavIM
    {
        public string Title { get; set; }
    }

    public class SortNavIM : AddNavIM
    {
        public string Title { get; set; }
    }
}