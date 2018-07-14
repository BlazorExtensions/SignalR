// Declare types here until we've Blazor.d.ts
export interface System_Object { System_Object__DO_NOT_IMPLEMENT: any };
export interface System_String extends System_Object { System_String__DO_NOT_IMPLEMENT: any }

export interface MethodOptions {
  type: TypeIdentifier;
  method: MethodIdentifier;
}

// Keep in sync with InvocationResult.cs
export interface InvocationResult {
  succeeded: boolean;
  result?: any;
  message?: string;
}

export interface MethodIdentifier {
  name: string;
  typeArguments?: { [key: string]: TypeIdentifier }
  parameterTypes?: TypeIdentifier[];
}

export interface TypeIdentifier {
  assembly: string;
  name: string;
  typeArguments?: { [key: string]: TypeIdentifier };
}

export interface Platform {
  toJavaScriptString(dotNetString: System_String): string;
}

export type BlazorType = {
  registerFunction(identifier: string, implementation: Function),
  invokeDotNetMethodAsync<T>(methodOptions: MethodOptions, ...args: any[]): Promise<T | null>,
  platform: Platform
};
