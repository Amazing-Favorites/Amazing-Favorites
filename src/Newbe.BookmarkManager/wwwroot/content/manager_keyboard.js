(async () => {
    const global = globalThis;
    window.DotNet.SetDotnetReference = function (pDotNetReference) {
        console.log(`dotnet set: ${pDotNetReference}`)
        window.DotNet.DotNetReference = pDotNetReference;
    };

    document.onkeydown = async function (event) {
        const e = event || window.event || arguments.callee.caller.arguments[0];
        await global.DotNet.DotNetReference.invokeMethodAsync('OnSearchInputKeydown', e.code, e.altKey);
    };

    document.onkeyup = async function (event) {
        const e = event || window.event || arguments.callee.caller.arguments[0];
        await global.DotNet.DotNetReference.invokeMethodAsync('OnSearchInputKeyup', e.code, e.altKey);
    };
    console.info("keyboard event loaded");
})();