using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac.Extras.Moq;
using Microsoft.Extensions.Options;
using Moq;
using Newbe.BookmarkManager.Services;
using Newbe.BookmarkManager.Services.Configuration;
using NUnit.Framework;
using WebExtensions.Net.Tabs;

namespace Newbe.BookmarkManager.Tests
{
    public class ShowWhatNewJobTest
    {
        [SetUp]
        public void Setup()
        {
        }

        [Test]
        public async Task OpenIfNotShown()
        {
            using var mocker = AutoMock.GetLoose();
            mocker.Mock<INewNotification>()
                .Setup(x => x.NewReleaseAsync(It.IsAny<NewReleaseInput>()))
                .Returns(Task.CompletedTask)
                .Verifiable();

            mocker.Mock<IOptions<StaticUrlOptions>>()
                .Setup(x => x.Value)
                .Returns(new StaticUrlOptions
                {
                    WhatsNew = "https://af.newbe.pro/docs/99-1-Whats-New"
                })
                .Verifiable();
            // act
            var job = mocker.Create<ShowWhatNewJob>();
            await job.StartAsync();
        }

        [Test]
        public async Task DoNotOpenIfNotShown()
        {
            using var mocker = AutoMock.GetLoose();

            mocker.Mock<IIndexedDbRepo<AfMetadata, string>>()
                .Setup(x => x.GetAsync(Consts.SingleOneDataId))
                .ReturnsAsync(new AfMetadata
                {
                    WhatsNewVersion = Consts.CurrentVersion
                });

            // act
            var job = mocker.Create<ShowWhatNewJob>();
            await job.StartAsync();

            mocker.Mock<ITabsApi>()
                .Verify(x => x.Create(It.IsAny<CreateProperties>()), Times.Never);
        }
    }
}