using System;
using System.Threading.Tasks;
using Excubo.Blazor.ScriptInjection;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.JSInterop;

namespace Newbe.BookmarkManager.Components
{
    /// <summary>
    /// Copy from https://github.com/excubo-ag/Blazor.ScriptInjection/blob/main/ScriptInjection/AddScript.cs
    /// but change path of src
    /// </summary>
    public class AfAddScript : ComponentBase
    {
        private string src;

        [Inject] private IScriptInjectionTracker ScriptInjectionTracker { get; set; }

        [Parameter]
        public string Src
        {
            get => this.src;
            set => this.src = Uri.IsWellFormedUriString(value, UriKind.RelativeOrAbsolute)
                ? value
                : throw new ArgumentException("Invalid URI");
        }

        [Parameter] public bool Async { get; set; }

        [Parameter] public bool Defer { get; set; }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (string.IsNullOrEmpty(this.Src) || !Uri.IsWellFormedUriString(this.Src, UriKind.RelativeOrAbsolute))
                return;
            if (this.ScriptInjectionTracker.OnloadNotification && this.ScriptInjectionTracker.NeedsInjection("self"))
            {
                string str = this.ScriptInjectionTracker.GzippedBootstrap ? "bootstrap.min.js.gz" : "bootstrap.min.js";
                builder.OpenElement(0, "script");
                builder.AddAttribute(1, "src", "content/Excubo.Blazor.ScriptInjection/" + str);
                builder.AddAttribute(2, "type", "text/javascript");
                builder.CloseElement();
            }

            if (!this.ScriptInjectionTracker.NeedsInjection(this.Src))
                return;
            builder.OpenElement(3, "script");
            builder.AddAttribute(4, "src", this.Src);
            if (this.Async)
                builder.AddAttribute(5, "async", true);
            if (this.Defer)
                builder.AddAttribute(6, "defer", true);
            builder.AddAttribute(7, "type", "text/javascript");
            if (this.ScriptInjectionTracker.OnloadNotification)
                builder.AddAttribute(8, "onload", "window.Excubo.ScriptInjection.Notify('" + this.Src + "')");
            builder.CloseElement();
        }

        [Inject] private IJSRuntime js { get; set; }

        protected override async Task OnAfterRenderAsync(bool firstRender)
        {
            if (firstRender && this.ScriptInjectionTracker.OnloadNotification &&
                !this.ScriptInjectionTracker.Initialized)
            {
                this.ScriptInjectionTracker.Initialized = true;
                while (true)
                {
                    if (!await this.js.InvokeAsync<bool>("window.hasOwnProperty", (object)"Excubo"))
                        await Task.Delay(10);
                    else
                        break;
                }

                while (true)
                {
                    if (!await this.js.InvokeAsync<bool>("Excubo.hasOwnProperty", (object)"ScriptInjection"))
                        await Task.Delay(10);
                    else
                        break;
                }
            }

            await this.js.InvokeVoidAsync("Excubo.ScriptInjection.Register",
                (object)DotNetObjectReference.Create<IScriptInjectionTracker>(this.ScriptInjectionTracker));
            await base.OnAfterRenderAsync(firstRender);
        }
    }
}