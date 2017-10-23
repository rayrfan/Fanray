using System;
using System.Text;

namespace Fan.Shortcodes
{
    /// <summary>
    /// Shortcode that allows user to paste in source code, like [code lang="lang of choice"]paste your code here[/code].
    /// Note the quotes around lang are not necessary.
    /// </summary>
    /// <remarks>
    /// This shortcode uses <see cref="http://alexgorbatchev.com/SyntaxHighlighter/"/> to render 
    /// content given the language.  If the language is not found among the supported ones, by
    /// default SyntaxHighlighter ignores it by surrounding content with p tag instead of pre tag,
    /// Here if input language is not found I default it to "text".
    /// </remarks>
    public class SourceCodeShortcode : Shortcode
    {
        /// <summary>
        /// Supported language aliases lookup.
        /// </summary>
        /// <remarks>
        /// Language list is based on <see cref="http://alexgorbatchev.com/SyntaxHighlighter/manual/brushes/"/>
        /// and <see cref="https://en.support.wordpress.com/code/posting-source-code/"/> with modifications.
        /// </remarks>
        private string[,] lang_alias_lookup = {
            { "as3", "as3" }, { "actionscript3", "as3" }, // ActionScript3
            { "bash", "bash" }, { "shell", "bash" }, // Bash
            { "clojure", "clojure" }, // Clojure
            { "cf", "cf" }, { "coldfusion", "cf" }, // ColdFusion            
            { "cs", "csharp" }, { "c#", "csharp" }, { "c-sharp", "csharp" }, { "csharp", "csharp" }, // C#
            { "c", "cpp" }, { "cpp", "cpp" }, { "c++", "cpp" }, // C / C++
            { "css", "css" }, // CSS
            { "delphi", "delphi" }, { "pas", "delphi" }, { "pascal", "delphi" }, // Delphi
            { "diff", "diff" }, { "patch", "diff" }, // Diff
            { "erl", "erlang" }, { "erlang", "erlang" }, // Erlang
            { "groovy", "groovy" }, // Groovy
            { "js", "js" }, { "jscript", "js" }, { "javascript", "js" }, // JavaScript
            { "java", "java" }, // Java
            { "jfx", "javafx" }, { "javafx", "javafx" }, // JavaFX
            { "perl", "perl" }, { "pl", "perl" }, // Perl
            { "php", "php" }, // PHP
            { "plain", "text" }, { "text", "text" }, // Plain Text
            { "ps", "powershell" }, { "powershell", "powershell" }, // PowerShell
            { "python", "py" }, { "py", "py" }, // Python
            { "r", "r" }, // R
            { "ruby", "ruby" }, { "rails", "ruby" }, { "ror", "ruby" }, // Ruby
            { "scala", "scala" }, // Scala
            { "sql", "sql" }, // SQL
            { "vb", "vb" }, { "vbnet", "vb" }, // VB
            { "html", "xml" }, { "htm", "xml" }, { "xhtml", "xml" }, { "xslt", "xml" }, { "xml", "xml" }, // XML / HTML
        };

        /// <summary>
        /// The source code language.
        /// </summary>
        public string Language { get; set; }
        /// <summary>
        /// An shorter alias of Language.
        /// </summary>
        public string Lang { get; set; }
        /// <summary>
        /// (number) — Use this to change what number the line numbering starts at. It defaults to 1.
        /// </summary>
        public string Firstline { get; set; }
        /// <summary>
        /// (true/false) — If false, the line numbering on the left side will be hidden. Defaults to true.
        /// </summary>
        public string Gutter { get; set; }
        /// <summary>
        /// (comma-seperated list of numbers) — You can list the line numbers you want to be highlighted. For example 4,7,19.
        /// </summary>
        public string Highlight { get; set; }
        /// <summary>
        /// (true/false) — If true, any HTML/XML in your code will be highlighted. This is useful when you are mixing code into HTML, such as PHP inside of HTML. Defaults to false and will only work with certain code languages.
        /// </summary>
        public string Htmlscript { get; set; }

        /// <summary>
        /// Returns html SyntaxHighlighter expects to understand.
        /// </summary>
        /// <remarks>
        /// Config params are based on <see cref="http://alexgorbatchev.com/SyntaxHighlighter/manual/configuration/"/> 
        /// and <see cref="https://en.support.wordpress.com/code/posting-source-code/"/>. Only part of all params I
        /// think useful are supported here.
        /// </remarks>
        public override string Process()
        {
            if (Content.IsNullOrEmpty()) return "";

            string brush = GetBrush();
            string firstline = Firstline.IsNullOrEmpty() ? "" : $"; first-line: {Firstline}";
            string gutter = Gutter.IsNullOrEmpty() ? "" : $"; gutter: {Gutter}";
            string highlight = Highlight.IsNullOrEmpty() ? "" : $"; highlight: [{Highlight}]";
            string htmlscript = Htmlscript.IsNullOrEmpty() ? "" : $"; html-script: {Htmlscript}";

            // massage the code to make syntaxhighlighter happy, olw mixes in p, br and /n 
            StringBuilder sb = new StringBuilder(Content);
            Content = sb.Replace("<p>", "")
                        .Replace("</p>", "")
                        .Replace("<br>", "\n")
                        .Replace("\n\n", "\n")
                        .ToString();

            return $"<pre class=\"{brush}{firstline}{gutter}{highlight}{htmlscript}\">{Content}</pre>";
        }

        /// <summary>
        /// Returns brush based on <see cref="Language"/> or <see cref="Lang"/> property, and it
        /// matches any language alias, for example, all these "c#", "cs", "c-sharp" match to "csharp".
        /// If neither Language or Lang property mataches anything, "text" which is used as default.
        /// </summary>
        /// <returns></returns>
		public string GetBrush()
		{
			// set lang to "text" if no language attri is set
			if (Lang.IsNullOrEmpty() && Language.IsNullOrEmpty())
				return "brush: text";

			// get lang
			string lang = Language ?? Lang;

			// lookup
			for (int i = 0; i < lang_alias_lookup.GetLength(0); i++)
			{
				if (lang_alias_lookup[i, 0].Equals(lang, StringComparison.InvariantCultureIgnoreCase))
				{
					lang = lang_alias_lookup[i, 1];

					return $"brush: {lang}";
				}
			}

			return "brush: text";
		}
	}
}
