export interface ConnectionOperation {
  connectionId: string;
}

export interface MessagePacket extends ConnectionOperation {
  methodName: string;
  payload: any;
}
