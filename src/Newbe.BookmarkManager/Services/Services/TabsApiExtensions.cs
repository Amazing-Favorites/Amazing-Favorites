using System.Linq;
using System.Threading.Tasks;
using WebExtensions.Net.Tabs;

namespace Newbe.BookmarkManager.Services
{
    public static class TabsApiExtensions
    {
        public static async Task ActiveOrOpenManagerAsync(this ITabsApi tabsApi)
        {
            var tabs = await tabsApi.Query(new QueryInfo
            {
                Title = Consts.ManagerTabTitle,
            });
            var managerTab = tabs.FirstOrDefault();
            if (managerTab is { Id: { } })
            {
                await tabsApi.Update(managerTab.Id.Value, new UpdateProperties
                {
                    Active = true
                });
            }
            else
            {
                await tabsApi.OpenAsync("/Manager/index.html");
            }
        }

        public static async Task ActiveOrOpenAsync(this ITabsApi tabsApi,
            string url)
        {
            var tabs = await tabsApi.Query(new QueryInfo
            {
                Url = url
            });
            var managerTab = tabs.FirstOrDefault();
            if (managerTab is { Id: { } })
            {
                await tabsApi.Update(managerTab.Id.Value, new UpdateProperties
                {
                    Active = true
                });
            }
            else
            {
                await tabsApi.OpenAsync(url);
            }
        }

        public static async Task OpenAsync(this ITabsApi tabsApi, string url)
        {
            await tabsApi.Create(new CreateProperties
            {
                Url = url
            });
        }
    }
}