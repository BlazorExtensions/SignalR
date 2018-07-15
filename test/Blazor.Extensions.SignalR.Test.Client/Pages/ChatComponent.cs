using Blazor.Extensions.Logging;
using Microsoft.AspNetCore.Blazor.Components;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Blazor.Extensions.SignalR.Test.Client.Pages
{
    public class ChatComponent : BlazorComponent
    {
        [Inject] private ILogger<ChatComponent> _logger { get; set; }
        internal string _toEverybody { get; set; }
        internal string _toConnection { get; set; }
        internal string _connectionId { get; set; }
        internal string _toMe { get; set; }
        internal string _toGroup { get; set; }
        internal string _groupName { get; set; }
        internal List<string> _messages { get; set; } = new List<string>();

        private HubConnection _connection;

        protected override async Task OnInitAsync()
        {
            this._connection = new HubConnectionBuilder()
                .WithUrl("/chathub",
                opt =>
                {
                    opt.LogLevel = SignalRLogLevel.Trace;
                    opt.Transport = HttpTransportType.WebSockets;
                })
                .Build();

            this._connection.On("Send", this.Handle);
            await this._connection.StartAsync();
        }

        private Task Handle(object msg)
        {
            this._logger.LogInformation(msg);
            this._messages.Add(msg.ToString());
            this.StateHasChanged();
            return Task.CompletedTask;
        }

        internal async Task Broadcast()
        {
            await this._connection.InvokeAsync("Send", this._toEverybody);
        }

        internal async Task SendToOthers()
        {
            await this._connection.InvokeAsync("SendToOthers", this._toEverybody);
        }

        internal async Task SendToConnection()
        {
            await this._connection.InvokeAsync("SendToConnection", this._connectionId, this._toConnection);
        }

        internal async Task SendToMe()
        {
            await this._connection.InvokeAsync("Echo", this._toMe);
        }

        internal async Task SendToGroup()
        {
            await this._connection.InvokeAsync("SendToGroup", this._groupName, this._toGroup);
        }

        internal async Task SendToOthersInGroup()
        {
            await this._connection.InvokeAsync("SendToOthersInGroup", this._groupName, this._toGroup);
        }

        internal async Task JoinGroup()
        {
            await this._connection.InvokeAsync("JoinGroup", this._groupName);
        }

        internal async Task LeaveGroup()
        {
            await this._connection.InvokeAsync("LeaveGroup", this._groupName);
        }
    }
}
