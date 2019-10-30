using Blazor.Extensions.SignalR.Test.Client;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Blazor.Extensions.SignalR.Test.Server.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        public async Task DoSomething()
        {
            await this.Clients.All.SendAsync("DemoMethodObject", new DemoData { Id = 1, Data = "Demo Data" });
            await this.Clients.All.SendAsync("DemoMethodList", Enumerable.Range(1, 10).Select(x => new DemoData { Id = x, Data = $"Demo Data #{x}" }).ToList());
        }

        public override async Task OnConnectedAsync()
        {
            await this.Clients.All.SendAsync("Send", $"{this.Context.ConnectionId} joined");
        }

        public override async Task OnDisconnectedAsync(Exception ex)
        {
            await this.Clients.Others.SendAsync("Send", $"{this.Context.ConnectionId} left");
        }

        public async Task<byte[]> DoByteArrayArg()
        {
            var array = new byte[] { 1, 2, 3 };

            await this.Clients.All.SendAsync("DemoByteArrayArg", array);

            return array;
        }

        public Task DoMultipleArgs()
        {
            return this.Clients.All.SendAsync("DemoMultiArgs", "one", 2, "three", 4);
        }

        public Task DoMultipleArgsComplex()
        {
            return this.Clients.All.SendAsync("DemoMultiArgs2", new DemoData { Id = 1, Data = "Demo Data", DecimalData = 0.000000001M, DateTime = DateTime.UtcNow, Bool = true },
                Enumerable.Range(1, 10).Select(x => new DemoData { Id = x, Data = $"Demo Data #{x}", DecimalData = x * 0.000000001M , DateTime = DateTime.UtcNow.AddSeconds(-x), Bool = (x % 2 == 0) }).ToList());
        }

        public Task Send(string message)
        {
            return this.Clients.All.SendAsync("Send", $"{this.Context.ConnectionId}: {message}");
        }

        public Task SendToOthers(string message)
        {
            return this.Clients.Others.SendAsync("Send", $"{this.Context.ConnectionId}: {message}");
        }

        public Task SendToConnection(string connectionId, string message)
        {
            return this.Clients.Client(connectionId).SendAsync("Send", $"Private message from {this.Context.ConnectionId}: {message}");
        }

        public Task SendToGroup(string groupName, string message)
        {
            return this.Clients.Group(groupName).SendAsync("Send", $"{this.Context.ConnectionId}@{groupName}: {message}");
        }

        public Task SendToOthersInGroup(string groupName, string message)
        {
            return this.Clients.OthersInGroup(groupName).SendAsync("Send", $"{this.Context.ConnectionId}@{groupName}: {message}");
        }

        public async Task JoinGroup(string groupName)
        {
            await this.Groups.AddToGroupAsync(this.Context.ConnectionId, groupName);

            await this.Clients.Group(groupName).SendAsync("Send", $"{this.Context.ConnectionId} joined {groupName}");
        }

        public async Task LeaveGroup(string groupName)
        {
            await this.Clients.Group(groupName).SendAsync("Send", $"{this.Context.ConnectionId} left {groupName}");

            await this.Groups.RemoveFromGroupAsync(this.Context.ConnectionId, groupName);
        }

        public Task Echo(string message)
        {
            return this.Clients.Caller.SendAsync("Send", $"{this.Context.ConnectionId}: {message}");
        }
    }
}
