import {
  faChartBar,
  faChartLine,
  faCogs,
  faUsers,
  faTriangleExclamation,
  faTruckMedical,
  faBell,
  faHospital,
} from '@fortawesome/free-solid-svg-icons';
import type { MenuItem } from '../interfaces';
import { PERMISSIONS } from '../permissions/tokens';

export const menuItems: MenuItem[] = [
  {
    label: 'Dashboard',
    icon: faChartLine,
    route: '/dashboard',
  },
  {
    label: 'Meus chamados',
    icon: faBell,
    route: '/my-dispatches',
    permission: PERMISSIONS.UNIT_OPERATIONS,
  },
  { label: 'Recebimentos', icon: faHospital, route: '/hospital-receptions', permission: PERMISSIONS.HOSPITAL_OPERATIONS },
  {
    label: 'Ocorrências',
    icon: faTriangleExclamation,
    route: '/occurrences',
    permission: PERMISSIONS.OCCURRENCES,
  },
  {
    label: 'Equipes',
    icon: faTruckMedical,
    route: '/units',
    permission: PERMISSIONS.UNITS_VIEW,
  },
  { label: 'Hospitais', icon: faHospital, route: '/hospitals', permission: PERMISSIONS.HOSPITALS_VIEW },
  {
    label: 'Usuários',
    icon: faUsers,
    route: '/users',
    permission: PERMISSIONS.USERS,
  },
  {
    label: 'Recursos',
    icon: faCogs,
    route: '/resources',
    permission: PERMISSIONS.RESOURCES,
  },
  {
    label: 'Relatórios',
    icon: faChartBar,
    route: '/reports',
    permission: PERMISSIONS.REPORTS,
  },
];
