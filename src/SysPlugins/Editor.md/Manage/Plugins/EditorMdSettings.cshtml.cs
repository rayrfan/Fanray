using Fan.Plugins;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Editor.md.Manage.Plugins
{
    public class EditorMdSettingsModel : PageModel
    {
        protected readonly IPluginService pluginService;
        public EditorMdSettingsModel(IPluginService pluginService)
        {
            this.pluginService = pluginService;
        }

        class LangInfo
        {
            public string Id { get; set; }
            public string DisplayName { get; set; }
        }

        readonly IEnumerable<LangInfo> LanguageInfos = new List<LangInfo>
        {
            new LangInfo { Id = "en", DisplayName = "English" },
            new LangInfo { Id = "zh-cn", DisplayName = "Chinese Simplified (中文简体)" },
            new LangInfo { Id = "zh-tw", DisplayName = "Chinese Traditional (中文繁體)" },
        };

        readonly string[] LightThemes =
        {
            "default",
            "3024-day",
            "ambiance-mobile",
            "base16-light",
            "eclipse",
            "elegant",
            "mdn-like",
            "neat",
            "neo",
            "paraiso-light", // beige color
            "solarized", // grayish
            "xq-light",
        };

        readonly string[] DarkThemes =
        {
            "3024-night",
            "ambiance",
            "base16-dark",
            "blackboard",
            "cobalt", // dark blue
            "erlang-dark",
            "lesser-dark",
            "mbo",
            "midnight",
            "monokai",
            "night",
            "paraiso-dark",
            "pastel-on-dark", // default dark
            "rubyblue", // dark blue
            "the-matrix",
            "tomorrow-night-eighties",
            "twilight",
            "vibrant-ink",
            "xq-dark",
        };

        public string ExtJson { get; set; }
        public string LanguagesJson { get; set; }
        public string LightThemesJson { get; set; }
        public string DarkThemesJson { get; set; }

        /// <summary>
        /// Selected language.
        /// </summary>
        public string LanguageJson { get; set; }
        /// <summary>
        /// Selected CodeMirror theme.
        /// </summary>
        public string Theme { get; set; }

        public async Task OnGetAsync(string name)
        {
            // plugin
            var plugin = (EditorMdPlugin)await pluginService.GetPluginAsync(name);
            ExtJson = JsonConvert.SerializeObject(plugin);

            // languages
            LanguagesJson = JsonConvert.SerializeObject(LanguageInfos);
            LanguageJson = JsonConvert.SerializeObject(LanguageInfos.First(l => l.Id == plugin.Language));

            // themes
            LightThemesJson = JsonConvert.SerializeObject(LightThemes);
            DarkThemesJson = JsonConvert.SerializeObject(DarkThemes);
            Theme = plugin.CodeMirrorTheme;
            if (plugin.DarkTheme && plugin.CodeMirrorTheme == "default")
            {
                Theme = "pastel-on-dark";
            }
        }

        public async Task<IActionResult> OnPostAsync([FromBody]EditorMdPlugin plugin)
        {
            if (ModelState.IsValid)
            {
                await pluginService.UpsertPluginAsync(plugin);
                return new JsonResult("Plugin settings updated.");
            }

            return BadRequest("Failed to update plugin settings.");
        }
    }
}