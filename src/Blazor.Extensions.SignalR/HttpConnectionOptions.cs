using Microsoft.JSInterop;
using System;
using System.Threading.Tasks;

namespace Blazor.Extensions
{
    public class HttpConnectionOptions
    {   
        public HttpTransportType Transport { [JSInvokable]get; set; }
        public SignalRLogLevel LogLevel { [JSInvokable]get; set; }
        public bool LogMessageContent { [JSInvokable]get; set; }
        public bool SkipNegotiation { [JSInvokable]get; set; }
        public bool EnableMessagePack { [JSInvokable]get; set; }
        public string Url { [JSInvokable]get; set; }
        public Func<Task<string>> AccessTokenProvider { get; set; }

        [JSInvokable]
        public Task<string> GetAccessToken() => this.AccessTokenProvider?.Invoke();

        [JSInvokable]
        public bool HasAccessTokenFactory() => this.AccessTokenProvider != null;
    }
}
