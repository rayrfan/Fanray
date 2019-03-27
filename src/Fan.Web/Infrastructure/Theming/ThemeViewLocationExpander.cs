using Fan.Settings;
using Microsoft.AspNetCore.Mvc.Razor;
using System;
using System.Collections.Generic;
using System.Linq;

namespace Fan.Web.Infrastructure.Theming
{
    /// <summary>
    /// Expand view location to themes, theme's view overrides view with same name outside.
    /// </summary>
    public class ThemeViewLocationExpander : IViewLocationExpander
    {
        private const string THEME_KEY = "fan-theme";

        /// <summary>
        /// Invoked by a <see cref="RazorViewEngine"/> to determine potential
        /// locations for a view. It adds two view folders for theme to existing view locations.
        /// </summary>
        /// <param name="context"> 
        /// The <see cref="ViewLocationExpanderContext"/> for the current view location expansion operation.
        /// </param>
        /// <param name="viewLocations">The sequence of view locations to expand.</param>
        /// <returns></returns>
        public IEnumerable<string> ExpandViewLocations(ViewLocationExpanderContext context, IEnumerable<string> viewLocations)
        {
            if (context.Values.TryGetValue(THEME_KEY, out string theme))
            {
                viewLocations = new[] {
                        $"/Themes/{theme}/Views/{{1}}/{{0}}.cshtml",
                        $"/Themes/{theme}/Views/Shared/{{0}}.cshtml",
                    }
                    .Concat(viewLocations);
            }

            return viewLocations;
        }

        /// <summary>
        /// Invoked by a <see cref="RazorViewEngine"/> to determine the values that would be consumed 
        /// by this instance of <see cref="IViewLocationExpander"/>. It's filled in from <see cref="CoreSettings.Theme"/>.
        /// 
        /// The calculated values are used to determine if the view location has changed
        /// since the last time it was located.
        /// </summary>
        /// <param name="context"> 
        /// The <see cref="ViewLocationExpanderContext"/> for the current view location expansion operation.
        /// </param>
        public void PopulateValues(ViewLocationExpanderContext context)
        {
            if (context == null)
            {
                throw new ArgumentNullException(nameof(context));
            }

            var settingSvc = (ISettingService)context.ActionContext.HttpContext.RequestServices.GetService(typeof(ISettingService));
            var settings = settingSvc.GetSettingsAsync<CoreSettings>().Result;
            context.Values[THEME_KEY] = settings.Theme;
        }
    }
}
