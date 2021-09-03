const global=globalThis;
(async()=>
{
    window.DotNet.SetDotnetReference = function (pDotNetReference) {
        console.log(`test _dotnet set: ${JSON.stringify(pDotNetReference)}`)
        window.DotNet.DotNetReference = pDotNetReference;
    };
})();
export async function addListenerTabsOnUpdated() {
    
    chrome.tabs.onUpdated.addListener(async (tabId,changeInfo,tab)=>{
        console.log(`changeInfo:${changeInfo.url}`);
        console.log(`test1:${JSON.stringify(global.DotNet.DotNetReference)}`);
        if(changeInfo.url){
            await global.DotNet.DotNetReference.invokeMethodAsync('OpenNewTab',tabId,changeInfo.url);
        }
    });
}