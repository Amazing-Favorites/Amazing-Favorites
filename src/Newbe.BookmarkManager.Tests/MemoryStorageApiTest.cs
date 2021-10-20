using System.Collections.Generic;
using System.Text.Json;
using System.Threading.Tasks;
using FluentAssertions;
using Newbe.BookmarkManager.Services.MessageBus;
using NUnit.Framework;

namespace Newbe.BookmarkManager.Tests
{
    public class MemoryStorageApiTest
    {
        [Test]
        public async Task RunAsync()
        {
            var options = new BusOptions
            {
                EnvelopName = "afEvent"
            };
            var memoryStorageApi = new MemoryStorageApi(options);
            JsonElement? data = null;
            await memoryStorageApi.RegisterCallBack((changes, area) => { data = changes; });
            await memoryStorageApi.SetLocal(new Dictionary<string, object>
            {
                {
                    options.EnvelopName, new TestData
                    {
                        Name = "nice"
                    }
                }
            });
            data.Should().NotBeNull();
            JsonSerializer.Serialize(data).Should().Be("{\"afEvent\":{\"newValue\":{\"Name\":\"nice\"}}}");
        }

        private record TestData
        {
            public string? Name { get; set; }
        }
    }

    public class MemoryStorageApi : IStorageApiWrapper
    {
        private readonly BusOptions _busOptions;

        public MemoryStorageApi(
            BusOptions busOptions)
        {
            _busOptions = busOptions;
        }

        private readonly List<StorageChangeCallback> _callbacks = new();

        public ValueTask RegisterCallBack(StorageChangeCallback callback)
        {
            _callbacks.Add(callback);
            return ValueTask.CompletedTask;
        }

        public ValueTask SetLocal(object value)
        {
            var source = JsonSerializer.SerializeToElement(value);
            var newValue = source.GetProperty(_busOptions.EnvelopName);
            foreach (var storageChangeCallback in _callbacks)
            {
                var changesObject = new Dictionary<string, object>
                {
                    {
                        _busOptions.EnvelopName, new
                        {
                            newValue = newValue
                        }
                    }
                };
                var changes = JsonSerializer.SerializeToElement(changesObject);
                storageChangeCallback.Invoke(changes, "local");
            }

            return ValueTask.CompletedTask;
        }
    }
}