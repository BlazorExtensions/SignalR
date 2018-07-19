using Microsoft.AspNetCore.Blazor.Browser.Interop;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Blazor.Extensions
{
    internal static class HubConnectionManager
    {
        private const string ON_METHOD = "Blazor.Extensions.SignalR.On";
        private const string OFF_METHOD = "Blazor.Extensions.SignalR.Off";
        private const string CREATE_CONNECTION_METHOD = "Blazor.Extensions.SignalR.CreateConnection";
        private const string REMOVE_CONNECTION_METHOD = "Blazor.Extensions.SignalR.RemoveConnection";

        private static readonly Dictionary<string, HubConnection> _connections = new Dictionary<string, HubConnection>();

        public static void On(string connectionId, SubscriptionHandle subscriptionHandle) =>
            RegisteredFunction.Invoke<object>(ON_METHOD, connectionId, subscriptionHandle.MethodName, subscriptionHandle.HandleId);
        public static void Off(string connectionId, SubscriptionHandle subscriptionHandle) =>
            RegisteredFunction.Invoke<object>(OFF_METHOD, connectionId, subscriptionHandle.MethodName, subscriptionHandle.HandleId);
        public static Task Dispatch(string connectionId, string methodName, string handleId, string payload) => _connections[connectionId].Dispatch(methodName, handleId, payload);
        public static Task OnClose(string connectionId, string error) => _connections[connectionId].OnClose(error);
        public static Task<string> GetAccessToken(string connectionId) => _connections[connectionId].GetAccessToken();

        public static void AddConnection(HubConnection connection)
        {
            RegisteredFunction.Invoke<object>(CREATE_CONNECTION_METHOD,
                connection.InternalConnectionId,
                new InternalHttpConnectionOptions(connection.Options));
            _connections[connection.InternalConnectionId] = connection;
        }

        public static void RemoveConnection(string connectionId)
        {
            if (_connections.ContainsKey(connectionId))
            {
                _connections.Remove(connectionId);
            }
            RegisteredFunction.Invoke<object>(REMOVE_CONNECTION_METHOD, connectionId);
        }
    }
}
