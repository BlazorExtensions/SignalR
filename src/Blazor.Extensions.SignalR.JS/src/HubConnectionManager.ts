import * as signalR from "@aspnet/signalr";
import * as sianglRMessagePack from "@aspnet/signalr-protocol-msgpack";

type DotNetType = {
  invokeMethod<T>(assemblyName: string, methodIdentifier: string, ...args: any[]): T,
  invokeMethodAsync<T>(assemblyName: string, methodIdentifier: string, ...args: any[]): Promise<T>
}

const BlazorExtensionsSignalrAssembly = 'Blazor.Extensions.SignalR';
const DotNet: DotNetType = window["DotNet"];

export class HubConnectionManager {
  private _hubConnections: Map<string, signalR.HubConnection> = new Map<string, signalR.HubConnection>();
  private _handles: Map<string, (payload: any) => Promise<void>> = new Map<string, (payload: any) => Promise<void>>();

  public CreateConnection = (connectionId: string, httpConnectionOptions: any) => {
    if (!connectionId) throw new Error('Invalid connectionId.');
    if (!httpConnectionOptions) throw new Error('Invalid transport options.');
    if (!httpConnectionOptions.url) throw new Error('Invalid hub url.');

    let options: any = {
      logger: httpConnectionOptions.logLevel,
      transport: httpConnectionOptions.transport,
      logMessageContent: httpConnectionOptions.logMessageContent,
      skipNegotiation: httpConnectionOptions.skipNegotiation
    };

    if (httpConnectionOptions.hasAccessTokenFactory) {
      options.accessTokenFactory = () => {
        return new Promise<string>(async (resolve, reject) => {
          const token = await DotNet.invokeMethodAsync<string>(BlazorExtensionsSignalrAssembly, 'GetAccessToken', connectionId);

          if (token) {
            resolve(token);
          } else {
            reject();
          }
        })
      }
    }

    if (this._hubConnections[connectionId]) return;

    let connectionBuilder = new signalR.HubConnectionBuilder()
      .withUrl(httpConnectionOptions.url, options);

    if (httpConnectionOptions.addMessagePack) {
      connectionBuilder
        .withHubProtocol(new sianglRMessagePack.MessagePackHubProtocol());
    }

    this._hubConnections.set(connectionId, connectionBuilder.build());
  }

  public RemoveConnection = (connectionId: string) => {
    this._hubConnections.delete(connectionId);
  }

  public StartConnection = (connectionId: string): Promise<void> => {
    const connection = this.GetConnection(connectionId);

    connection.onclose(async err => {
      await DotNet.invokeMethodAsync(BlazorExtensionsSignalrAssembly, 'OnClose', connectionId, JSON.stringify(err));
    });

    return connection.start();
  }

  public StopConnection = (connectionId: string): Promise<void> => {
    const connection = this.GetConnection(connectionId);

    return connection.stop();
  }

  public InvokeAsync = (connectionId: string, methodName: string, args: any[]): Promise<void> => {
    const connection = this.GetConnection(connectionId);
    return connection.invoke(methodName, ...args);
  }

  public InvokeWithResultAsync = (connectionId: string, methodName: string, args: any[]): Promise<any> => {
    const connection = this.GetConnection(connectionId);
    return connection.invoke(methodName, ...args);
  }

  private GetConnection = (connectionId: string) => {
    if (!connectionId) throw new Error('Invalid connectionId.');
    const connection = this._hubConnections.get(connectionId);
    if (!connection) throw new Error('Invalid connection.');

    return connection;
  }

  private On = (connectionId: string, methodName: string, handleId: string) => {
    const connection = this.GetConnection(connectionId);
    const handle = (payload) => this.OnHandler(connectionId, methodName, handleId, payload);
    this._handles.set(handleId, handle);
    connection.on(methodName, handle);
  }

  private Off = (connectionId: string, methodName: string, handleId: string) => {
    const connection = this.GetConnection(connectionId);
    const handle = this._handles.get(handleId);
    if (handle) {
      connection.off(methodName, handle);
      this._handles.delete(handleId);
    }
  }

  private OnHandler = async (connectionId: string, methodName: string, handleId: string, payload: any) => {
    await DotNet.invokeMethodAsync(BlazorExtensionsSignalrAssembly, 'Dispatch', connectionId, methodName, handleId, JSON.stringify(payload));
  }

  public static initialize() {
    //const Blazor: BlazorType = window["Blazor"];
    //window["BlazorExtensions"].HubConnectionManager = new HubConnectionManager();

    //Blazor.registerFunction('Blazor.Extensions.SignalR.InvokeAsync', (connectionId: string, methodName: string, args: any) => {
    //  //TODO remove this parse after Blazor fixed the async args json parsing code
    //  const parsedConnectionId = JSON.parse(connectionId);
    //  const parsedMethodName = JSON.parse(methodName);
    //  const parsedArgs = JSON.parse(args);

    //  return window["BlazorExtensions"].HubConnectionManager.invokeAsync(parsedConnectionId, parsedMethodName, ...parsedArgs);
    //});

    //Blazor.registerFunction('Blazor.Extensions.SignalR.InvokeWithResultAsync', (connectionId: string, methodName: string, args: any) => {
    //  //TODO remove this parse after Blazor fixed the async args json parsing code
    //  const parsedConnectionId = JSON.parse(connectionId);
    //  const parsedMethodName = JSON.parse(methodName);
    //  const parsedArgs = JSON.parse(args);

    //  return window["BlazorExtensions"].HubConnectionManager.invokeWithResultAsync(parsedConnectionId, parsedMethodName, ...parsedArgs);
    //});

    //Blazor.registerFunction('Blazor.Extensions.SignalR.On', (connectionId: string, methodName: string, handleId: string) => {
    //  return window["BlazorExtensions"].HubConnectionManager.on(connectionId, methodName, handleId);
    //});

    //Blazor.registerFunction('Blazor.Extensions.SignalR.Off', (connectionId: string, methodName: string, handleId: string) => {
    //  return window["BlazorExtensions"].HubConnectionManager.off(connectionId, methodName, handleId);
    //});
  }
}
