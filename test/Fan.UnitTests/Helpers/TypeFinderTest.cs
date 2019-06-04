using Fan.Data;
using Fan.Helpers;
using Xunit;

namespace Fan.UnitTests.Helpers
{
    /// <summary>
    /// Test <see cref="TypeFinder"/>.
    /// </summary>
    public class TypeFinderTest
    {
        /// <summary>
        /// TypeFinder can find interface type from dll.
        /// </summary>
        [Fact]
        public void TypeFinder_is_able_to_find_IEntityModelBuilder_from_dlls()
        {
           var consumers = TypeFinder.Find(typeof(IEntityModelBuilder));
            Assert.NotEmpty(consumers);
        }

        /// <summary>
        /// TypeFinder scans only matched dll files.
        /// </summary>
        [Fact]
        public void TypeFind_is_able_to_only_load_dlls_that_match_pattern()
        {
            Assert.False(TypeFinder.IsDllMatch("xunit.runner.visualstudio.dotnetcore.testadapter.dll"));
            Assert.True(TypeFinder.IsDllMatch("Fan.Blog.dll"));
        }
    }
}
