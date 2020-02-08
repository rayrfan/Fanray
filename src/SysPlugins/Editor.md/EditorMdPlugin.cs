using Fan.Plugins;

namespace Editor.md
{
    public class EditorMdPlugin : Plugin
    {
        /// <summary>
        /// Editor language, default is English.
        /// </summary>
        public string Language { get; set; } = "en";

        /// <summary>
        /// Whether dark theme is on, default is false.
        /// </summary>
        /// <remarks>
        /// True will assign <see cref="EditorTheme"/> the value "dark", and <see cref="CodeMirrorTheme"/> 
        /// can only choose from a list of dark themes; false will assign <see cref="EditorTheme"/>
        /// the value "default", and <see cref="CodeMirrorTheme"/> chooses from a list of light themes.
        /// </remarks>
        public bool DarkTheme { get; set; } = false;

        /// <summary>
        /// CodeMirror Theme, default is "default".
        /// </summary>
        /// <remarks>
        /// This value will assigned to the "editorTheme" property on Editor.md.
        /// See <a href="https://pandao.github.io/editor.md/examples/themes.html">Editor Themes</a>,
        /// and <a href="https://codemirror.net/demo/theme.html">CodeMirror Themes</a>
        /// </remarks>
        public string CodeMirrorTheme { get; set; } = "default";

        public override string GetFootScriptsViewName() => "EditorMdScripts";
        public override string GetStylesViewName() => "EditorMdStyles";

        /// <summary>
        /// EditorMd plugin's Settings URL.
        /// </summary>
        public override string SettingsUrl => $"/{PluginService.PLUGIN_DIR}/EditorMdSettings?name=Editor.md";
    }
}

