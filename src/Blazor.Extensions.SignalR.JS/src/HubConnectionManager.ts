import * as signalR from "@aspnet/signalr";
import * as sianglRMessagePack from "@aspnet/signalr-protocol-msgpack";
import { BlazorType, MethodIdentifier, TypeIdentifier } from './BlazorTypes';

export class HubConnectionManager {
  private _hubConnections: Map<string, signalR.HubConnection> = new Map<string, signalR.HubConnection>();

  public createConnection(connectionId: string, url: string, transportOptions: signalR.IHttpConnectionOptions, addMessagePack: boolean) {
    if (!connectionId) throw new Error('Invalid connectionId.');
    if (!url) throw new Error('Invalid hub url.');
    if (!transportOptions) throw new Error('Invalid transport options.');

    if (this._hubConnections[connectionId]) return;

    if (addMessagePack) {
      const connection = new signalR.HubConnectionBuilder()
        .withUrl(url, transportOptions)
        .withHubProtocol(new sianglRMessagePack.MessagePackHubProtocol())
        .build();

      this._hubConnections.set(connectionId, connection);
    } else {
      const connection = new signalR.HubConnectionBuilder()
        .withUrl(url, transportOptions)
        .build();

      this._hubConnections.set(connectionId, connection);
    }
  }

  public removeConnection(connectionId: string) {
    this._hubConnections.delete(connectionId);
  }

  public startConnection = (connectionId: string): Promise<void> => {
    const Blazor: BlazorType = window["Blazor"];
    const connection = this.getConnection(connectionId);

    connection.onclose(async err => {
      await Blazor.invokeDotNetMethodAsync(
        {
          type: {
            assembly: 'Blazor.Extensions.SignalR',
            name: 'Blazor.Extensions.HubConnectionManager'
          },
          method: {
            name: 'OnClose'
          }
        }, connectionId, JSON.stringify(err));
    });

    return connection.start();
  }

  public stopConnection = (connectionId: string): Promise<void> => {
    const connection = this.getConnection(connectionId);

    return connection.stop();
  }

  public invokeAsync = (connectionId: string, methodName: string, ...args: any[]): Promise<void> => {
    const connection = this.getConnection(connectionId);

    return connection.invoke(methodName, ...args);
  }

  public invokeWithResultAsync = (connectionId: string, methodName: string, ...args: any[]): Promise<any> => {
    const connection = this.getConnection(connectionId);

    return connection.invoke(methodName, ...args);
  }

  private getConnection = (connectionId: string) => {
    if (!connectionId) throw new Error('Invalid connectionId.');
    const connection = this._hubConnections.get(connectionId);
    if (!connection) throw new Error('Invalid connection.');

    return connection;
  }

  private on(connectionId: string, methodName: string) {
    const connection = this.getConnection(connectionId);
    connection.on(methodName, (payload) => this.onHandler(connectionId, methodName, payload));
  }

  private async onHandler(connectionId: string, methodName: string, payload: any) {
    const Blazor: BlazorType = window["Blazor"];

    await Blazor.invokeDotNetMethodAsync(
      {
        type: {
          assembly: 'Blazor.Extensions.SignalR',
          name: 'Blazor.Extensions.HubConnectionManager'
        },
        method: {
          name: 'Dispatch'
        }
      }, connectionId, methodName, payload);
  }

  public static initialize() {
    const Blazor: BlazorType = window["Blazor"];
    window["BlazorExtensions"].HubConnectionManager = new HubConnectionManager();

    Blazor.registerFunction('Blazor.Extensions.SignalR.CreateConnection',
      (connectionId: string, httpConnectionOptions: any) => {
        let options: any = {
          logger: httpConnectionOptions.logLevel,
          transport: httpConnectionOptions.transport,
          logMessageContent: httpConnectionOptions.logMessageContent,
          skipNegotiation: httpConnectionOptions.skipNegotiation
        };

        if (httpConnectionOptions.hasAccessTokenFactory) {
          options.accessTokenFactory = () => {
            return new Promise<string>(async (resolve, reject) => {
              const token = await Blazor.invokeDotNetMethodAsync<string>(
                {
                  type: {
                    assembly: 'Blazor.Extensions.SignalR',
                    name: 'Blazor.Extensions.HubConnectionManager'
                  },
                  method: {
                    name: 'GetAccessToken'
                  }
                }, connectionId);

              if (token) {
                resolve(token);
              } else {
                reject();
              }
            })
          }
        }

        window["BlazorExtensions"].HubConnectionManager.createConnection(
          connectionId,
          httpConnectionOptions.url,
          options,
          httpConnectionOptions.enableMessagePack
        );
        return true;
      }
    );

    Blazor.registerFunction('Blazor.Extensions.SignalR.RemoveConnection', (connectionId: string) => {
      return window["BlazorExtensions"].HubConnectionManager.removeConnection(connectionId);
    });

    Blazor.registerFunction('Blazor.Extensions.SignalR.StartConnection', (connectionId: string) => {
      //TODO remove this parse after Blazor fixed the async args json parsing code
      const parsedConnectionId = JSON.parse(connectionId);

      return window["BlazorExtensions"].HubConnectionManager.startConnection(parsedConnectionId);
    });

    Blazor.registerFunction('Blazor.Extensions.SignalR.StopConnection', (connectionId: string) => {
      //TODO remove this parse after Blazor fixed the async args json parsing code
      const parsedConnectionId = JSON.parse(connectionId);

      return window["BlazorExtensions"].HubConnectionManager.stopConnection(parsedConnectionId);
    });

    Blazor.registerFunction('Blazor.Extensions.SignalR.InvokeAsync', (connectionId: string, methodName: string, args: any) => {
      //TODO remove this parse after Blazor fixed the async args json parsing code
      const parsedConnectionId = JSON.parse(connectionId);
      const parsedMethodName = JSON.parse(methodName);
      const parsedArgs = JSON.parse(args);

      return window["BlazorExtensions"].HubConnectionManager.invokeAsync(parsedConnectionId, parsedMethodName, ...parsedArgs);
    });

    Blazor.registerFunction('Blazor.Extensions.SignalR.InvokeWithResultAsync', (connectionId: string, methodName: string, args: any) => {
      //TODO remove this parse after Blazor fixed the async args json parsing code
      const parsedConnectionId = JSON.parse(connectionId);
      const parsedMethodName = JSON.parse(methodName);
      const parsedArgs = JSON.parse(args);

      return window["BlazorExtensions"].HubConnectionManager.invokeWithResultAsync(parsedConnectionId, parsedMethodName, ...parsedArgs);
    });

    Blazor.registerFunction('Blazor.Extensions.SignalR.On', (connectionId: string, methodName: string) => {
      return window["BlazorExtensions"].HubConnectionManager.on(connectionId, methodName);
    });
  }
}
