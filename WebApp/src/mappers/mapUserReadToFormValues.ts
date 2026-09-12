import type { UserFormValues, UserRead } from "../interfaces";

export function mapUserReadToFormValues(user: UserRead): UserFormValues {
  const { id, username, email, fullName, permissions, unitId, hospitalId } = user;

    return {
    id,
    username,
    email,
    fullName,
    permissionIds: permissions.map(p => p.id),
    unitId: unitId ?? null,
    hospitalId: hospitalId ?? null,
  };
}
