using System;
using Autofac.Extras.Moq;
using FluentAssertions;
using Newbe.BookmarkManager.Services;
using NUnit.Framework;

namespace Newbe.BookmarkManager.Tests
{
    public class UrlHashServiceTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public void Hash()
        {
            using var mocker = AutoMock.GetLoose();
            var service = mocker.Create<UrlHashService>();
            var hash1 = service.GetHash("https://www.newbe.pro");
            var hash2 = service.GetHash("https://www.newbe.pro/");
            Console.WriteLine(hash1);
            Console.WriteLine(hash2);
            hash1.Should().NotBe(hash2);
        }
    }
}