import type { SystemResourceOption } from '../systemResourcesInterfaces/SystemResourceOption';

export interface UserRead {
  id: number;
  username: string;
  email: string;
  fullName: string;
  permissions: SystemResourceOption[];
  unitId?: number | null;
  unit?: { id: number; name: string; service: string } | null;
  hospitalId?: number | null;
  hospitalName?: string | null;
}
