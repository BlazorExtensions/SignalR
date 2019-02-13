import * as signalR from "@aspnet/signalr";
import * as signalRMessagePack from "@aspnet/signalr-protocol-msgpack";
import { HttpTransportType, LogLevel } from "@aspnet/signalr";

type DotNetType = {
  invokeMethod<T>(assemblyName: string, methodIdentifier: string, ...args: any[]): T,
  invokeMethodAsync<T>(assemblyName: string, methodIdentifier: string, ...args: any[]): Promise<T>
}

type DotNetReferenceType = {
  invokeMethod<T>(methodIdentifier: string, ...args: any[]): T,
  invokeMethodAsync<T>(methodIdentifier: string, ...args: any[]): Promise<T>
}

export class HubConnectionManager {
  private _hubConnections: Map<string, signalR.HubConnection> = new Map<string, signalR.HubConnection>();
  private _handles: Map<string, (payload: any) => Promise<void>> = new Map<string, (payload: any) => Promise<void>>();

  public CreateConnection = (connectionId: string, httpConnectionOptions: DotNetReferenceType) => {
    if (!connectionId) throw new Error('Invalid connectionId.');
    if (!httpConnectionOptions) throw new Error('Invalid transport options.');

    const url = httpConnectionOptions.invokeMethod<string>('get_Url');

    if (!url) throw new Error('Invalid hub url.');

    let options: any = {
      logger: httpConnectionOptions.invokeMethod<LogLevel>('get_LogLevel'),
      transport: httpConnectionOptions.invokeMethod<HttpTransportType>('get_Transport'),
      logMessageContent: httpConnectionOptions.invokeMethod<boolean>('get_LogMessageContent'),
      skipNegotiation: httpConnectionOptions.invokeMethod<boolean>('get_SkipNegotiation')
    };

    if (httpConnectionOptions.invokeMethod<true>('HasAccessTokenFactory')) {
      options.accessTokenFactory = () => {
        return new Promise<string>(async (resolve, reject) => {
          const token = await httpConnectionOptions.invokeMethodAsync<string>('GetAccessToken');
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
      .withUrl(url, options);

    if (httpConnectionOptions.invokeMethod<true>('get_EnableMessagePack')) {
      connectionBuilder
        .withHubProtocol(new signalRMessagePack.MessagePackHubProtocol());
    }

    this._hubConnections.set(connectionId, connectionBuilder.build());
  }

  public RemoveConnection = (connectionId: string) => {
    this._hubConnections.delete(connectionId);
  }

  public StartConnection = (connectionId: string): Promise<void> => {
    const connection = this.GetConnection(connectionId);

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

  public InvokeWithResultAsync = async (connectionId: string, methodName: string, args: any[]): Promise<any> => {
    const connection = this.GetConnection(connectionId);

    var result = await connection.invoke(methodName, ...args);

    return this.ReplaceTypedArray(result);
  }

  private GetConnection = (connectionId: string) => {
    if (!connectionId) throw new Error('Invalid connectionId.');

    const connection = this._hubConnections.get(connectionId);

    if (!connection) throw new Error('Invalid connection.');

    return connection;
  }

  private ReplaceTypedArray = (obj: any): any => {
    if (obj instanceof Int8Array ||
        obj instanceof Uint8Array ||
        obj instanceof Uint8ClampedArray ||
        obj instanceof Int16Array ||
        obj instanceof Uint16Array ||
        obj instanceof Int32Array ||
        obj instanceof Uint32Array ||
        obj instanceof Float32Array ||
        obj instanceof Float64Array)
    {
      obj = Array.prototype.slice.call(obj);
    }

    return obj;
  }

  public On = (connectionId: string, callback: DotNetReferenceType) => {
    const connection = this.GetConnection(connectionId);
    const handle = (...payloads) => callback.invokeMethodAsync<void>(
      'On', payloads.map(payload => JSON.stringify(this.ReplaceTypedArray(payload))));

    this._handles.set(callback.invokeMethod<string>('get_Id'), handle);

    connection.on(callback.invokeMethod<string>('get_MethodName'), handle);
  }

  public Off = (connectionId: string, methodName: string, handleId: string) => {
    const connection = this.GetConnection(connectionId);
    const handle = this._handles.get(handleId);

    if (handle) {
      connection.off(methodName, handle);

      this._handles.delete(handleId);
    }
  }

  public OnClose = (connectionId: string, onErrorCallback: DotNetReferenceType) => {
    const connection = this.GetConnection(connectionId);

    connection.onclose(async err => {
      onErrorCallback.invokeMethodAsync<void>('OnClose', JSON.stringify(err));
    });
  }
}
