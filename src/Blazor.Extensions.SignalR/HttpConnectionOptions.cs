namespace Blazor.Extensions
{
    public class HttpConnectionOptions
    {
        public HttpTransportType Transport { get; set; }
        public SignalRLogLevel LogLevel { get; set; }
        public bool LogMessageContent { get; set; }
        public bool SkipNegotiation { get; set; }
        public string AccessToken { get; set; }
    }
}
