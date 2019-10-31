using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace Blazor.Extensions.SignalR.Test.Client.Pages
{
    public class ChatComponent : ComponentBase
    {
        [Inject] private HttpClient _http { get; set; }
        [Inject] private HubConnectionBuilder _hubConnectionBuilder { get; set; }
        //        [Inject] private ILogger<ChatComponent> _logger { get; set; }
        internal string ToEverybody { get; set; }
        internal string ToConnection { get; set; }
        internal string ConnectionId { get; set; }
        internal string ToMe { get; set; }
        internal string ToGroup { get; set; }
        internal string GroupName { get; set; }
        internal List<string> Messages { get; set; } = new List<string>();

        private IDisposable objectHandle;
        private IDisposable listHandle;
        private IDisposable multiArgsHandle;
        private IDisposable multiArgsComplexHandle;
        private IDisposable byteArrayHandle;
        private HubConnection connection;

        protected override async Task OnInitializedAsync()
        {
            this.connection = this._hubConnectionBuilder
                .WithUrl("/chathub",
                opt =>
                {
                    opt.LogLevel = SignalRLogLevel.None;
                    opt.Transport = HttpTransportType.WebSockets;
                    opt.SkipNegotiation = true;
                    opt.AccessTokenProvider = async () =>
                    {
                        var token = await this.GetJwtToken("DemoUser");
                        Console.WriteLine($"Access Token: {token}");
                        return token;
                    };
                })
                //.AddMessagePackProtocol()
                .Build();

            this.connection.On<string>("Send", this.Handle);
            this.connection.OnClose(exc =>
            {
                Console.WriteLine("Connection was closed! " + exc.ToString());
                return Task.CompletedTask;
            });
            await this.connection.StartAsync();
        }

        public Task DemoMethodObject(object data)
        {
            Console.WriteLine("Got object!");
            Console.WriteLine(data?.GetType().FullName ?? "<NULL>");
            this.objectHandle.Dispose();
            if (data == null) return Task.CompletedTask;
            return this.Handle(data);
        }

        public Task DemoMethodList(object data)
        {
            Console.WriteLine("Got List!");
            Console.WriteLine(data?.GetType().FullName ?? "<NULL>");
            this.listHandle.Dispose();
            if (data == null) return Task.CompletedTask;
            return this.Handle(data);
        }

        public Task DemoMultipleArgs(string arg1, int arg2, string arg3, int arg4)
        {
            Console.WriteLine("Got Multiple Args!");
            this.multiArgsHandle.Dispose();

            return this.HandleArgs(arg1, arg2, arg3, arg4);
        }

        public Task DemoMultipleArgsComplex(object arg1, object arg2)
        {
            Console.WriteLine("Got Multiple Args Complex!");
            this.multiArgsComplexHandle.Dispose();

            return this.HandleArgs(arg1, arg2);
        }

        public Task DemoByteArrayArg(byte[] array)
        {
            Console.WriteLine("Got byte array!");
            this.byteArrayHandle.Dispose();

            return this.HandleArgs(BitConverter.ToString(array));
        }

        private async Task<string> GetJwtToken(string userId)
        {
            var httpResponse = await this._http.GetAsync($"generatetoken?user={userId}");
            httpResponse.EnsureSuccessStatusCode();
            return await httpResponse.Content.ReadAsStringAsync();
        }

        private Task Handle(object msg)
        {
            Console.WriteLine(msg);
            if (msg is DemoData[])
            {
                var demoDatas = msg as DemoData[];
                DumpData(demoDatas);
            }
            else if (msg is DemoData)
            {
                DumpData(msg as DemoData);
            }
            else
            {
                this.Messages.Add(msg.ToString());
            }
            this.StateHasChanged();
            return Task.CompletedTask;
        }

        private void DumpData(params DemoData[] arr)
        {
            foreach (var demoData in arr)
            {
                this.Messages.Add($"demoData.id({demoData.Id}) | demoData.Data({demoData.Data}) | demoData.DateTime({demoData.DateTime}) | demoData.DecimalData({demoData.DecimalData}) | demoData.Bool({demoData.Bool})");
            }
        }

        private Task HandleArgs(params object[] args)
        {
            string msg = string.Join(", ", args);

            Console.WriteLine(msg);
            this.Messages.Add(msg);
            this.StateHasChanged();
            return Task.CompletedTask;
        }

        internal async Task Broadcast()
        {
            await this.connection.InvokeAsync("Send", this.ToEverybody);
        }

        internal async Task SendToOthers()
        {
            await this.connection.InvokeAsync("SendToOthers", this.ToEverybody);
        }

        internal async Task SendToConnection()
        {
            await this.connection.InvokeAsync("SendToConnection", this.ConnectionId, this.ToConnection);
        }

        internal async Task SendToMe()
        {
            await this.connection.InvokeAsync("Echo", this.ToMe);
        }

        internal async Task SendToGroup()
        {
            await this.connection.InvokeAsync("SendToGroup", this.GroupName, this.ToGroup);
        }

        internal async Task SendToOthersInGroup()
        {
            await this.connection.InvokeAsync("SendToOthersInGroup", this.GroupName, this.ToGroup);
        }

        internal async Task JoinGroup()
        {
            await this.connection.InvokeAsync("JoinGroup", this.GroupName);
        }

        internal async Task LeaveGroup()
        {
            await this.connection.InvokeAsync("LeaveGroup", this.GroupName);
        }

        internal async Task DoMultipleArgs()
        {
            this.multiArgsHandle = this.connection.On<string, int, string, int>("DemoMultiArgs", this.DemoMultipleArgs);
            this.multiArgsComplexHandle = this.connection.On<DemoData, DemoData[]>("DemoMultiArgs2", this.DemoMultipleArgsComplex);
            await this.connection.InvokeAsync("DoMultipleArgs");
            await this.connection.InvokeAsync("DoMultipleArgsComplex");
        }

        internal async Task DoByteArrayArg()
        {
            this.byteArrayHandle = this.connection.On<byte[]>("DemoByteArrayArg", this.DemoByteArrayArg);
            var array = await this.connection.InvokeAsync<byte[]>("DoByteArrayArg");

            Console.WriteLine("Got byte returned from hub method array: {0}", BitConverter.ToString(array));
        }

        internal async Task TellHubToDoStuff()
        {
            this.objectHandle = this.connection.On<DemoData>("DemoMethodObject", this.DemoMethodObject);
            this.listHandle = this.connection.On<DemoData[]>("DemoMethodList", this.DemoMethodList);
            await this.connection.InvokeAsync("DoSomething");
        }
    }
}
