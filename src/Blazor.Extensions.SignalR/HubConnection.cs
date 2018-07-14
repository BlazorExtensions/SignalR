using Microsoft.AspNetCore.Blazor.Browser.Interop;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Blazor.Extensions
{
    public class HubConnection : IDisposable
    {
        private const string START_CONNECTION_METHOD = "Blazor.Extensions.SignalR.StartConnection";
        private const string STOP_CONNECTION_METHOD = "Blazor.Extensions.SignalR.StopConnection";
        internal string Url { get; }
        internal HttpConnectionOptions Options { get; }
        internal string InternalConnectionId { get; }

        private Dictionary<string, Func<object, Task>> _handlers = new Dictionary<string, Func<object, Task>>();

        public HubConnection(string url, HttpConnectionOptions options)
        {
            this.Url = url;
            this.Options = options;
            this.InternalConnectionId = Guid.NewGuid().ToString();
            HubConnectionManager.AddConnection(this);
        }

        public Task StartAsync() => RegisteredFunction.InvokeAsync<object>(START_CONNECTION_METHOD, this.InternalConnectionId);
        public Task StopAsync() => RegisteredFunction.InvokeAsync<object>(STOP_CONNECTION_METHOD, this.InternalConnectionId);

        public void On(string methodName, Func<object, Task> handler)
        {
            if (string.IsNullOrEmpty(methodName)) throw new ArgumentNullException(nameof(methodName));
            this._handlers[methodName] = handler ?? throw new ArgumentNullException(nameof(handler));
            HubConnectionManager.On(this.InternalConnectionId, methodName);
        }

        internal Task Dispatch(string methodName, object payload)
        {
            if (this._handlers.TryGetValue(methodName, out var handler))
            {
                return handler(payload);
            }

            return Task.CompletedTask;
        }

        public void Dispose() => HubConnectionManager.RemoveConnection(this.InternalConnectionId);
    }
}
