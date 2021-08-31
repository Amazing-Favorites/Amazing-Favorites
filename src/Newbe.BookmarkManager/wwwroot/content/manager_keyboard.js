(async () => {
    const global = globalThis;
    window.DotNet.SetDotnetReference = function (pDotNetReference) {
        console.log(`dotnet set: ${pDotNetReference}`)
        window.DotNet.DotNetReference = pDotNetReference;
    };
    global.browser.commands.onCommand.addListener(async (c) => {
        console.log(`received command: ${c}`);
        await global.DotNet.DotNetReference.invokeMethodAsync('OnReceivedCommand', c);
    });

    document.onkeydown = async function (event) {
        const e = event || window.event || arguments.callee.caller.arguments[0];
        await global.DotNet.DotNetReference.invokeMethodAsync('OnSearchInputKeydown', e.code, e.altKey);
    };

    document.onkeyup = async function (event) {
        const e = event || window.event || arguments.callee.caller.arguments[0];
        await global.DotNet.DotNetReference.invokeMethodAsync('OnSearchInputKeyup', e.code, e.altKey);
    };
    console.info("keyboard event loaded");
    chrome.tabs.onCreated.addListener(async (tab)=>{
        console.log(`tabs onCreated id:${tab.id},bookmarkInfo.url:${tab.url}`);
        console.log(`pendingUrl:${tab.pendingUrl}`);
        console.log(`title:${tab.title}`);
        //await global.DotNet.DotNetReference.invokeMethodAsync('OpenNewTab',tab.id,tab.url);
    });
    chrome.tabs.onUpdated.addListener(async (tabId,changeInfo,tab)=>{
        console.log(`tabs onChanged id:${tabId},url:${changeInfo.url}`);
        console.log(`title:${changeInfo.title}`);
        //await global.DotNet.DotNetReference.invokeMethodAsync('OpenNewTab',tab.id,tab.url);
    });
})();