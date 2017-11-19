using Fan.Helpers;
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
            _typeFinder = new TypeFinder();
        }

        //[Fact]
        //public void FindTest()
        //{
        //    // must provide a specific BlogPost for it to work
        //    //var consumers = _typeFinder.Find(typeof(IAsyncNotificationHandler<>)).ToList();
        //    //Assert.NotEmpty(consumers);
        //}
    }
}
