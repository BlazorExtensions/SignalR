using System;

namespace Blazor.Extensions
{
    public class SubscriptionHandle : IDisposable
    {
        public string HandleId { get; private set; }
        public string MethodName { get; private set; }
        private readonly HubConnection _connection;

        internal SubscriptionHandle(string methodName, HubConnection connection)
        {
            this.MethodName = methodName;
            this.HandleId = Guid.NewGuid().ToString();
            this._connection = connection;
        }

        public void Dispose()
        {
            this._connection.RemoveHandle(this.MethodName, this.HandleId);
        }
    }
}
