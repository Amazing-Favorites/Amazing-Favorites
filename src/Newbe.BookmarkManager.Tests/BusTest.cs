using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extras.Moq;
using FluentAssertions;
using Newbe.BookmarkManager.Services.MessageBus;
using NUnit.Framework;

namespace Newbe.BookmarkManager.Tests
{
    public class BusTest
    {
        [Test]
        public async Task NormalAsync()
        {
            var options = new BusOptions
            {
                EnvelopName = "afEvent"
            };
            var api = new MemoryStorageApi(options);
            using var mocker = AutoMock.GetLoose(builder =>
            {
                builder.RegisterInstance(api)
                    .AsImplementedInterfaces();
            });
            var factory = mocker.Create<Bus.Factory>();
            var channel = factory.Invoke(options);
            await channel.EnsureStartAsync();
            channel.RegisterHandler<TestRequest>((scope, payload, message) =>
            {
                channel.SendResponse(new TestResponse
                {
                    Greetings = GetGreetings(payload.Name)
                }, message);
            });
            var name = "newbe36524";
            var testResponse = await channel.SendRequest<TestRequest, TestResponse>(new TestRequest
            {
                Name = name
            });
            testResponse.Should().NotBeNull();
            testResponse.Greetings.Should().Be(GetGreetings(name));

            channel.MessageHandlerCollection.HandlerItems.Count.Should().Be(1);

            string GetGreetings(string n)
            {
                return $"hi, {n}";
            }
        }

        [Test]
        public async Task ConcurrentAsync()
        {
            var options = new BusOptions
            {
                EnvelopName = "afEvent"
            };
            var api = new MemoryStorageApi(options);
            using var mocker = AutoMock.GetLoose(builder =>
            {
                builder.RegisterInstance(api)
                    .AsImplementedInterfaces();
            });
            var factory = mocker.Create<Bus.Factory>();
            var channel = factory.Invoke(options);
            await channel.EnsureStartAsync();

            channel.RegisterHandler<TestRequest>((scope, payload, message) =>
            {
                channel.SendResponse(new TestResponse
                {
                    Greetings = GetGreetings(payload.Name)
                }, message);
            });

            const int maxCount = 100;
            // act
            var tasks = Enumerable.Range(0, maxCount)
                .Select(x => channel.SendRequest<TestRequest, TestResponse>(new TestRequest
                {
                    Name = x.ToString()
                }));
            var results = await Task.WhenAll(tasks);
            var expectation = Enumerable.Range(0, maxCount)
                .Select(x => GetGreetings(x.ToString())).OrderBy(x => x);
            results.Select(x => x.Greetings).OrderBy(x => x)
                .Should().BeEquivalentTo(expectation);

            channel.MessageHandlerCollection.HandlerItems.Count.Should().Be(1);

            string GetGreetings(string n)
            {
                return $"hi, {n}";
            }
        }

        record TestRequest : IRequest, IMessage
        {
            public string Name { get; set; }
        }

        record TestResponse : IResponse, IMessage
        {
            public string Greetings { get; set; }
        }
    }
}