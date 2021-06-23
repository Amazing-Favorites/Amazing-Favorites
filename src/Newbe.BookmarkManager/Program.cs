using System;
using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using Newbe.BookmarkManager.Pages;
using Newbe.BookmarkManager.Services;
using Newbe.BookmarkManager.Services.Configuration;
using Refit;
using TG.Blazor.IndexedDB;
using WebExtensions.Net.Bookmarks;
using WebExtensions.Net.Storage;
using WebExtensions.Net.Tabs;
using WebExtensions.Net.Windows;

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
                .AddSingleton(typeof(IIndexedDbRepo<,>), typeof(IndexedDbRepo<,>))
                ;
            builder.Services
                .AddAntDesign()
                .AddBrowserExtensionServices(options => { options.ProjectNamespace = typeof(Program).Namespace; })
                .AddTransient<IBookmarksApi, BookmarksApi>()
                .AddTransient<ITabsApi, TabsApi>()
                .AddTransient<IWindowsApi, WindowsApi>()
                .AddTransient<IStorageApi, StorageApi>()
                .AddTransient<IClock, SystemClock>()
                .AddTransient<IBkManager, IndexedBkManager>()
                .AddTransient<ITagsManager, TagsManager>()
                .AddTransient<IBkSearcher, IndexedBkSearcher>()
                .AddSingleton<IUrlHashService, UrlHashService>()
                .AddTransient<IUserOptionsService, UserOptionsService>()
                .AddSingleton<ISyncBookmarkJob, SyncBookmarkJob>()
                .AddSingleton<ISyncAliasJob, SyncAliasJob>()
                .AddSingleton<ISyncTagRelatedBkCountJob, SyncTagRelatedBkCountJob>()
                .AddTransient<ITextAliasProvider, PinyinTextAliasProvider>()
                .AddTransient<ICloudService, CloudService>()
                .AddSingleton<ISyncCloudJob, SyncCloudJob>();

            builder.Services
                .AddTransient<IBkEditFormData, BkEditFormData>()
                ;

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

            builder.Services.AddIndexedDB(dbStore =>
            {
                dbStore.DbName = Consts.DbName;
                dbStore.Version = 1;

                dbStore.Stores.Add(new StoreSchema
                {
                    Name = Consts.StoreNames.Bks,
                    PrimaryKey = new IndexSpec {Name = "url", KeyPath = "url", Auto = false, Unique = true},
                });
                dbStore.Stores.Add(new StoreSchema
                {
                    Name = Consts.StoreNames.Tags,
                    PrimaryKey = new IndexSpec {Name = "tag", KeyPath = "tag", Auto = false, Unique = true},
                });
                dbStore.Stores.Add(new StoreSchema
                {
                    Name = Consts.StoreNames.BkMetadata,
                    PrimaryKey = new IndexSpec {Name = "id", KeyPath = "id", Auto = false, Unique = true},
                });
                dbStore.Stores.Add(new StoreSchema
                {
                    Name = Consts.StoreNames.UserOptions,
                    PrimaryKey = new IndexSpec {Name = "id", KeyPath = "id", Auto = false, Unique = true},
                });
            });

            await builder.Build().RunAsync();
        }
    }
}