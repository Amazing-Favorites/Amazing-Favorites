using System;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extras.Moq;
using FluentAssertions;
using Microsoft.Extensions.Options;
using Moq;
using Newbe.BookmarkManager.Services;
using Newbe.BookmarkManager.Services.Configuration;
using Newbe.BookmarkManager.Services.LPC;
using Newbe.BookmarkManager.Services.MessageBus;
using NUnit.Framework;

namespace Newbe.BookmarkManager.Tests;

public class InviteUserCommentsJobTest
{
    [SetUp]
    public void Setup()
    {
    }
    [Test]

    public async Task InvitationTimeAfter14Days()
    {
        using var mocker = AutoMock.GetLoose();
        mocker.Mock<INewNotification>()
            .Setup(x => x.InviteUserCommentsAsync())
            .Returns(Task.CompletedTask)
            .Verifiable();
        
        mocker.Mock<IUserOptionsService>()
            .Setup(x=> x.GetOptionsAsync())
            .ReturnsAsync(new UserOptions
            {
                InvitationTime = DateTime.Now.Date.AddDays(-Consts.InviteUserCommentsCdDays).AddDays(-1)
            });
        // act
        var job = mocker.Create<InviteUserCommentsJob>();
        await job.StartAsync();
    }
    [Test]
    public async Task InvitationTimeBefore14Days()
    {
        using var mocker = AutoMock.GetLoose();
        mocker.Mock<INewNotification>()
            .Verify(x => x.InviteUserCommentsAsync(),Times.Never);
        mocker.Mock<IUserOptionsService>()
            .Setup(x=> x.GetOptionsAsync())
            .ReturnsAsync(new UserOptions
            {
                InvitationTime = DateTime.Now.Date.AddDays(-Consts.InviteUserCommentsCdDays).AddDays(1)
            });
        // act
        var job = mocker.Create<InviteUserCommentsJob>();
        await job.StartAsync();
    }
    
}