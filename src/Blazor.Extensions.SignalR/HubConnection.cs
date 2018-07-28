using Microsoft.JSInterop;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Blazor.Extensions
{
    public class HubConnection : IDisposable
    {
        private const string ON_METHOD = "BlazorExtensions.SignalR.On";
        private const string ON_CLOSE_METHOD = "BlazorExtensions.SignalR.OnClose";
        private const string OFF_METHOD = "BlazorExtensions.SignalR.Off";
        private const string CREATE_CONNECTION_METHOD = "BlazorExtensions.SignalR.CreateConnection";
        private const string REMOVE_CONNECTION_METHOD = "BlazorExtensions.SignalR.RemoveConnection";
        private const string START_CONNECTION_METHOD = "BlazorExtensions.SignalR.StartConnection";
        private const string STOP_CONNECTION_METHOD = "BlazorExtensions.SignalR.StopConnection";
        private const string INVOKE_ASYNC_METHOD = "BlazorExtensions.SignalR.InvokeAsync";
        private const string INVOKE_WITH_RESULT_ASYNC_METHOD = "BlazorExtensions.SignalR.InvokeWithResultAsync";

        internal HttpConnectionOptions Options { get; }
        internal string InternalConnectionId { get; }

        private Dictionary<string, Dictionary<string, HubMethodCallback>> _callbacks = new Dictionary<string, Dictionary<string, HubMethodCallback>>();

        private HubCloseCallback _closeCallback;

        public HubConnection(HttpConnectionOptions options)
        {
            this.Options = options;
            this.InternalConnectionId = Guid.NewGuid().ToString();
            ((IJSInProcessRuntime)JSRuntime.Current).Invoke<object>(CREATE_CONNECTION_METHOD,
                this.InternalConnectionId,
                new DotNetObjectRef(this.Options));
        }


        public Task StartAsync() => JSRuntime.Current.InvokeAsync<object>(START_CONNECTION_METHOD, this.InternalConnectionId);
        public Task StopAsync() => JSRuntime.Current.InvokeAsync<object>(STOP_CONNECTION_METHOD, this.InternalConnectionId);

        public IDisposable On<TResult>(string methodName, Func<TResult, Task> handler)
        {
            if (string.IsNullOrEmpty(methodName)) throw new ArgumentNullException(nameof(methodName));
            if (handler == null) throw new ArgumentNullException(nameof(handler));

            var callbackId = Guid.NewGuid().ToString();

            var callback = new HubMethodCallback(callbackId, methodName, this,
                 (json) =>
                 {
                     var payload = Json.Deserialize<TResult>(json);
                     return handler(payload);
                 }
            );

            if (this._callbacks.TryGetValue(methodName, out var methodHandlers))
            {
                methodHandlers[callback.Id] = callback;
            }
            else
            {
                this._callbacks[methodName] = new Dictionary<string, HubMethodCallback>
                {
                    { callback.Id, callback }
                };
            }

            ((IJSInProcessRuntime)JSRuntime.Current).Invoke<object>(ON_METHOD, this.InternalConnectionId, new DotNetObjectRef(callback));

            //HubConnectionManager.On(this.InternalConnectionId, callback);
            return callback;
        }

        internal void RemoveHandle(string methodName, string callbackId)
        {
            if (this._callbacks.TryGetValue(methodName, out var callbacks))
            {
                if (callbacks.TryGetValue(callbackId, out var callback))
                {
                    ((IJSInProcessRuntime)JSRuntime.Current).Invoke<object>(OFF_METHOD, this.InternalConnectionId, new DotNetObjectRef(callback));
                    //HubConnectionManager.Off(this.InternalConnectionId, handle.Item1);
                    callbacks.Remove(callbackId);

                    if (callbacks.Count == 0)
                    {
                        this._callbacks.Remove(methodName);
                    }
                }
            }
        }

        public void OnClose(Func<Exception, Task> callback) {
            this._closeCallback = new HubCloseCallback(callback);
            ((IJSInProcessRuntime)JSRuntime.Current).Invoke<object>(ON_CLOSE_METHOD,
                this.InternalConnectionId,
                new DotNetObjectRef(this._closeCallback));
        } 

        public Task InvokeAsync(string methodName, params object[] args) =>
            JSRuntime.Current.InvokeAsync<object>(INVOKE_ASYNC_METHOD, this.InternalConnectionId, methodName, args);

        public Task<TResult> InvokeAsync<TResult>(string methodName, params object[] args) =>
            JSRuntime.Current.InvokeAsync<TResult>(INVOKE_WITH_RESULT_ASYNC_METHOD, this.InternalConnectionId, methodName, args);

        public void Dispose() => ((IJSInProcessRuntime)JSRuntime.Current).Invoke<object>(REMOVE_CONNECTION_METHOD, this.InternalConnectionId);
    }
}
