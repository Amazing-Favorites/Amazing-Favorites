using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Autofac.Extras.Moq;
using FluentAssertions;
using Moq;
using Newbe.BookmarkManager.Services;
using NUnit.Framework;

namespace Newbe.BookmarkManager.Tests
{
    public class AfCodeServiceTest
    {
        public const string ShortUrl = "http://www.newbe.pro";
        public const string LongUrl = "https://mvp.microsoft.com/zh-cn/PublicProfile/5004283?fullName=Justin%20Yu";

        [Test]
        [TestCase(ShortUrl, AfCodeType.JsonBase64)]
        [TestCase(ShortUrl, AfCodeType.CompressionJsonBase64)]
        [TestCase(ShortUrl, AfCodeType.PlainText)]
        [TestCase(LongUrl, AfCodeType.JsonBase64)]
        [TestCase(LongUrl, AfCodeType.CompressionJsonBase64)]
        [TestCase(LongUrl, AfCodeType.PlainText)]
        public async Task Test(string url, AfCodeType type)
        {
            using var mocker = AutoMock.GetLoose();
            var service = mocker.Create<AfCodeService>();
            var bk = new Bk
            {
                Url = url,
                Title = "newbe.pro",
                Tags = new List<string>
                {
                    "newbe", "blog"
                }
            };
            mocker.Mock<IIndexedDbRepo<Bk, string>>()
                .Setup(x => x.GetAsync(url))
                .ReturnsAsync(bk);

            // act
            var code = await service.CreateAfCodeAsync(url, type);
            Console.WriteLine(code);

            var tryParseAsync = await service.TryParseAsync(code, out var result);
            tryParseAsync.Should().BeTrue();
            result!.Should().NotBeNull();
            result.Url.Should().Be(bk.Url);
            result.Title.Should().Be(bk.Title);
            result.Tags.Should().BeEquivalentTo(bk.Tags);
        }
    }
}