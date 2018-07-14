namespace Blazor.Extensions
{
    public class ConnectionOperation
    {
        public string ConnectionId { get; set; }
    }

    public class MessagePacket : ConnectionOperation
    {
        public const string METHOD_NAME = "MessageArrived";
        public string MethodName { get; set; }
        public object Payload { get; set; }
    }
}
