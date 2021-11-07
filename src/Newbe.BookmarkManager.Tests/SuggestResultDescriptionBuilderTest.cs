using FluentAssertions;
using Newbe.BookmarkManager.Services;
using NUnit.Framework;

namespace Newbe.BookmarkManager.Tests
{
    public class SuggestResultDescriptionBuilderTest
    {
        [Test]
        public void Normal()
        {
            new SuggestResultDescriptionBuilder()
                .AddUrl("https://www.newbe.pro")
                .AddText("nice")
                .AddDim("wow")
                .Build()
                .Should().Be("<url>https://www.newbe.pro</url> nice<dim>wow</dim> ");
        }
    }
}