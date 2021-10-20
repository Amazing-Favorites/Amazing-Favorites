using System;
using Autofac;
using Autofac.Extras.Moq;
using FluentAssertions;
using Newbe.BookmarkManager.Services.MessageBus;
using NUnit.Framework;

namespace Newbe.BookmarkManager.Tests
{
    public class HandlersTest
    {
        [Test]
        public void AddHandler()
        {
            using var mocker = AutoMock.GetLoose();
            var handlers = mocker.Create<MessageHandlerCollection>();

            // act
            handlers.AddHandler(message => true,
                (scope, message) => false);
            handlers.HandlerItems.Count.Should().Be(1);
        }

        [Test]
        [TestCase(false, true, true, 0)]
        [TestCase(false, true, false, 1)]
        [TestCase(false, false, true, 1)]
        [TestCase(false, false, false, 1)]
        [TestCase(true, true, true, 0)]
        [TestCase(true, true, false, 0)]
        [TestCase(true, false, true, 0)]
        [TestCase(true, false, false, 0)]
        public void RemoveHandlerIfHandleSuccess(bool handlerExpired, bool filterSuccess, bool done, int leftCount)
        {
            var clock = new StaticClock
            {
                UtcNow = DateTimeOffset.Now.ToUnixTimeSeconds()
            };

            using var mocker = AutoMock.GetLoose(builder =>
            {
                builder.RegisterInstance(clock)
                    .AsImplementedInterfaces();
            });
            var handlers = mocker.Create<MessageHandlerCollection>();
            long expiredAt;
            if (handlerExpired)
            {
                expiredAt = clock.UtcNow + Bus.DefaultExpiredDuration;
            }
            else
            {
                expiredAt = default;
            }

            handlers.AddHandler(message => filterSuccess,
                (scope, message) => done,
                expiredAt);

            if (handlerExpired)
            {
                clock.UtcNow = expiredAt + 1;
            }

            // act
            handlers.Handle(new BusMessage(), default!);
            handlers.HandlerItems.Count.Should().Be(leftCount);
        }
    }
}