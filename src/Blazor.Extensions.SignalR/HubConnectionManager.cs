using Microsoft.JSInterop;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Blazor.Extensions
{
    public static class HubConnectionManager
    {
        private const string ON_METHOD = "BlazorExtensions.HubConnectionManager.On";
        private const string OFF_METHOD = "BlazorExtensions.HubConnectionManager.Off";
        private const string CREATE_CONNECTION_METHOD = "BlazorExtensions.HubConnectionManager.CreateConnection";
        private const string REMOVE_CONNECTION_METHOD = "BlazorExtensions.HubConnectionManager.RemoveConnection";

        private static readonly Dictionary<string, HubConnection> _connections = new Dictionary<string, HubConnection>();

        [JSInvokable]
        public static void On(string connectionId, SubscriptionHandle subscriptionHandle) =>
            JSRuntime.Current.InvokeAsync<object>(ON_METHOD, connectionId, subscriptionHandle.MethodName, subscriptionHandle.HandleId);

        [JSInvokable]
        public static void Off(string connectionId, SubscriptionHandle subscriptionHandle) =>
            JSRuntime.Current.InvokeAsync<object>(OFF_METHOD, connectionId, subscriptionHandle.MethodName, subscriptionHandle.HandleId);

        [JSInvokable]
        public static Task Dispatch(string connectionId, string methodName, string handleId, string payload) => _connections[connectionId].Dispatch(methodName, handleId, payload);

        [JSInvokable]
        public static Task OnClose(string connectionId, string error) => _connections[connectionId].OnClose(error);

        [JSInvokable]
        public static Task<string> GetAccessToken(string connectionId) => _connections[connectionId].GetAccessToken();

        [JSInvokable]
        public static void AddConnection(HubConnection connection)
        {
            JSRuntime.Current.InvokeAsync<object>(CREATE_CONNECTION_METHOD,
                connection.InternalConnectionId,
                new InternalHttpConnectionOptions(connection.Options));


            _connections[connection.InternalConnectionId] = connection;
        }

        [JSInvokable]
        public static void RemoveConnection(string connectionId)
        {
            if (_connections.ContainsKey(connectionId))
            {
                _connections.Remove(connectionId);
            }
            JSRuntime.Current.InvokeAsync<object>(REMOVE_CONNECTION_METHOD, connectionId);
        }
    }
}
