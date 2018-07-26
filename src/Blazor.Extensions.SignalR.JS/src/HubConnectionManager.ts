import * as signalR from "@aspnet/signalr";
import * as sianglRMessagePack from "@aspnet/signalr-protocol-msgpack";

const assemblyName = 'Blazor.Extensions.SignalR';
export class HubConnectionManager {
  private _hubConnections: Map<string, signalR.HubConnection> = new Map<string, signalR.HubConnection>();
  private _handles: Map<string, (payload: any) => Promise<void>> = new Map<string, (payload: any) => Promise<void>>();

  public CreateConnection = (connectionId: string, httpConnectionOptions) => {

    var addMessagePack: boolean = httpConnectionOptions.enableMessagePack;
    var url = httpConnectionOptions.url;

    var transportOptions: signalR.IHttpConnectionOptions = {
      logger: httpConnectionOptions.logLevel,
      transport: httpConnectionOptions.transport,
      logMessageContent: httpConnectionOptions.logMessageContent,
      skipNegotiation: httpConnectionOptions.skipNegotiation
    };


    if (httpConnectionOptions.hasAccessTokenFactory) {
      transportOptions.accessTokenFactory = () => {
        return new Promise<string>(async (resolve, reject) => {
          try {
            var token = await DotNet
              .invokeMethodAsync<string>(assemblyName, "GetAccessToken", connectionId);
            console.debug("Access token: " + token)
            if (token) {
              resolve(token);
            } else {
              reject("Could not get the AccessToken");
            }
          } catch (e) {
            reject(e);
          }
        });
      }
    }
    if (!connectionId) throw new Error('Invalid connectionId.');
    if (!url) throw new Error('Invalid hub url.');
    if (!transportOptions) throw new Error('Invalid transport options.');

    if (this._hubConnections[connectionId]) return;
    var builder = new signalR.HubConnectionBuilder()
      .withUrl(url, transportOptions);
    if (addMessagePack) {
      builder
        .withHubProtocol(new sianglRMessagePack.MessagePackHubProtocol());
    }
    const connection = builder.build();
    this._hubConnections.set(connectionId, connection);

    return true;
  }

  public RemoveConnection = (connectionId: string) => {
    this._hubConnections.delete(connectionId);
  }

  public StartConnection = (connectionId: string): Promise<void> => {
    const connection = this.getConnection(connectionId);

    connection.onclose(async err => {
      await DotNet.invokeMethodAsync(assemblyName, "OnClose", connectionId, JSON.stringify(err));
    });

    return connection.start();
  }

  public StopConnection = (connectionId: string): Promise<void> => {
    const connection = this.getConnection(connectionId);

    return connection.stop();
  }

  public InvokeAsync = (connectionId: string, methodName: string, args: any[]): Promise<void> => {

    const connection = this.getConnection(connectionId);

    return connection.invoke(methodName,...args);
  }

  public InvokeWithResultAsync = (connectionId: string, methodName: string, args: any[]): Promise<any> => {

    let connection = this.getConnection(connectionId);
    return connection.invoke(methodName, ...args);
  }

  private getConnection = (connectionId: string) => {
    if (!connectionId) throw new Error('Invalid connectionId.');
    const connection = this._hubConnections.get(connectionId);
    if (!connection) throw new Error('Connection not found with the id:' + connectionId);

    return connection;
  }

  public On = (connectionId: string, methodName: string, handleId: string) => {
    const connection = this.getConnection(connectionId);
    const handle = (payload) => this.onHandler(connectionId, methodName, handleId, payload);
    this._handles.set(handleId, handle);
    connection.on(methodName, handle);
  }

  public Off = (connectionId: string, methodName: string, handleId: string) => {
    const connection = this.getConnection(connectionId);
    const handle = this._handles.get(handleId);
    if (handle) {
      connection.off(methodName, handle);
      this._handles.delete(handleId);
    }
  }

  private async onHandler(connectionId: string, methodName: string, handleId: string, payload: any) {

    DotNet.invokeMethodAsync(assemblyName, "Dispatch", connectionId, methodName, handleId, JSON.stringify(payload));
  }

  public static initialize() {
    window["BlazorExtensions"].HubConnectionManager = new HubConnectionManager();
  }
}
