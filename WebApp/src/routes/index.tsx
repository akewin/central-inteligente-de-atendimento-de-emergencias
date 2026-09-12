import { createBrowserRouter } from "react-router-dom";
import ProtectedRoute from "./ProtectedRoute";
import {
  DashBoard,
  Login,
  NotFound,
  PasswordReset,
  Reports,
  Resources,
  UnauthorizedAccess,
  Users,
  OccurrenceDetails,
  Occurrences,
  NewOccurrence,
  Units,
  MyDispatches,
  Hospitals,
  HospitalReceptions,
} from "../pages";
import { CleanLayout, DefaultLayout } from "../layouts";
import {
  PermissionsProvider,
  SystemResourcesProvider,
  UsersProvider,
  OccurrencesProvider,
} from "../contexts";
import { PERMISSIONS } from "../permissions";

const publicRoutes = [
  { path: "/login", element: <Login /> },
  { path: "/password-reset", element: <PasswordReset /> },
];

const privateRoutes = [
  { path: "/hospital-receptions", element: <HospitalReceptions />, requiredPermission: PERMISSIONS.HOSPITAL_OPERATIONS },
  {
    path: "/my-dispatches",
    element: <MyDispatches />,
    requiredPermission: PERMISSIONS.UNIT_OPERATIONS,
  },
  {
    path: "/dashboard",
    element: <DashBoard />,
  },
  {
    path: "/occurrences",
    element: (
      <OccurrencesProvider>
        <Occurrences />
      </OccurrencesProvider>
    ),
    requiredPermission: PERMISSIONS.OCCURRENCES,
  },
  {
    path: "/occurrences/new",
    element: <NewOccurrence />,
    requiredPermission: PERMISSIONS.OCCURRENCES,
  },
  {
    path: "/occurrences/:id",
    element: <OccurrenceDetails />,
    requiredPermission: PERMISSIONS.OCCURRENCES,
  },
  {
    path: "/units",
    element: <Units />,
    requiredPermission: PERMISSIONS.UNITS_VIEW,
  },
  { path: "/hospitals", element: <Hospitals />, requiredPermission: PERMISSIONS.HOSPITALS_VIEW },
  {
    path: "/users",
    element: (
      <UsersProvider>
        <Users />
      </UsersProvider>
    ),
    requiredPermission: PERMISSIONS.USERS,
  },
  {
    path: "/resources",
    element: (
      <SystemResourcesProvider>
        <Resources />
      </SystemResourcesProvider>
    ),
    requiredPermission: PERMISSIONS.RESOURCES,
  },
  {
    path: "/reports",
    element: <Reports />,
    requiredPermission: PERMISSIONS.REPORTS,
  },
];

const protectedRoutes = privateRoutes.map((route) => ({
  path: route.path,
  element: (
    <ProtectedRoute requiredPermission={route.requiredPermission}>
      {route.element}
    </ProtectedRoute>
  ),
}));

const router = createBrowserRouter([
  {
    element: <CleanLayout />,
    children: [
      { path: "/", element: <Login /> },
      ...publicRoutes,
      { path: "/unauthorized", element: <UnauthorizedAccess /> },
      { path: "*", element: <NotFound /> },
    ],
  },

  {
    element: (
      <PermissionsProvider>
        <DefaultLayout />
      </PermissionsProvider>
    ),
    children: protectedRoutes,
  },
]);

export default router;
