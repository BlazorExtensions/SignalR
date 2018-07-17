using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using System;
using System.Threading.Tasks;

namespace Blazor.Extensions.SignalR.Test.Server.Hubs
{
    [Authorize(JwtBearerDefaults.AuthenticationScheme)]
    public class ChatHub : Hub
    {
        public override async Task OnConnectedAsync()
        {
            await this.Clients.All.SendAsync("Send", $"{this.Context.ConnectionId} joined");
        }

        public override async Task OnDisconnectedAsync(Exception ex)
        {
            await this.Clients.Others.SendAsync("Send", $"{this.Context.ConnectionId} left");
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
