export const PERMISSIONS = {
  ROOT: 'root',
  USERS: 'users',
  RESOURCES: 'resources',
  REPORTS: 'reports',
  OCCURRENCES: 'occurrences',
  UNITS_VIEW: 'units-view',
  UNITS_MANAGE: 'units-manage',
  UNIT_OPERATIONS: 'unit-operations',
  HOSPITALS_VIEW: 'hospitals-view',
  HOSPITALS_MANAGE: 'hospitals-manage',
  HOSPITAL_OPERATIONS: 'hospital-operations',
} as const;

export type ValidPermission = (typeof PERMISSIONS)[keyof typeof PERMISSIONS];
