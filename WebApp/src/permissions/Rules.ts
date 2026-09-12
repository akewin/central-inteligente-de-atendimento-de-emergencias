import type { AuthUser, SystemResource, UserRead } from '../interfaces';
import { PERMISSIONS, type ValidPermission } from './tokens';
// permissions may come from full `SystemResource` objects or lightweight
// `SystemResourceOption` values returned by some endpoints.  The latter do not
// include a `name` field, so we treat that property as optional.
interface EvalPermissions {
  username: string;
  permissions: Array<SystemResource | { exhibitionName: string; id: number; name?: string }>;
}

function getUserPermissions(authUser: EvalPermissions): Set<ValidPermission> {
  return new Set(
    authUser.permissions
      .map((permission) => permission.name)
      .filter((name): name is ValidPermission => typeof name === 'string')
  );
}

export function isRootUser(authUser: EvalPermissions): boolean {
  return getUserPermissions(authUser).has(PERMISSIONS.ROOT);
}

export function hasPermission(
  authUser: EvalPermissions,
  permissionName: ValidPermission
): boolean {
  const permissions = getUserPermissions(authUser);
  if (isRootUser(authUser) || permissions.has(permissionName)) return true;
  return permissionName === PERMISSIONS.UNITS_VIEW && permissions.has(PERMISSIONS.UNITS_MANAGE);
}

export function canEditPassword(
  authUser: AuthUser,
  targetUser?: UserRead
): boolean {
  const authPermissions = getUserPermissions(authUser);
  const isUserTeam = authPermissions.has(PERMISSIONS.USERS);
  const isEditingSelf = targetUser && authUser.username === targetUser.username;
  const isTargetRoot =
    targetUser && getUserPermissions(targetUser).has(PERMISSIONS.ROOT);

  if (isRootUser(authUser)) return true;
  if (isUserTeam && !isTargetRoot) return true;
  return Boolean(isEditingSelf);
}

export function canEditPermissions(
  authUser: AuthUser,
  targetUser?: UserRead
): boolean {
  const authPermissions = getUserPermissions(authUser);
  const isUserTeam = authPermissions.has(PERMISSIONS.USERS);
  const isTargetRoot =
    targetUser && getUserPermissions(targetUser).has(PERMISSIONS.ROOT);

  if (isRootUser(authUser)) return true;
  if (isUserTeam && !isTargetRoot) return true;

  return false;
}

export function filterAssignablePermissions(
  authUser: AuthUser,
  allPermissions: SystemResource[]
): SystemResource[] {
  if (isRootUser(authUser)) return allPermissions;
  const isUserTeam = getUserPermissions(authUser).has(PERMISSIONS.USERS);
  if (isUserTeam)
    return allPermissions.filter((p) => p.name !== PERMISSIONS.ROOT);
  return [];
}
