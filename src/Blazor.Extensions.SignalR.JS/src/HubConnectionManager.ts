import * as signalR from "@aspnet/signalr";
import { BlazorType, MethodIdentifier, TypeIdentifier } from './BlazorTypes';

export class HubConnectionManager {
  private _hubConnections: Map<string, signalR.HubConnection> = new Map<string, signalR.HubConnection>();

  public createConnection(connectionId: string, url: string, transportOptions: signalR.IHttpConnectionOptions) {
    if (!connectionId) throw new Error('Invalid connectionId.');
    if (!url) throw new Error('Invalid hub url.');
    if (!transportOptions) throw new Error('Invalid transport options.');

    if (this._hubConnections[connectionId]) return;

    const connection = new signalR.HubConnectionBuilder()
      .withUrl(url, transportOptions)
      .build();

    this._hubConnections.set(connectionId, connection);
  }

  public removeConnection(connectionId: string) {
    this._hubConnections.delete(connectionId);
  }

  public startConnection(connectionId: string): Promise<void> {
    const connection = this.getConnection(connectionId);

    return connection.start();
  }

  public stopConnection(connectionId: string): Promise<void> {
    const connection = this.getConnection(connectionId);

    return connection.stop();
  }

  private getConnection(connectionId: string) {
    console.log(connectionId);
    console.log(this._hubConnections);
    if (!connectionId) throw new Error('Invalid connectionId.');
    if (this._hubConnections.has(connectionId)) {
      console.log(true);
    } else {
      console.log(false);
    }
    const connection = this._hubConnections.get(connectionId);
    console.log(connection);
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
          name: 'HubConnectionManager'
        },
        method: {
          name: 'Dispatch'
        }
      }, connectionId, methodName, payload);
  }

  public static initialize() {
    const Blazor: BlazorType = window["Blazor"];
    window["BlazorExtensions"].HubConnectionManager = new HubConnectionManager();

    Blazor.registerFunction('Blazor.Extensions.SignalR.CreateConnection', (connectionId: string, url: string, httpConnectionOptions: any) => {
      window["BlazorExtensions"].HubConnectionManager.createConnection(connectionId, url,
        {
          logger: httpConnectionOptions.LogLevel,
          transport: httpConnectionOptions.Transport,
          logMessageContent: httpConnectionOptions.LogMessageContent,
          skipNegotiation: httpConnectionOptions.SkipNegotiation,
          accessTokenFactory: () => httpConnectionOptions.AccessToken
        });
      return true;
    });

    Blazor.registerFunction('Blazor.Extensions.SignalR.RemoveConnection', (connectionId: string) => {
      return window["BlazorExtensions"].HubConnectionManager.removeConnection(connectionId);
    });

    Blazor.registerFunction('Blazor.Extensions.SignalR.StartConnection', (connectionId: string) => {
      return window["BlazorExtensions"].HubConnectionManager.startConnection(connectionId);
    });

    Blazor.registerFunction('Blazor.Extensions.SignalR.StopConnection', (connectionId: string) => {
      return window["BlazorExtensions"].HubConnectionManager.stopConnection(connectionId);
    });

    Blazor.registerFunction('Blazor.Extensions.SignalR.On', (connectionId: string, methodName: string) => {
      return window["BlazorExtensions"].HubConnectionManager.on(connectionId, methodName);
    });
  }
}
