import { Navigate } from "react-router-dom";
import { useAuth } from "../hooks";
import { hasPermission, isRootUser } from "../permissions/Rules";
import type { JSX } from "react";
import type { ValidPermission } from "../permissions/tokens";

function getTokenUnitId(token: string | null): number | null {
  if (!token) return null;

  try {
    const payload = token.split(".")[1];
    if (!payload) return null;
    const decoded = atob(payload.replace(/-/g, "+").replace(/_/g, "/"));
    const unitId = JSON.parse(decoded).unitId;
    return Number.isInteger(Number(unitId)) ? Number(unitId) : null;
  } catch {
    return null;
  }
}

interface ProtectedRouteProps {
  children: JSX.Element;
  requiredPermission?: ValidPermission;
}

export default function ProtectedRoute({
  children,
  requiredPermission,
}: ProtectedRouteProps) {
  const { authUser, token } = useAuth();

  if (!authUser) {
    return <Navigate to="/login" replace />;
  }

  if (!requiredPermission) {
    return children;
  }

  // Operations must always execute in the context of a real unit. Root access
  // grants administration capabilities, but cannot impersonate an emergency team.
  if (requiredPermission === "unit-operations" && !authUser.unitId) {
    return <Navigate to="/unauthorized" replace />;
  }

  if (requiredPermission === "unit-operations" && getTokenUnitId(token) !== authUser.unitId) {
    return <Navigate to="/login" replace />;
  }

  if (isRootUser(authUser) || hasPermission(authUser, requiredPermission)) {
    return children;
  }

  return <Navigate to="/unauthorized" replace />;
}
