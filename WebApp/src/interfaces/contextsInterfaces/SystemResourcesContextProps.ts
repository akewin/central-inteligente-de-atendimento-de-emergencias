import type { SystemResource } from '../systemResourcesInterfaces/SystemResource';
import type { SystemResourceOption } from '../systemResourcesInterfaces/SystemResourceOption';
import type { SystemResourcesPagination } from '../systemResourcesInterfaces/SystemResourcesPagination';

export interface SystemResourcesContextProps {
  resources: SystemResource[];
  pagination: Omit<SystemResourcesPagination, 'data'>;
  loading: boolean;
  error: string | null;
  fetchSystemResources: (
    page?: number,
    pageSize?: number,
    searchKey?: string
  ) => Promise<void>;
  addSystemResource: (resource: SystemResource) => Promise<void>;
  editSystemResource: (resource: SystemResource) => Promise<void>;
  removeSystemResource: (id: string) => Promise<void>;
  fetchSystemResourcesForSelect: () => Promise<SystemResourceOption[]>;
  setPagination: React.Dispatch<
    React.SetStateAction<Omit<SystemResourcesPagination, 'data'>>
  >;
}
