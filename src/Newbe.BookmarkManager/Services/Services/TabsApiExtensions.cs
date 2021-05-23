using System.Linq;
using System.Threading.Tasks;
using WebExtension.Net.Tabs;

namespace Newbe.BookmarkManager.Services
{
    public static class TabsApiExtensions
    {
        public static async Task ActiveOrOpenManagerAsync(this ITabsApi tabsApi)
        {
            var tabs = await tabsApi.Query(new QueryInfo
            {
                Title = Consts.ManagerTabTitle
            });
            var managerTab = tabs.FirstOrDefault();
            if (managerTab is {Id: { }})
            {
                await tabsApi.Update(managerTab.Id.Value, new UpdateProperties
                {
                    Active = true
                });
            }
            else
            {
                await tabsApi.Create(new CreateProperties
                {
                    Url = "/Manager/index.html"
                });
            }
        }
    }
}