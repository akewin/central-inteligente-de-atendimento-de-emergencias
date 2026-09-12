import type { IconDefinition } from '@fortawesome/fontawesome-svg-core';
import type { MenuItem } from '../MenuItem';
import type { AuthUser } from '../userInterfaces/AuthUser';
import type { ValidPermission } from '../../permissions';

export interface PermissionsContextProps {
  permissionsMap: Record<string, ValidPermission>;
  pageTitleIcons: Record<string, IconDefinition>;
  menuItems: MenuItem[];
  loading: boolean;
  error: string | null;
  getMenuItemsForUser: (authUser: AuthUser | null) => MenuItem[];
}
