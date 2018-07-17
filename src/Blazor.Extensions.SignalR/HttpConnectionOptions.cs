using System;
using System.Runtime.Serialization;
using System.Threading.Tasks;

namespace Blazor.Extensions
{
    public class HttpConnectionOptions
    {
        public HttpTransportType Transport { get; set; }
        public SignalRLogLevel LogLevel { get; set; }
        public bool LogMessageContent { get; set; }
        public bool SkipNegotiation { get; set; }
        internal bool EnableMessagePack { get; set; }
        internal string Url { get; set; }
        public Func<Task<string>> AccessTokenProvider { get; set; }
    }

    internal class InternalHttpConnectionOptions
    {
        public HttpTransportType Transport { get; set; }
        public SignalRLogLevel LogLevel { get; set; }
        public bool LogMessageContent { get; set; }
        public bool SkipNegotiation { get; set; }
        public bool EnableMessagePack { get; set; }
        public string Url { get; set; }
        public bool HasAccessTokenFactory { get; set; }

        public InternalHttpConnectionOptions(HttpConnectionOptions options)
        {
            this.Transport = options.Transport;
            this.LogLevel = options.LogLevel;
            this.LogMessageContent = options.LogMessageContent;
            this.SkipNegotiation = options.SkipNegotiation;
            this.EnableMessagePack = options.EnableMessagePack;
            this.Url = options.Url;
            this.HasAccessTokenFactory = options.AccessTokenProvider != null;
        }
    }
}
