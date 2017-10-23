using Fan.Shortcodes;
using Xunit;

namespace Fan.Tests.Shortcodes
{
    /// <summary>
    /// Unit tests for <see cref="SourceCodeShortcode"/> class.
    /// </summary>
    public class SourceCodeShortcodeTest
    {
		private IShortcodeService _svc;
		public SourceCodeShortcodeTest()
		{
			_svc = new ShortcodeService();
			_svc.Add<SourceCodeShortcode>(tag: "code");
		}

        /// <summary>
        /// Test <see cref="SourceCodeShortcode.GetBrush"/> returns correct brush given a language alias.
        /// </summary>
        [Theory]
        [InlineData("c#", "csharp")]
        [InlineData("cs", "csharp")]
        [InlineData("htm", "xml")]
        [InlineData("not exist", "text")]
        public void GetBrush_Returns_Correct_Brush_Given_Language_Alias(string alias, string expected)
        {
            var sc = new SourceCodeShortcode
            {
                Lang = alias
            };
            Assert.Equal($"brush: {expected}", sc.GetBrush());
        }

        /// <summary>
        /// When you don't give a langauge attribute it's "text"
        /// </summary>
        [Fact]
        public void When_language_is_missing_brush_set_to_text()
        {
            var content = "[code gutter=false]var i = 1;[/code]";
            var result = _svc.Parse(content);
            Assert.Equal("<pre class=\"brush: text; gutter: false\">var i = 1;</pre>", result);
        }

        /// <summary>
        /// You can have multiple attributes and surround them with or without any of the quote chars.
        /// </summary>
        [Theory]
        [InlineData("<p>[code language=\"c#\" highlight=\"1,3\"]</p><p>var i = 1;</p><p>[/code]</p>")]
        [InlineData("<p>[code language=“c#“ highlight=“1,3“]</p><p>var i = 1;</p><p>[/code]</p>")]
        [InlineData("<p>[code language=’c#’ highlight=’1,3’]</p><p>var i = 1;</p><p>[/code]</p>")]
        [InlineData("<p>[code language=‘c#‘ highlight=’1,3’]</p><p>var i = 1;</p><p>[/code]</p>")]
        [InlineData("<p>[code language=c# highlight=1,3]</p><p>var i = 1;</p><p>[/code]</p>")]
        public void Multiple_attributes_with_or_without_quote(string input)
        {
            var result = _svc.Parse(input);
            Assert.Equal("<p><pre class=\"brush: csharp; highlight: [1,3]\">var i = 1;</pre></p>", result);
        }

        /// <summary>
        /// You can have one attribute and surround it with or without any of the quote chars.
        /// </summary>
        /// <param name="input"></param>
        [Theory]
        [InlineData("<p>[code language=‘cs’]</p><p>var i = 1;</p><p>[/code]</p>")]
        [InlineData("<p>[code language=‘cs‘]</p><p>var i = 1;</p><p>[/code]</p>")]
        [InlineData("<p>[code language=“cs”]</p><p>var i = 1;</p><p>[/code]</p>")]
        [InlineData("<p>[code language= cs]</p><p>var i = 1;</p><p>[/code]</p>")]
        public void Single_attribute_with_or_without_quote(string input)
        {
            var result = _svc.Parse(input);
            Assert.Equal("<p><pre class=\"brush: csharp\">var i = 1;</pre></p>", result);
        }

        /// <summary>
        /// C# code with attributes.
        /// </summary>
        [Fact]
        public void CSharp_code_with_attributes_works()
        {
            var result = _svc.Parse("[code language=cs][Theory][InlineData(\"test\")]public void Test() {}[/code]");
            Assert.Equal("<pre class=\"brush: csharp\">[Theory][InlineData(\"test\")]public void Test() {}</pre>", result);
        }

        /// <summary>
        /// C# code that contains c# attribute which contains in it "[code][/code]" will break! 
        /// THIS IS AN EDGE CASE that does not work, but it should be rare.
        /// </summary>
        [Fact]
        public void CSharp_code_with_attribute_that_contains_multiple_square_brackets_breaks()
        {
            // when the contained attribute contains code that contains [code][/code], it breaks down
            var result = _svc.Parse("[code language=cs][Theory][InlineData(\"[code language=‘cs’]var i = 1;[/code]\")]public void Test() {}[/code]");
            Assert.NotEqual("<pre class=\"brush: csharp\">[Theory][InlineData(\"[code language=‘cs’]var i = 1;[/code]\")]public void Test() {}</pre>", result);
            // it instead escapes the whole thing
            Assert.Equal("[code language=cs][Theory][InlineData(\"[code language=‘cs’]var i = 1;[/code]\")]public void Test() {}[/code]", result);

            // if it's just one pair of [] it works
            result = _svc.Parse("[code language=cs][Theory][InlineData(\"[code language=‘cs’]\")]public void Test() {}[/code]");
            Assert.Equal("<pre class=\"brush: csharp\">[Theory][InlineData(\"[code language=‘cs’]\")]public void Test() {}</pre>", result);
        }


        /// <summary>
        /// User can use either lang or language.
        /// </summary>
        [Fact]
		public void Lang_Is_Alias_Of_Language_Attribute()
		{
			var languageResult = _svc.Parse("[code language=c#]var i = 1;[/code]");
			var langResult = _svc.Parse("[code lang=c#]var i = 1;[/code]");

			Assert.Equal("<pre class=\"brush: csharp\">var i = 1;</pre>", languageResult);
			Assert.Equal("<pre class=\"brush: csharp\">var i = 1;</pre>", langResult);
		}
	}
}
