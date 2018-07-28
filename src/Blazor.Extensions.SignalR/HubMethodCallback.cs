using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace Blazor.Extensions
{
    public class HubMethodCallback : IDisposable
    {
        private readonly HubConnection _connection;
        private readonly Func<string, Task> _callback;

        public HubMethodCallback(string id, string methodName, HubConnection connection, Func<string, Task> callback)
        {
            this.MethodName = methodName;
            this.Id = id;
            this._connection = connection;
            this._callback = callback;
        }

        public string MethodName { [JSInvokable]get; private set; }
        public string Id { [JSInvokable]get; private set; }
        
        [JSInvokable]
        public Task On(string payload) => this._callback(payload);

        public void Dispose()
        {
            this._connection.RemoveHandle(this.MethodName, this.Id);
        }
    }
}
