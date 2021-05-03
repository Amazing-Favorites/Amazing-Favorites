using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newbe.BookmarkManager.Services;
using Newbe.BookmarkManager.Services.Configuration;
using Refit;
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
                .Configure<BaseUriOptions>(builder.Configuration.GetSection(nameof(BaseUriOptions)))
                .Configure<AliasJobOptions>(builder.Configuration.GetSection(nameof(AliasJobOptions)))
                ;
            builder.Services
                .AddAntDesign()
                .AddBrowserExtensionServices(options => { options.ProjectNamespace = typeof(Program).Namespace; })
                .AddTransient<IBookmarksApi, BookmarksApi>()
                .AddTransient<ITabsApi, TabsApi>()
                .AddTransient<IWindowsApi, WindowsApi>()
                .AddTransient<IStorageApi, StorageApi>()
                .AddTransient<IClock, SystemClock>()
                .AddTransient<IBkManager, BkManager>()
                .AddTransient<IBkSearcher, BkSearcher>()
                .AddSingleton<IBookmarkDataHolder, BookmarkDataHolder>()
                .AddTransient<IBkRepository, BkRepository>()
                .AddSingleton<IBkDataHolder, BkDataHolder>()
                .AddSingleton<ISyncBookmarkJob, SyncBookmarkJob>()
                .AddSingleton<ISyncAliasJob, SyncAliasJob>()
                .AddTransient<ITextAliasProvider, PinyinTextAliasProvider>();
            builder.Services
                .AddRefitClient<IPinyinApi>()
                .ConfigureHttpClient((sp, client) =>
                {
                    client.BaseAddress = new Uri(sp.GetRequiredService<IOptions<BaseUriOptions>>().Value.PinyinApi);
                })
                ;

            await builder.Build().RunAsync();
        }
    }
}