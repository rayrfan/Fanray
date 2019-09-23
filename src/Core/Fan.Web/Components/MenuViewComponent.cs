using Fan.Navigation;
using Fan.Themes;
using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;

namespace Fan.Web.Components
{
    public class MenuViewComponent : ViewComponent
    {
        /// <summary>
        /// Renders the view for a menu based on its id.
        /// </summary>
        /// <param name="menu"></param>
        /// <returns></returns>
        /// <remarks>
        /// For the naming of the view files for menus, I could use the "name" of the menus from "theme.json",
        /// e.g. "MainMenu.cshtml" but I feel using <see cref="EMenu"/> as the name e.g. "Menu1.cshtml" is 
        /// a bit more intuitive for theme designers.
        /// </remarks>
        public IViewComponentResult Invoke((EMenu id, IList<Nav> navList) menu)
        {
            return View($"{menu.id}.cshtml", menu.navList);
        }
    }
}
