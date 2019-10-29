using Fan.Navigation;
using Fan.Themes;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Threading.Tasks;

namespace Fan.WebApp.Manage.Admin.Shared
{
    public class NavSettingsModel : PageModel
    {
        private readonly INavigationService navigationService;

        public NavSettingsModel(INavigationService navigationService)
        {
            this.navigationService = navigationService;
        }

        public string NavJson { get; set; }
        public int MenuId { get; set; }
        public int Index { get; set; }
        public string IsCustomLink { get; set; }

        public async Task OnGetAsync(EMenu menuId, int index)
        {
            var navList = await navigationService.GetMenuAsync(menuId);
            var nav = navList[index];

            IsCustomLink = (nav.Type == ENavType.CustomLink).ToString().ToLower();
            Index = index;
            NavJson = JsonConvert.SerializeObject(nav);
            MenuId = (int) menuId;
        }

        public async Task<IActionResult> OnPostAsync([FromBody]UpdateNavIM im)
        {
            if (ModelState.IsValid)
            {
                await navigationService.UpdateNavInMenuAsync(im.MenuId, im.Index, new Nav
                {
                    Id = im.Id,
                    Text = im.Text.Trim(),
                    Type = im.Type,
                    Title = im.Title?.Trim(),
                    Url = im.Url,
                });
                return new JsonResult("Menu item updated.");
            }

            return BadRequest("Invalid form values submitted.");
        }
    }
}