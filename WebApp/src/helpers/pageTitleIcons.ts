import type { IconDefinition } from '@fortawesome/fontawesome-svg-core';
import type { MenuItem } from '../interfaces';

export function getPageTitleIcons(menuItems: MenuItem[]) {
  const pageTitleIcons = menuItems.reduce((acc, item) => {
    const key = item.route.replace('/', '');
    acc[key] = item.icon;
    return acc;
  }, {} as Record<string, IconDefinition>);

  return pageTitleIcons;
}
