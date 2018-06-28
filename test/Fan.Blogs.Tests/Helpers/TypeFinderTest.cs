using Fan.Data;
using Fan.Helpers;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace Fan.Blogs.Tests.Helpers
{
    /// <summary>
    /// Test <see cref="TypeFinder"/> logic.
    /// </summary>
    /// <remarks>
    /// Since TypeFinder needs dlls, I put it in the web test project.
    /// </remarks>
    public class TypeFinderTest
    {
        TypeFinder _typeFinder;
        public TypeFinderTest()
        {
            var serviceProvider = new ServiceCollection().AddMemoryCache().AddLogging().BuildServiceProvider();
            var loggerFactory = serviceProvider.GetService<ILoggerFactory>();
            var logger = loggerFactory.CreateLogger<TypeFinder>();

            _typeFinder = new TypeFinder(logger);
        }

        [Fact]
        public void TypeFinder_is_able_to_find_IEntityModelBuilder_from_dlls()
        {
           var consumers = _typeFinder.Find(typeof(IEntityModelBuilder));
            Assert.NotEmpty(consumers);
        }
    }
}
