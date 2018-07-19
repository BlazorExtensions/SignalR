using Microsoft.AspNetCore.Blazor;
using Microsoft.AspNetCore.Blazor.Browser.Interop;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Blazor.Extensions
{
    public class HubConnection : IDisposable
    {
        private const string START_CONNECTION_METHOD = "Blazor.Extensions.SignalR.StartConnection";
        private const string STOP_CONNECTION_METHOD = "Blazor.Extensions.SignalR.StopConnection";
        private const string INVOKE_ASYNC_METHOD = "Blazor.Extensions.SignalR.InvokeAsync";
        private const string INVOKE_WITH_RESULT_ASYNC_METHOD = "Blazor.Extensions.SignalR.InvokeWithResultAsync";
        internal HttpConnectionOptions Options { get; }
        internal string InternalConnectionId { get; }
        
        private Dictionary<string, Dictionary<string, (SubscriptionHandle, Func<string, Task>)>> _handlers = new Dictionary<string, Dictionary<string, (SubscriptionHandle, Func<string, Task>)>>();
        //private Dictionary<string, Func<string, Task>> _handlers = new Dictionary<string, Func<string, Task>>();
        private Func<Exception, Task> _errorHandler;

        public HubConnection(HttpConnectionOptions options)
        {
            this.Options = options;
            this.InternalConnectionId = Guid.NewGuid().ToString();
            HubConnectionManager.AddConnection(this);
        }

        internal Task<string> GetAccessToken() => this.Options.AccessTokenProvider != null ? this.Options.AccessTokenProvider() : null;
        internal Task OnClose(string error) => this._errorHandler != null ? this._errorHandler(new Exception(error)) : Task.CompletedTask;

        public Task StartAsync() => RegisteredFunction.InvokeAsync<object>(START_CONNECTION_METHOD, this.InternalConnectionId);
        public Task StopAsync() => RegisteredFunction.InvokeAsync<object>(STOP_CONNECTION_METHOD, this.InternalConnectionId);

        public IDisposable On<TResult>(string methodName, Func<TResult, Task> handler)
        {
            if (string.IsNullOrEmpty(methodName)) throw new ArgumentNullException(nameof(methodName));
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            var subscriptionHandle = new SubscriptionHandle(methodName, this);
            if (this._handlers.TryGetValue(methodName, out var methodHandlers))
            {
                methodHandlers[subscriptionHandle.HandleId] =
                    (subscriptionHandle, (json) =>
                    {
                        var payload = JsonUtil.Deserialize<TResult>(json);
                        return handler(payload);
                    });
            }
            else
            {
                this._handlers[methodName] = new Dictionary<string, (SubscriptionHandle, Func<string, Task>)>
                {
                    {
                        subscriptionHandle.HandleId,
                        (subscriptionHandle, (json) =>
                        {
                            var payload = JsonUtil.Deserialize<TResult>(json);
                            return handler(payload);
                        })
                    }
                };
            }
            
            HubConnectionManager.On(this.InternalConnectionId, subscriptionHandle);
            return subscriptionHandle;
        }

        internal void RemoveHandle(string methodName, string handleId)
        {
            if (this._handlers.TryGetValue(methodName, out var handlers))
            {
                if (handlers.TryGetValue(handleId, out var handle))
                {
                    HubConnectionManager.Off(this.InternalConnectionId, handle.Item1);
                    handlers.Remove(handleId);

                    if (handlers.Count == 0)
                    {
                        this._handlers.Remove(methodName);
                    }
                }
            }
        }

        public void OnClose(Func<Exception, Task> handler) => this._errorHandler = handler;

        public Task InvokeAsync(string methodName, params object[] args) =>
            RegisteredFunction.InvokeAsync<object>(INVOKE_ASYNC_METHOD, this.InternalConnectionId, methodName, args);

        public Task<TResult> InvokeAsync<TResult>(string methodName, params object[] args) =>
            RegisteredFunction.InvokeAsync<TResult>(INVOKE_WITH_RESULT_ASYNC_METHOD, this.InternalConnectionId, methodName, args);

        internal Task Dispatch(string methodName, string handleId, string payload)
        {
            if (this._handlers.TryGetValue(methodName, out var handlers))
            {
                if (handlers.TryGetValue(handleId, out var handle))
                {
                    return handle.Item2(payload);
                }
            }
            return Task.CompletedTask;
        }

        public void Dispose() => HubConnectionManager.RemoveConnection(this.InternalConnectionId);
    }
}
