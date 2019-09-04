using Fan.Extensibility;
using Fan.Plugins;
using System.Text.RegularExpressions;
using Xunit;

namespace Fan.UnitTests.Extensibility
{
    public class ExtensibleServiceTest
    {
        /// <summary>
        /// An extension's folder allows "a-zA-Z", "_", "-", "." and numbers.
        /// </summary>
        [Theory]
        [InlineData("MyPlugin", true)]
        [InlineData("my-plugin", true)]
        [InlineData("my_plugin", true)]
        [InlineData("2my_plugin", true)]
        [InlineData("MyPLUGIN", true)]
        [InlineData("My.plugin", true)]
        [InlineData("My Plugin", false)]
        [InlineData("My/Plugin", false)]
        [InlineData("$MyPlugin", false)]
        public void IsValidExtensionFolder_Test(string folder, bool expected)
        {
            var actual = new Regex(ExtensibleService<PluginManifest, Plugin>.FOLDER_REGEX).IsMatch(folder);
            Assert.Equal(expected, actual);
        }
    }
}
