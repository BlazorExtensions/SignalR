using System;
using System.Text.Json.Serialization;

namespace Blazor.Extensions.SignalR.Test.Client
{
    public class DemoData
    {
        [JsonPropertyName("id")]
        public long Id { get; set; }

        [JsonPropertyName("data")]
        public string Data { get; set; }

        [JsonPropertyName("decimalData")]
        public decimal DecimalData { get; set; }

        [JsonPropertyName("dateTime")]
        public DateTime DateTime { get; set; }

        [JsonPropertyName("bool")]
        public bool Bool { get; set; }
    }
}
