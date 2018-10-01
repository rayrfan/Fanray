using Fan.Helpers;
using Xunit;

namespace Fan.UnitTests.Helpers
{
    /// <summary>
    /// Test for <see cref="RegexUtilities"/> class.
    /// </summary>
    public class RegexUtilitiesTest
    {
        RegexUtilities _util;
        public RegexUtilitiesTest()
        {
            _util = new RegexUtilities();
        }

        /// <summary>
        /// Test cases for <see cref="RegexUtilities.IsValidEmail(string)"/> method.
        /// </summary>
        /// <param name="email"></param>
        /// <param name="expected"></param>
        /// <remarks>
        /// Test data provided by https://docs.microsoft.com/en-us/dotnet/standard/base-types/how-to-verify-that-strings-are-in-valid-email-format#compiling-the-code
        /// </remarks>
        [Theory]
        [InlineData("david.jones@proseware.com", true)]
        [InlineData("d.j@server1.proseware.com", true)]
        [InlineData("jones@ms1.proseware.com", true)]
        [InlineData("j.@server1.proseware.com", false)]
        [InlineData("j@proseware.com9", true)]
        [InlineData("js#internal@proseware.com", true)]
        [InlineData("j_9@[129.126.118.1]", true)]
        [InlineData("j..s@proseware.com", false)]
        [InlineData("js*@proseware.com", false)]
        [InlineData("js@proseware..com", false)]
        [InlineData("js@proseware.com9", true)]
        [InlineData("j.s@server1.proseware.com", true)]
        [InlineData(@"""j\""s\""""@proseware.com", true)]
        [InlineData("js@contoso.中国", true)]
        [InlineData("username", false)]
        public void IsValidEmail_Test(string email, bool expected)
        {
            Assert.Equal(expected, _util.IsValidEmail(email));
        }
    }
}
