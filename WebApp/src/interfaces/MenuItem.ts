import type { IconDefinition } from '@fortawesome/fontawesome-svg-core';
import type { ValidPermission } from '../permissions';

export interface MenuItem {
  label: string;
  route: string;
  icon: IconDefinition;
  permission?: ValidPermission;
}
