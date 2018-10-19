using System;
using Microsoft.JSInterop;

namespace Blazor.Extensions
{
    internal static class JSRuntimeExtensions
    {
        public static T InvokeSync<T>(this IJSRuntime jsRuntime, string identifier, params object[] args)
        {
            if (jsRuntime == null)
                throw new ArgumentNullException(nameof(jsRuntime));

            if (jsRuntime is IJSInProcessRuntime inProcessJsRuntime)
            {
                return inProcessJsRuntime.Invoke<T>(identifier, args);
            }

            return jsRuntime.InvokeAsync<T>(identifier, args).ConfigureAwait(false).GetAwaiter().GetResult();
        }
    }
}
