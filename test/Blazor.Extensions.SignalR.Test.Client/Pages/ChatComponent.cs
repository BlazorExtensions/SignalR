using Blazor.Extensions.Logging;
using Microsoft.AspNetCore.Blazor.Components;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace Blazor.Extensions.SignalR.Test.Client.Pages
{
    public class ChatComponent : BlazorComponent
    {
        [Inject] private ILogger<ChatComponent> _logger { get; set; }
        [Parameter] internal string _toEverybody { get; set; }
        [Parameter] internal string _toConnection { get; set; }
        [Parameter] internal string _connectionId { get; set; }
        [Parameter] internal string _toMe { get; set; }
        [Parameter] internal string _toGroup { get; set; }
        [Parameter] internal string _groupName { get; set; }

        private HubConnection _connection;

        protected override async Task OnInitAsync()
        {
            this._connection = new HubConnectionBuilder()
                .WithUrl("http://localhost:59663/chathub",
                opt =>
                {
                    opt.LogLevel = SignalRLogLevel.Trace;
                    opt.Transport = HttpTransportType.WebSockets;
                })
                .Build();
            
            this._connection.On("MessageArrived", this.Handle);
            await this._connection.StartAsync();
            this._logger.LogInformation("OnInit completed.");
        }

        private Task Handle(object msg)
        {
            this._logger.LogInformation(msg);

            return Task.CompletedTask;
        }
    }
}
