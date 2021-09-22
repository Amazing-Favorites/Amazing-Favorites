(async () => {
    const global = globalThis;
    window.DotNet.SetDotnetReference = function (pDotNetReference) {
        console.log(`dotnet set: ${pDotNetReference}`)
        window.DotNet.DotNetReference = pDotNetReference;
    };
    chrome.runtime.onMessage.addListener( async
        (request, sender, sendResponse)=>{
            await global.DotNet.DotNetReference.invokeMethodAsync('OnMessage', request, sender,sendResponse);
        }
    )
    console.info("mediator loaded");
})();

// export async function sendMessage(message){
//     let res;
//     chrome.runtime.sendMessage(message, function(response) {
//         res = response;
//     });
//    
//     return res;
// }