import type { SystemLogDataPayload } from "./SystemLogDataPayload";

interface SystemLogCreateData {
  type: 'create';
  created: SystemLogDataPayload;
}

interface SystemLogUpdateData {
  type: 'update';
  prevState: SystemLogDataPayload;
  currState: SystemLogDataPayload;
}

export type SystemLogData = SystemLogCreateData | SystemLogUpdateData;
