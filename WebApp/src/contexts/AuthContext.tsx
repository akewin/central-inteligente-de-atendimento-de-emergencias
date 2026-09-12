import {
  createContext,
  useState,
  useEffect,
  useCallback,
  type ReactNode,
} from "react";
import type {
  AuthContextProps,
  AuthUser,
  ExternalLoginPayload,
  LoginPayload,
  LoginResponse,
  PasswordResetPayload,
  RefreshResponse,
} from "../interfaces";
import {
  externalLogin,
  login,
  requestPasswordReset,
  resetPassword,
  refreshToken as refreshService,
  logout as logoutService,
} from "../services";

const AuthContext = createContext<AuthContextProps | undefined>(undefined);
export default AuthContext;

export function AuthProvider({ children }: { children: ReactNode }) {
  const [token, setToken] = useState<string | null>(
    localStorage.getItem("token"),
  );
  const [refreshToken, setRefreshToken] = useState<string | null>(
    localStorage.getItem("refreshToken"),
  );
  const [authUser, setAuthUser] = useState<AuthUser | null>(null);

  const handleAuthData = useCallback((data: LoginResponse) => {
    const userData: AuthUser = {
      id: data.id,
      username: data.username,
      fullName: data.fullName,
      permissions: data.permissions,
      unitId: data.unitId,
      unit: data.unit,
      hospitalId: data.hospitalId,
      hospitalName: data.hospitalName,
    };
    setAuthUser(userData);
    setToken(data.token);
    setRefreshToken(data.refreshToken);
    localStorage.setItem("token", data.token);
    localStorage.setItem("refreshToken", data.refreshToken);
    localStorage.setItem("authUser", JSON.stringify(userData));
  }, []);

  const handleLogin = useCallback(
    async (payload: LoginPayload) => {
      const data = await login(payload);
      handleAuthData(data);
      return data;
    },
    [handleAuthData],
  );

  const handleExternalLogin = useCallback(
    async (payload: ExternalLoginPayload) => {
      const data = await externalLogin(payload);
      handleAuthData(data);
      return data;
    },
    [handleAuthData],
  );

  const handlePasswordResetRequest = useCallback(
    async (email: string): Promise<string> => {
      return await requestPasswordReset(email);
    },
    [],
  );

  const handlePasswordReset = useCallback(
    async (payload: PasswordResetPayload): Promise<string> => {
      return await resetPassword(payload);
    },
    [],
  );

  async function handleLogout() {
    const existingToken = localStorage.getItem("token");

    if (existingToken) {
      try {
        await logoutService();
      } catch (err) {
        // ignore network errors — we'll always clear local state
        console.error("logout service failed", err);
      }
    }

    localStorage.removeItem("token");
    localStorage.removeItem("refreshToken");
    localStorage.removeItem("authUser");
    setToken(null);
    setRefreshToken(null);
    setAuthUser(null);
  }

  // keep authUser in sync with localStorage (e.g. when another tab updates)
  useEffect(() => {
    const savedUser = localStorage.getItem("authUser");
    if (savedUser) setAuthUser(JSON.parse(savedUser));

    const onRefreshed = () => {
      const newToken = localStorage.getItem("token");
      if (newToken) setToken(newToken);
      const newRefresh = localStorage.getItem("refreshToken");
      if (newRefresh) setRefreshToken(newRefresh);
    };

    window.addEventListener("tokenRefreshed", onRefreshed);
    window.addEventListener("logout", handleLogout);

    // try to refresh on startup if there's a refresh token but no active token
    const tryStartupRefresh = async () => {
      const storedRefresh = localStorage.getItem("refreshToken");
      if (storedRefresh && !token) {
        try {
          const data: RefreshResponse = await refreshService({
            refreshToken: storedRefresh,
          });
          setToken(data.token);
          setRefreshToken(data.refreshToken);
          localStorage.setItem("token", data.token);
          localStorage.setItem("refreshToken", data.refreshToken);
        } catch {
          // refresh failed; clear state so ProtectedRoute can redirect
          handleLogout();
        }
      }
    };
    tryStartupRefresh();

    return () => {
      window.removeEventListener("tokenRefreshed", onRefreshed);
      window.removeEventListener("logout", handleLogout);
    };
  }, [token, handleAuthData]);

  return (
    <AuthContext.Provider
      value={{
        token,
        refreshToken,
        authUser,
        handleLogin,
        handleExternalLogin,
        handlePasswordResetRequest,
        handlePasswordReset,
        handleLogout,
      }}
    >
      {children}
    </AuthContext.Provider>
  );
}
