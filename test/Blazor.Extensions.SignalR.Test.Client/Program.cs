using Blazor.Extensions.Logging;
using Microsoft.AspNetCore.Blazor.Browser.Rendering;
using Microsoft.AspNetCore.Blazor.Browser.Services;
using Microsoft.Extensions.DependencyInjection;

namespace Blazor.Extensions.SignalR.Test.Client
{
    public class Program
    {
        static void Main(string[] args)
        {
            var serviceProvider = new BrowserServiceProvider(services =>
            {
                // Add any custom services here
                services.AddLogging(builder => builder.AddBrowserConsole());
            });

            new BrowserRenderer(serviceProvider).AddComponent<App>("app");
        }
    }
}
