[![Build status](https://dotnet-ci.visualstudio.com/DotnetCI/_apis/build/status/Blazor-Extensions-SignalR-CI?branch=master)](https://dotnet-ci.visualstudio.com/DotnetCI/_build/latest?definitionId=5&branch=master)
[![Package Version](https://img.shields.io/nuget/v/Blazor.Extensions.SignalR.svg)](https://www.nuget.org/packages/Blazor.Extensions.SignalR)
[![NuGet Downloads](https://img.shields.io/nuget/dt/Blazor.Extensions.SignalR.svg)](https://www.nuget.org/packages/Blazor.Extensions.SignalR)
[![License](https://img.shields.io/github/license/BlazorExtensions/SignalR.svg)](https://github.com/BlazorExtensions/SignalR/blob/master/LICENSE)

# Blazor Extensions

Blazor Extensions is a set of packages with the goal of adding useful features to [Blazor](https://blazor.net).

# Blazor Extensions SignalR

This package adds a [Microsoft ASP.NET Core SignalR](https://github.com/aspnet/SignalR) client library for [Microsoft ASP.NET Blazor](https://github.com/aspnet/Blazor).

The package aims to mimic the C# APIs of SignalR Client as much as possible and it is developed by wrapping the TypeScript client by using Blazor's interop capabilities. 

For more information about SignalR development, please check [SignalR documentation](https://docs.microsoft.com/en-us/aspnet/core/signalr/introduction?view=aspnetcore-2.1).

# Features

This package implements all public features of SignalR Typescript client.

> Note: The _Streaming_ APIs are not implemented yet. We will add it soon.

# Sample usage

The following snippet shows how to setup the client to send and receive messages using SignalR.

```c#
var connection = new HubConnectionBuilder()
        .WithUrl("/myHub", // The hub URL. If the Hub is hosted on the server where the blazor is hosted, you can just use the relative path.
        opt =>
        {
            opt.LogLevel = SignalRLogLevel.Trace; // Client log level
            opt.Transport = HttpTransportType.WebSockets; // Which transport you want to use for this connection
        })
        .Build(); // Build the HubConnection

connection.On("Receive", this.Handle); // Subscribe to messages sent from the Hub to the "Receive" method by passing a handle (Func<object, Task>) to process messages.
await connection.StartAsync(); // Start the connection.

await connection.InvokeAsync("ServerMethod", param1, param2, paramX); // Invoke a method on the server called "ServerMethod" and pass parameters to it. 

var result = await connection.InvokeAsync<MyResult>("ServerMethod", param1, param2, paramX); // Invoke a method on the server called "ServerMethod", pass parameters to it and get the result back.
```

# Contributions and feedback

Please feel free to use the component, open issues, fix bugs or provide feedback.

# Contributors

The following people are the maintainers of the Blazor Extensions projects:

- [Attila Hajdrik](https://github.com/attilah)
- [Gutemberg Ribiero](https://github.com/galvesribeiro)
