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
    console.info("keyboard event loaded");
})();