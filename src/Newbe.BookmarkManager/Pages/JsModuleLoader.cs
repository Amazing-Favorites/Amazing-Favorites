using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Newbe.BookmarkManager.Pages
{
    public class JsModuleLoader : IAsyncDisposable
    {
        private readonly IJSRuntime _jsRuntime;
        private readonly List<IJSObjectReference> _modules = new();

        public JsModuleLoader(
            IJSRuntime jsRuntime)
        {
            _jsRuntime = jsRuntime;
        }

        public async Task LoadAsync(string modulePath)
        {
            var module = await _jsRuntime.InvokeAsync<IJSObjectReference>("import",
                modulePath);
            _modules.Add(module);
        }

        public async ValueTask DisposeAsync()
        {
            foreach (var jsObjectReference in _modules)
            {
                await jsObjectReference.DisposeAsync();
            }
        }
    }
}