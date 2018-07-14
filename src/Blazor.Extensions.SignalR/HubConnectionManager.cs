using Microsoft.AspNetCore.Blazor.Browser.Interop;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Blazor.Extensions
{
    internal static class HubConnectionManager
    {
        private const string ON_METHOD = "Blazor.Extensions.SignalR.On";
        private const string CREATE_CONNECTION_METHOD = "Blazor.Extensions.SignalR.CreateConnection";
        private const string REMOVE_CONNECTION_METHOD = "Blazor.Extensions.SignalR.RemoveConnection";

        private static readonly Dictionary<string, HubConnection> _connections = new Dictionary<string, HubConnection>();

        public static void On(string connectionId, string methodName) => RegisteredFunction.Invoke<object>(ON_METHOD, connectionId, methodName);

        public static Task Dispatch(string connectionId, string methodName, object payload) => _connections[connectionId].Dispatch(methodName, payload);

        public static void AddConnection(HubConnection connection)
        {
            RegisteredFunction.Invoke<object>(CREATE_CONNECTION_METHOD, connection.InternalConnectionId, connection.Url, connection.Options);
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
