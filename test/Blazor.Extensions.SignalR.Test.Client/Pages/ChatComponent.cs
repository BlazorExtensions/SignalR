using Blazor.Extensions.Logging;
using Microsoft.AspNetCore.Blazor.Components;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Blazor.Extensions.SignalR.Test.Client.Pages
{
    public class ChatComponent : BlazorComponent
    {
        [Inject] private HttpClient _http { get; set; }
        [Inject] private ILogger<ChatComponent> _logger { get; set; }
        internal string _toEverybody { get; set; }
        internal string _toConnection { get; set; }
        internal string _connectionId { get; set; }
        internal string _toMe { get; set; }
        internal string _toGroup { get; set; }
        internal string _groupName { get; set; }
        internal List<string> _messages { get; set; } = new List<string>();

        private IDisposable _objectHandle;
        private IDisposable _listHandle;
        private IDisposable _multiArgsHandle;
        private IDisposable _multiArgsComplexHandle;
        private HubConnection _connection;

        protected override async Task OnInitAsync()
        {
            this._connection = new HubConnectionBuilder()
                .WithUrl("/chathub",
                opt =>
                {
                    opt.LogLevel = SignalRLogLevel.None;
                    opt.Transport = HttpTransportType.WebSockets;
                    opt.SkipNegotiation = true;
                    opt.AccessTokenProvider = async () =>
                    {
                        var token = await this.GetJwtToken("DemoUser");
                        this._logger.LogInformation($"Access Token: {token}");
                        return token;
                    };
                })
                .AddMessagePackProtocol()
                .Build();

            this._connection.On<string>("Send", this.Handle);
            this._connection.OnClose(exc =>
            {
                this._logger.LogError(exc, "Connection was closed!");
                return Task.CompletedTask;
            });
            await this._connection.StartAsync();
        }

        public Task DemoMethodObject(object data)
        {
            this._logger.LogInformation("Got object!");
            this._logger.LogInformation(data?.GetType().FullName ?? "<NULL>");
            this._objectHandle.Dispose();
            if (data == null) return Task.CompletedTask;
            return this.Handle(data);
        }

        public Task DemoMethodList(object data)
        {
            this._logger.LogInformation("Got List!");
            this._logger.LogInformation(data?.GetType().FullName ?? "<NULL>");
            this._listHandle.Dispose();
            if (data == null) return Task.CompletedTask;
            return this.Handle(data);
        }

        public Task DemoMultipleArgs(string arg1, int arg2, string arg3, int arg4)
        {
            this._logger.LogInformation("Got Multiple Args!");
            this._multiArgsHandle.Dispose();

            return this.HandleArgs(arg1, arg2, arg3, arg4);
        }

        public Task DemoMultipleArgsComplex(object arg1, object arg2)
        {
            this._logger.LogInformation("Got Multiple Args Complex!");
            this._multiArgsComplexHandle.Dispose();

            return this.HandleArgs(arg1, arg2);
        }

        private async Task<string> GetJwtToken(string userId)
        {
            var httpResponse = await this._http.GetAsync($"/generatetoken?user={userId}");
            httpResponse.EnsureSuccessStatusCode();
            return await httpResponse.Content.ReadAsStringAsync();
        }

        private Task Handle(object msg)
        {
            this._logger.LogInformation(msg);
            this._messages.Add(msg.ToString());
            this.StateHasChanged();
            return Task.CompletedTask;
        }

        private Task HandleArgs(params object[] args)
        {
            string msg = string.Join(", ", args);

            this._logger.LogInformation(msg);
            this._messages.Add(msg);
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

        internal async Task DoMultipleArgs()
        {
            this._multiArgsHandle = this._connection.On<string, int, string, int>("DemoMultiArgs", this.DemoMultipleArgs);
            this._multiArgsComplexHandle = this._connection.On<DemoData, DemoData[]>("DemoMultiArgs2", this.DemoMultipleArgsComplex);
            await this._connection.InvokeAsync("DoMultipleArgs");
            await this._connection.InvokeAsync("DoMultipleArgsComplex");
        }

        internal async Task TellHubToDoStuff()
        {
            this._objectHandle = this._connection.On<DemoData>("DemoMethodObject", this.DemoMethodObject);
            this._listHandle = this._connection.On<DemoData[]>("DemoMethodList", this.DemoMethodList);
            await this._connection.InvokeAsync("DoSomething");
        }
    }
}
