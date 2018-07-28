using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace Blazor.Extensions
{
    public class HubCloseCallback
    {
        private readonly Func<Exception, Task> _callback;

        public HubCloseCallback(Func<Exception, Task> closeCallback)
        {
            this._callback = closeCallback;
        }

        [JSInvokable]
        public Task OnClose(string error) => this._callback(new Exception(error));
    }
}
