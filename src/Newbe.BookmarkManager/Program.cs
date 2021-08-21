using System;
using System.Net.Http;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Autofac.Extras.DynamicProxy;
using BlazorApplicationInsights;
using Microsoft.AspNetCore.Components.WebAssembly.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Newbe.BookmarkManager.Components;
using Newbe.BookmarkManager.Services;
using Newbe.BookmarkManager.Services.Ai;
using Newbe.BookmarkManager.Services.Configuration;
using Newbe.BookmarkManager.Services.EventHubs;
using Newbe.BookmarkManager.Services.SimpleData;
using Refit;
using TG.Blazor.IndexedDB;
using WebExtensions.Net.Bookmarks;
using WebExtensions.Net.Identity;
using WebExtensions.Net.Runtime;
using WebExtensions.Net.Storage;
using WebExtensions.Net.Tabs;
using WebExtensions.Net.Windows;
using Module = Autofac.Module;

namespace Newbe.BookmarkManager
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var builder = WebAssemblyHostBuilder.CreateDefault(args);
            builder.ConfigureContainer(new AutofacServiceProviderFactory(Register));
            builder.RootComponents.Add<App>("#app");

            builder.Services.AddBlazorApplicationInsights(addILoggerProvider: false)
                .AddSingleton<ApplicationInsights>()
                .AddSingleton<EmptyApplicationInsights>()
                .AddTransient<IApplicationInsights>(provider =>
                {
                    if (ApplicationInsightAop.Enabled)
                    {
                        return provider.GetRequiredService<ApplicationInsights>();
                    }

                    return provider.GetRequiredService<EmptyApplicationInsights>();
                })
                .AddSingleton<ILoggerProvider, AiLoggerProvider>();
            builder.Services.AddScoped(
                    sp => new HttpClient { BaseAddress = new Uri(builder.HostEnvironment.BaseAddress) })
                .Configure<BaseUriOptions>(builder.Configuration.GetSection(nameof(BaseUriOptions)))
                .Configure<DevOptions>(builder.Configuration.GetSection(nameof(DevOptions)))
                .Configure<GoogleDriveOAuthOptions>(builder.Configuration.GetSection(nameof(GoogleDriveOAuthOptions)))
                .Configure<StaticUrlOptions>(builder.Configuration.GetSection(nameof(StaticUrlOptions)));
            builder.Services
                .AddSingleton(typeof(IIndexedDbRepo<,>), typeof(IndexedDbRepo<,>));
            builder.Services
                .AddAntDesign()
                .AddBrowserExtensionServices(options => { options.ProjectNamespace = typeof(Program).Namespace; })
                .AddTransient<IBookmarksApi, BookmarksApi>()
                .AddTransient<ITabsApi, TabsApi>()
                .AddTransient<IWindowsApi, WindowsApi>()
                .AddTransient<IStorageApi, StorageApi>()
                .AddTransient<IIdentityApi, IdentityApi>()
                .AddTransient<IRuntimeApi, RuntimeApi>()
                .AddTransient<IManagePageNotificationService, ManagePageNotificationService>()
                .AddTransient<IClock, SystemClock>()
                .AddTransient<ITagsManager, TagsManager>()
                .AddSingleton<IRecentSearchHolder, RecentSearchHolder>()
                .AddSingleton<IUrlHashService, UrlHashService>()
                .AddSingleton<IAfCodeService, AfCodeService>()
                .AddSingleton<IRecordService, RecordService>()
                .AddSingleton<ISyncBookmarkJob, SyncBookmarkJob>()
                .AddSingleton<ISyncAliasJob, SyncAliasJob>()
                .AddSingleton<ISyncTagRelatedBkCountJob, SyncTagRelatedBkCountJob>()
                .AddTransient<ITextAliasProvider, PinyinTextAliasProvider>()
                .AddSingleton<ISyncCloudJob, SyncCloudJob>()
                .AddSingleton<IShowWhatNewJob, ShowWhatNewJob>()
                .AddSingleton<IShowWelcomeJob, ShowWelcomeJob>()
                .AddSingleton<IDataFixJob, DataFixJob>();

            builder.Services
                .AddTransient<IBkEditFormData, BkEditFormData>();

            builder.Services
                .AddTransient<AuthHeaderHandler>();
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
                .AddHttpMessageHandler<AuthHeaderHandler>();

            builder.Services.AddIndexedDB(dbStore =>
            {
                dbStore.DbName = Consts.DbName;
                dbStore.Version = 5;

                dbStore.Stores.Add(new StoreSchema
                {
                    Name = Consts.StoreNames.Bks,
                    PrimaryKey = new IndexSpec { Name = "url", KeyPath = "url", Auto = false, Unique = true },
                });
                dbStore.Stores.Add(new StoreSchema
                {
                    Name = Consts.StoreNames.Tags,
                    PrimaryKey = new IndexSpec { Name = "tag", KeyPath = "tag", Auto = false, Unique = true },
                });
                dbStore.Stores.Add(new StoreSchema
                {
                    Name = Consts.StoreNames.BkMetadata,
                    PrimaryKey = new IndexSpec { Name = "id", KeyPath = "id", Auto = false, Unique = true },
                });
                dbStore.Stores.Add(new StoreSchema
                {
                    Name = Consts.StoreNames.UserOptions,
                    PrimaryKey = new IndexSpec { Name = "id", KeyPath = "id", Auto = false, Unique = true },
                });
                dbStore.Stores.Add(new StoreSchema
                {
                    Name = Consts.StoreNames.AfMetadata,
                    PrimaryKey = new IndexSpec { Name = "id", KeyPath = "id", Auto = false, Unique = true },
                });
                dbStore.Stores.Add(new StoreSchema
                {
                    Name = Consts.StoreNames.SearchRecord,
                    PrimaryKey = new IndexSpec { Name = "id", KeyPath = "id", Auto = false, Unique = true },
                });
                dbStore.Stores.Add(new StoreSchema
                {
                    Name = Consts.StoreNames.RecentSearch,
                    PrimaryKey = new IndexSpec { Name = "id", KeyPath = "id", Auto = false, Unique = true },
                });
                dbStore.Stores.Add(new StoreSchema
                {
                    Name = Consts.StoreNames.SimpleData,
                    PrimaryKey = new IndexSpec { Name = "id", KeyPath = "id", Auto = false, Unique = true },
                });
            });

            var webAssemblyHost = builder.Build();
            var userOptionsService = webAssemblyHost.Services.GetRequiredService<IUserOptionsService>();
            var userOptions = await userOptionsService.GetOptionsAsync();
            ApplicationInsightAop.Enabled = userOptions is
            {
                AcceptPrivacyAgreement: true,
                ApplicationInsightFeature:
                {
                    Enabled: true
                }
            };
            await webAssemblyHost.RunAsync();
        }

        private static void Register(ContainerBuilder builder)
        {
            builder.RegisterType<ApplicationInsightAop>();
            RegisterType<IndexedBkSearcher, IBkSearcher>();
            RegisterType<IndexedBkManager, IBkManager>();
            RegisterType<UserOptionsService, IUserOptionsService>();
            builder.RegisterModule<CloudServiceModule>();
            builder.RegisterModule<EventHubModule>();
            builder.RegisterModule<SimpleObjectStorageModule>();

            void RegisterType<TType, TInterface>()
            {
#pragma warning disable 8714
                builder.RegisterType<TType>()
                    .As<TInterface>()
#pragma warning restore 8714
                    .EnableInterfaceInterceptors()
                    .InterceptedBy(typeof(ApplicationInsightAop));
            }
        }

        private class CloudServiceModule : Module
        {
            protected override void Load(ContainerBuilder builder)
            {
                base.Load(builder);
                builder.RegisterType<CloudServiceFactory>()
                    .As<ICloudServiceFactory>();
                builder.RegisterType<NewbeApiCloudService>()
                    .Keyed<ICloudService>(CloudBkProviderType.NewbeApi)
                    .EnableInterfaceInterceptors()
                    .InterceptedBy(typeof(ApplicationInsightAop));
                builder.RegisterType<GoogleDriveCloudService>()
                    .Keyed<ICloudService>(CloudBkProviderType.GoogleDrive)
                    .SingleInstance()
                    .EnableInterfaceInterceptors()
                    .InterceptedBy(typeof(ApplicationInsightAop));
                builder.RegisterType<GoogleDriveClient>()
                    .As<IGoogleDriveClient>()
                    .SingleInstance();
            }
        }

        private class EventHubModule : Module
        {
            protected override void Load(ContainerBuilder builder)
            {
                base.Load(builder);
                builder.RegisterType<AfEventHub>()
                    .As<IAfEventHub>()
                    .SingleInstance();

                builder.RegisterType<FuncEventHandler>().AsSelf();
            }
        }

        private class SimpleObjectStorageModule : Module
        {
            protected override void Load(ContainerBuilder builder)
            {
                base.Load(builder);
                builder.RegisterType<SimpleDataStorage>()
                    .As<ISimpleDataStorage>()
                    .SingleInstance();
            }
        }
    }
}