using System;

namespace Blazor.Extensions
{
    public class HubConnectionBuilder
    {
        private bool _hubConnectionBuilt;

        internal string Url { get; set; }
        internal HttpConnectionOptions Options { get; set; }
        internal bool EnableMessagePack { get; set; }

        public HubConnection Build()
        {
            // Build can only be used once
            if (this._hubConnectionBuilt)
            {
                throw new InvalidOperationException("HubConnectionBuilder allows creation only of a single instance of HubConnection.");
            }

            this._hubConnectionBuilt = true;

            return new HubConnection(this.Url, this.Options, this.EnableMessagePack);
        }
    }

    /// <summary>
    /// Extension methods for <see cref="IHubConnectionBuilder"/>.
    /// </summary>
    public static class HubConnectionBuilderHttpExtensions
    {
        /// <summary>
        /// Configures the <see cref="HubConnection" /> to use HTTP-based transports to connect to the specified URL.
        /// </summary>
        /// <param name="hubConnectionBuilder">The <see cref="HubConnectionBuilder" /> to configure.</param>
        /// <param name="url">The URL the <see cref="HttpConnection"/> will use.</param>
        /// <returns>The same instance of the <see cref="HubConnectionBuilder"/> for chaining.</returns>
        public static HubConnectionBuilder WithUrl(this HubConnectionBuilder hubConnectionBuilder, string url, Action<HttpConnectionOptions> configureHttpOptions = null)
        {
            if (hubConnectionBuilder == null) throw new ArgumentNullException(nameof(hubConnectionBuilder));
            if (string.IsNullOrWhiteSpace(url)) throw new ArgumentNullException(nameof(url));

            hubConnectionBuilder.Url = url;
            var opt = new HttpConnectionOptions();
            configureHttpOptions?.Invoke(opt);
            hubConnectionBuilder.Options = opt;
            return hubConnectionBuilder;
        }

        /// <summary>
        /// Enables the MessagePack protocol for SignalR.
        /// </summary>
        /// <param name="hubConnectionBuilder">The <see cref="HubConnectionBuilder"/> representing the SignalR server to add MessagePack protocol support to.</param>
        /// <returns>The same instance of the <see cref="HubConnectionBuilder"/> for chaining.</returns>
        public static HubConnectionBuilder AddMessagePackProtocol(this HubConnectionBuilder hubConnectionBuilder)
        {
            if (hubConnectionBuilder == null) throw new ArgumentNullException(nameof(hubConnectionBuilder));
            hubConnectionBuilder.EnableMessagePack = true;
            return hubConnectionBuilder;
        }
    }
}
