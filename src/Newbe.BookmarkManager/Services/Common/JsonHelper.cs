using System;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.Toolkit.HighPerformance;

namespace Newbe.BookmarkManager.Services
{
    public static class JsonHelper
    {
        public static async Task<T?> DeserializeAsync<T>(string source)
        {
            if (string.IsNullOrEmpty(source))
            {
                return default;
            }

            var re = await JsonSerializer.DeserializeAsync<T>(
                Encoding.UTF8.GetBytes(source)
                    .AsMemory()
                    .AsStream());
            return re;
        }

        public static async Task<object?> DeserializeAsync(string source, Type type)
        {
            if (string.IsNullOrEmpty(source))
            {
                return default;
            }

            var re = await JsonSerializer.DeserializeAsync(
                Encoding.UTF8.GetBytes(source)
                    .AsMemory()
                    .AsStream(), type);
            return re;
        }
    }
}