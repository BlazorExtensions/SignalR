declare var DotNet: DotNet; 


interface DotNet {
  invokeMethodAsync<T>(assemblyName: string | any, methodName: string, ...args: any[]): Promise<T | never>;
  invokeMethod<T>(assemblyName: string | any, methodName: string, ...args: any[]): T;
}
