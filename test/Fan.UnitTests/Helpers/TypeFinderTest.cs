using Fan.Data;
using Fan.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Fan.UnitTests.Helpers
{
    /// <summary>
    /// Test <see cref="TypeFinder"/> logic.
    /// </summary>
    public class TypeFinderTest
    {
        TypeFinder _typeFinder;
        public TypeFinderTest()
        {
            var loggerFactory = new ServiceCollection().AddLogging().BuildServiceProvider().GetService<ILoggerFactory>();
            _typeFinder = new TypeFinder(loggerFactory);
        }

        [Fact]
        public void TypeFinder_is_able_to_find_IEntityModelBuilder_from_dlls()
        {
           var consumers = _typeFinder.Find(typeof(IEntityModelBuilder));
            Assert.NotEmpty(consumers);
        }
    }
}
