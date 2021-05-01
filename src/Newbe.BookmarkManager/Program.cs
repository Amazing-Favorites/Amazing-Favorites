using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Newbe.BookmarkManager.Services;
using WebExtension.Net.Bookmarks;
using WebExtension.Net.Storage;
using WebExtension.Net.Tabs;
using WebExtension.Net.Windows;

namespace Newbe.BookmarkManager
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddScoped(
                    sp => new HttpClient {BaseAddress = new Uri(builder.HostEnvironment.BaseAddress)})
                .AddAntDesign()
                .AddBrowserExtensionServices(options => { options.ProjectNamespace = typeof(Program).Namespace; })
                .AddTransient<IBookmarksApi, BookmarksApi>()
                .AddTransient<ITabsApi, TabsApi>()
                .AddTransient<IWindowsApi, WindowsApi>()
                .AddTransient<IBkManager, BkManager>()
                .AddTransient<IBkSearcher, BkSearcher>()
                .AddSingleton<IBookmarkDataHolder, BookmarkDataHolder>()
                .AddTransient<IClock, SystemClock>()
                .AddTransient<IBkRepository, BkRepository>()
                .AddSingleton<IBkDataHolder, BkDataHolder>()
                .AddSingleton<ISyncBookmarkJob, SyncBookmarkJob>()
                .AddTransient<IStorageApi, StorageApi>()
                ;

            await builder.Build().RunAsync();
        }
    }
}