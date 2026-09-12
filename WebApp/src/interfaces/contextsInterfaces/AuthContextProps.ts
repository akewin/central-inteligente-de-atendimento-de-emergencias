import type { ExternalLoginPayload } from '../authInterfaces/ExternalLoginPayload';
import type { LoginPayload } from '../authInterfaces/LoginPayload';
import type { PasswordResetPayload } from '../authInterfaces/PasswordResetPayload';
import type { AuthUser } from '../userInterfaces/AuthUser';
import type { LoginResponse } from '../authInterfaces/LoginResponse';

export interface AuthContextProps {
  token: string | null;
  refreshToken: string | null;
  authUser: AuthUser | null;
  handleLogin: (payload: LoginPayload) => Promise<LoginResponse>;
  handleExternalLogin: (payload: ExternalLoginPayload) => Promise<LoginResponse>;
  handlePasswordResetRequest: (email: string) => Promise<string>;
  handlePasswordReset: (payload: PasswordResetPayload) => Promise<string>;
  handleLogout: () => Promise<void>;
}
