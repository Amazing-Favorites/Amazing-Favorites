(async()=>{

    browser.omnibox.onInputChanged.addListener(async (input,suggest)=>{

        var result = await global.DotNet.DotNetReference.invokeMethodAsync("GetOnimiBoxSuggest",input);
        suggest(result);
    });
    browser.omnibox.onInputEntered.addListener(async (url, disposition) => {
        var result = await global.DotNet.DotNetReference.invokeMethodAsync("CheckIsUrl",url);
        if(!result)
        {
            const managerTabTitle = "Amazing Favorites";
            const managerTabs = await browser.tabs.query({
                title: managerTabTitle
            });
            if (managerTabs.length > 0) {
                await browser.tabs.update(managerTabs[0].id, {
                    active: true
                });
            } else {
                await browser.tabs.create({
                    //url: "/Manager/index.html?editTabId=" + tab.id
                    url: "/Manager/index.html"
                });
            }
            return;
        }
        switch (disposition) {
            case "currentTab":
                browser.tabs.update({url});
                break;
            case "newForegroundTab":
                browser.tabs.create({url});
                break;
            case "newBackgroundTab":
                browser.tabs.create({url, active: false});
                break;
        }
    });
    console.info("ominiboxSuggest loaded");
})