using System;
using System.Collections.Generic;
using Xunit;

namespace Fan.UnitTests.Helpers
{
    /// <summary>
    /// Unit tests for <see cref="IEnumerableExtensions"/> class.
    /// </summary>
    public class IEnumerableExtensionsTest
    {
        public static IEnumerable<object[]> ListOfStr => new List<object[]>
        {
            new object[] { null, true },
            new object[] { new List<string>(), true },
            new object[] { new List<string> { }, true },
            new object[] { new List<string> { "" }, false },
            new object[] { new List<string> { "test", "" }, false },
        };

        public static IEnumerable<object[]> ListOfInt => new List<object[]>
        {
            new object[] { null, true },
            new object[] { new List<int>(), true },
            new object[] { new List<int> { }, true },
            new object[] { new List<int> { 3 }, false },
        };

        /// <summary>
        /// Test a list of IEnumberable of string.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="expect"></param>
        [Theory]
        [MemberData(nameof(ListOfStr))]
        public void Test_ListOfStrings(IEnumerable<string> data, bool expect)
        {
            Assert.Equal(expect, data.IsNullOrEmpty());
        }

        /// <summary>
        /// Test a list of IEnumberable of int.
        /// </summary>
        /// <param name="data"></param>
        /// <param name="expect"></param>
        [Theory]
        [MemberData(nameof(ListOfInt))]
        public void Test_ListOfInts(IEnumerable<int> data, bool expect)
        {
            Assert.Equal(expect, data.IsNullOrEmpty());
        }
    }
}
