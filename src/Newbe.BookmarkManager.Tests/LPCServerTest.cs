using System.Threading.Tasks;
using Autofac;
using Autofac.Extras.Moq;
using FluentAssertions;
using Moq;
using Newbe.BookmarkManager.Pages.Test;
using Newbe.BookmarkManager.Services.LPC;
using Newbe.BookmarkManager.Services.MessageBus;
using NUnit.Framework;

namespace Newbe.BookmarkManager.Tests
{
    public class LPCServerTest
    {
        [Test]
        public void AddServer()
        {
            using var mocker = AutoMock.GetLoose(builder =>
            {
                builder.RegisterType<BusFactory>()
                    .AsImplementedInterfaces()
                    .SingleInstance();
                builder.RegisterType<Bus>()
                    .AsSelf();
            });
            var server = mocker.Create<LPCServer>();
            var instance = new TestServerInstance();
            server.AddServerInstance(instance);
        }

        [Test]
        public async Task InvokeAsync()
        {
            var options = new BusOptions
            {
                EnvelopName = "test_lpc"
            };
            var api = new MemoryStorageApi(options);
            using var mocker = AutoMock.GetLoose(builder =>
            {
                builder.RegisterInstance(api)
                    .AsImplementedInterfaces();
            });
            var factory = mocker.Create<Bus.Factory>();
            var bus = factory.Invoke(options);

            mocker.Mock<IBusFactory>()
                .Setup(x => x.Create(It.IsAny<BusOptions>()))
                .Returns(bus);

            // act
            var server = mocker.Create<LPCServer>();
            var instance = new TestServerInstance();
            server.AddServerInstance(instance);
            await server.StartAsync();

            var request = new BusTestPage.TestRequest
            {
                Name = "newbe36524"
            };
            var response = await bus.SendRequest<BusTestPage.TestRequest, BusTestPage.TestResponse>(request);
            response.Greetings.Should().Be($"hi, {request.Name}");
        }

        [Test]
        [TestCase(nameof(ITestServerInstance2.Ok1))]
        [TestCase(nameof(ITestServerInstance2.No1))]
        [TestCase(nameof(ITestServerInstance2.No2))]
        [TestCase(nameof(ITestServerInstance2.No3))]
        [TestCase(nameof(ITestServerInstance2.No4))]
        [TestCase(nameof(ITestServerInstance2.No5))]
        public void IsRemoteMethod(string name)
        {
            var isOk = name.Contains("Ok");
            var re = LPCServerExtensions.IsRemoteMethod(typeof(ITestServerInstance2).GetMethod(name)!);
            re.Should().Be(isOk);
        }
    }

    public interface ITestServerInstance2
    {
        Task<BusTestPage.TestResponse> Ok1(BusTestPage.TestRequest request);
        BusTestPage.TestResponse No1(BusTestPage.TestRequest request);
        BusTestPage.TestResponse No2();
        Task<BusTestPage.TestResponse> No3(int a);
        Task<BusTestPage.TestResponse> No4(BusTestPage.TestRequest request, int a);
        Task<(BusTestPage.TestResponse, int)> No5(BusTestPage.TestRequest request, int a);
    }

    public interface ITestServerInstance
    {
        Task<BusTestPage.TestResponse> GoAsync(BusTestPage.TestRequest request);
    }

    public class TestServerInstance : ITestServerInstance
    {
        public Task<BusTestPage.TestResponse> GoAsync(BusTestPage.TestRequest request)
        {
            return Task.FromResult(new BusTestPage.TestResponse
            {
                Greetings = $"hi, {request.Name}"
            });
        }
    }
}