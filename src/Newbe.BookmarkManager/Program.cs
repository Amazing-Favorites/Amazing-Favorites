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
                .AddSingleton<IUrlHashService, UrlHashService>()
                .AddSingleton<IBookmarkDataHolder, BookmarkDataHolder>()
                .AddTransient<IBkRepository, BkRepository>()
                .AddTransient<IUserOptionsRepository, UserOptionsRepository>()
                .AddSingleton<IBkDataHolder, BkDataHolder>()
                .AddSingleton<ISyncBookmarkJob, SyncBookmarkJob>()
                .AddSingleton<ISyncAliasJob, SyncAliasJob>()
                .AddTransient<ITextAliasProvider, PinyinTextAliasProvider>()
                .AddTransient<ICloudService, CloudService>()
                .AddSingleton<ISyncCloudJob, SyncCloudJob>();
            builder.Services
                .AddTransient<AuthHeaderHandler>()
                ;
            builder.Services
                .AddRefitClient<IPinyinApi>()
                .ConfigureHttpClient((sp, client) =>
                {
                    var service = sp.GetRequiredService<IOptions<UserOptions>>().Value;
                    client.BaseAddress = new Uri(service?.PinyinFeature?.BaseUrl ??
                                                 sp.GetRequiredService<IOptions<BaseUriOptions>>().Value.PinyinApi);
                })
                .AddHttpMessageHandler<AuthHeaderHandler>()
                ;

            builder.Services
                .AddRefitClient<ICloudBkApi>()
                .ConfigureHttpClient((sp, client) =>
                {
                    var service = sp.GetRequiredService<IOptions<UserOptions>>().Value;
                    client.BaseAddress = new Uri(service?.CloudBkFeature?.BaseUrl ??
                                                 sp.GetRequiredService<IOptions<BaseUriOptions>>().Value.CloudBkApi);
                })
                .AddHttpMessageHandler<AuthHeaderHandler>()
                ;

            await builder.Build().RunAsync();
        }
    }
}