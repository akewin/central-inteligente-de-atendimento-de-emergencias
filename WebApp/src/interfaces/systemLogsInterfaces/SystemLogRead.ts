import type { SystemLogData } from './SystemLogData';

export interface SystemLogRead {
  id: number;
  action: string;
  data?: SystemLogData;
  generatedBy: string;
  ipAddress?: string;
  createdAt: Date;
}
