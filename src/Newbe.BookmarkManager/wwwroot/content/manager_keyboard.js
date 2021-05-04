(async () => {
    document.addEventListener('DOMContentLoaded', function () {
        const global = globalThis;
        global.GLOBAL = {};
        GLOBAL.DotNetReference = null;
        GLOBAL.SetDotnetReference = function (pDotNetReference) {
            console.log(`dotnet set: ${pDotNetReference}`)
            GLOBAL.DotNetReference = pDotNetReference;
        };
        setTimeout(() => {
            global.browser.commands.onCommand.addListener(async (c) => {
                console.log(`received command: ${c}`);
                await GLOBAL.DotNetReference.invokeMethodAsync('OnReceivedCommand', c);
            });
        }, 5000);
    });
})();