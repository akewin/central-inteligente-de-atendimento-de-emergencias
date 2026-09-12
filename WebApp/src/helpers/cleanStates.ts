import type {
  PagedResponse,
  SystemLogFiltersPayload,
  SystemResource,
  UserFormValues,
} from '../interfaces';
import { PERMISSIONS, type ValidPermission } from '../permissions';

interface FormStates {
  initialPermissionsMap: Record<string, ValidPermission>;
  logsReportFilters: Omit<SystemLogFiltersPayload, 'page' | 'pageSize'>;
  systemResource: SystemResource;
  tablePagination: Omit<PagedResponse<unknown>, 'data'>;
  userForm: UserFormValues;
}

export const cleanStates: FormStates = {
  initialPermissionsMap: PERMISSIONS,
  logsReportFilters: {
    startDate: undefined,
    endDate: undefined,
    userId: undefined,
    action: '',
  },
  systemResource: {
    name: '',
    exhibitionName: '',
  },
  tablePagination: {
    totalItems: 0,
    page: 1,
    pageSize: 10,
    totalPages: 1,
  },
  userForm: {
    username: '',
    email: '',
    fullName: '',
    password: '',
    permissionIds: [],
    unitId: null,
    hospitalId: null,
  },
};
