using System;

namespace Blazor.Extensions
{
    public class HubConnectionBuilder
    {
        private bool _hubConnectionBuilt;

        internal string Url { get; set; }
        internal HttpConnectionOptions Options { get; set; }

        public HubConnection Build()
        {
            // Build can only be used once
            if (this._hubConnectionBuilt)
            {
                throw new InvalidOperationException("HubConnectionBuilder allows creation only of a single instance of HubConnection.");
            }

            this._hubConnectionBuilt = true;

            return new HubConnection(this.Url, this.Options);
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
        /// <param name="hubConnectionBuilder">The <see cref="IHubConnectionBuilder" /> to configure.</param>
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
    }
}
