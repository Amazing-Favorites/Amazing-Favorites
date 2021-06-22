(async () => {
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

    document.onkeydown = async function (event) {
        const e = event || window.event || arguments.callee.caller.arguments[0];
        await GLOBAL.DotNetReference.invokeMethodAsync('OnSearchInputKeydown', e.code, e.altKey);
    };

    document.onkeyup = async function (event) {
        const e = event || window.event || arguments.callee.caller.arguments[0];
        await GLOBAL.DotNetReference.invokeMethodAsync('OnSearchInputKeyup', e.code, e.altKey);
    };
})();