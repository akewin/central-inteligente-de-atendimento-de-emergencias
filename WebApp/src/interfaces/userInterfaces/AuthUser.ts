import type { SystemResource } from '../systemResourcesInterfaces/SystemResource';

export interface AuthUser {
  id: number;
  username: string;
  fullName: string;
  permissions: SystemResource[];
  unitId?: number | null;
  unit?: { id: number; name: string; service: string } | null;
  hospitalId?: number | null;
  hospitalName?: string | null;
}
