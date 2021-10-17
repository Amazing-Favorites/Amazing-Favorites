using FluentAssertions;
using NUnit.Framework;

namespace Newbe.BookmarkManager.Tests
{
    public class ValueTupleTest
    {
        [Test]
        public void Test()
        {
            var str1 = "nice";
            var str2 = "good";
            (str1, str2).Should().BeEquivalentTo((str1, str2));
        }
    }
}