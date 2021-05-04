﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace WebExtension.Net.Bookmarks
{
  /// <summary>
    /// Invoke JavaScript object options.
    /// </summary>
    public class InvokeObjectReferenceOption : InvokeOption
    {
        internal InvokeObjectReferenceOption(string referenceId) : this(referenceId, null) { }

        internal InvokeObjectReferenceOption(string referenceId, string targetPath) : this(referenceId, targetPath, false) { }

        internal InvokeObjectReferenceOption(string referenceId, string targetPath, bool isFunction)
        {
            ReferenceId = referenceId;
            TargetPath = targetPath;
            IsFunction = isFunction;
        }

        /// <summary>
        /// Reference ID of the JavaScript object.
        /// </summary>
        public string ReferenceId { get; set; }

        /// <summary>
        /// The target path of the JavaScript object.
        /// </summary>
        public string TargetPath { get; set; }

        /// <summary>
        /// The indicator if the target is a function.
        /// </summary>
        public bool IsFunction { get; set; }
    }
    
     /// <summary>
    /// Base object returned from JavaScript.
    /// </summary>
    public class BaseObject : IDisposable
    {
        internal bool IsInitialized;
        private WebExtensionJSRuntime webExtensionJSRuntime;
        private string referenceId;
        private string accessPath;

        internal void Initialize(WebExtensionJSRuntime webExtensionJSRuntime, string referenceId, string accessPath)
        {
            if (!IsInitialized)
            {
                IsInitialized = true;
                this.webExtensionJSRuntime = webExtensionJSRuntime;
                this.referenceId = referenceId;
                this.accessPath = accessPath;
            }
        }

        /// <summary>
        /// Initialize property if it is a base object
        /// </summary>
        /// <param name="propertyName"></param>
        /// <param name="propertyValue"></param>
        protected void InitializeProperty(string propertyName, object propertyValue)
        {
            if (propertyValue is BaseObject baseObject && !baseObject.IsInitialized)
            {
                var propertyAccessPath = string.IsNullOrEmpty(accessPath) ? propertyName : $"{accessPath}.{propertyName}";
                baseObject.Initialize(webExtensionJSRuntime, referenceId, propertyAccessPath);
            }
        }

        /// <summary>
        /// Invokes the specified JavaScript function asynchronously.
        /// </summary>
        /// <param name="propertyName">The function to invoke.</param>
        /// <param name="args">JSON-serializable arguments.</param>
        /// <returns>An instance of TValue obtained by JSON-deserializing the return value.</returns>
        protected ValueTask<TValue> GetPropertyAsync<TValue>(string propertyName, params object[] args)
        {
            var functionIdentifier = string.IsNullOrEmpty(accessPath) ? propertyName : $"{accessPath}.{propertyName}";
            return webExtensionJSRuntime.InvokeAsync<TValue>("WebExtensionNet.InvokeOnObjectReference", new InvokeObjectReferenceOption(referenceId, functionIdentifier, false), args);
        }

        /// <summary>
        /// Invokes the specified JavaScript function asynchronously.
        /// </summary>
        /// <param name="function">The function to invoke.</param>
        /// <param name="args">JSON-serializable arguments.</param>
        /// <returns>An instance of TValue obtained by JSON-deserializing the return value.</returns>
        protected ValueTask<TValue> InvokeAsync<TValue>(string function, params object[] args)
        {
            var functionIdentifier = string.IsNullOrEmpty(accessPath) ? function : $"{accessPath}.{function}";
            return webExtensionJSRuntime.InvokeAsync<TValue>("WebExtensionNet.InvokeOnObjectReference", new InvokeObjectReferenceOption(referenceId, functionIdentifier, true), args);
        }

        /// <summary>
        /// Invokes the specified JavaScript function asynchronously.
        /// </summary>
        /// <param name="function">The function to invoke.</param>
        /// <param name="args">JSON-serializable arguments.</param>
        /// <returns>A System.Threading.Tasks.ValueTask that represents the asynchronous invocation operation.</returns>
        protected ValueTask InvokeVoidAsync(string function, params object[] args)
        {
            var functionIdentifier = string.IsNullOrEmpty(accessPath) ? function : $"{accessPath}.{function}";
            return webExtensionJSRuntime.InvokeVoidAsync("WebExtensionNet.InvokeOnObjectReference", new InvokeObjectReferenceOption(referenceId, functionIdentifier, true), args);
        }

        /// <summary>
        /// Dispose the object
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Dispose the object
        /// </summary>
        /// <param name="disposing"></param>
        protected virtual void Dispose(bool disposing)
        {
            // Cleanup
            if (!string.IsNullOrEmpty(referenceId) && webExtensionJSRuntime != null)
            {
#pragma warning disable CA2012 // Use ValueTasks correctly - Waiting is not supported in runtime
                webExtensionJSRuntime.InvokeVoidAsync("WebExtensionNet.RemoveObjectReference", new InvokeObjectReferenceOption(referenceId));
#pragma warning restore CA2012 // Use ValueTasks correctly
                referenceId = null;
            }
        }

        /// <summary>
        /// Finalizer to call Dispose()
        /// </summary>
        ~BaseObject()
        {
            Dispose(false);
        }
    }
    /// <summary>
    /// Base API class.
    /// </summary>
    public class BaseApi : BaseObject
    {
        /// <summary>
        /// Gets the WebExtensionJsRuntime instance.
        /// </summary>
        protected WebExtensionJSRuntime webExtensionJSRuntime;

        internal BaseApi(WebExtensionJSRuntime webExtensionJSRuntime, string apiNamespace)
        {
            this.webExtensionJSRuntime = webExtensionJSRuntime;
            Initialize(webExtensionJSRuntime, "browser", apiNamespace);
        }
    }
}