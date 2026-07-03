import { createContext, useCallback, useContext, useEffect, useMemo, useState, type ReactNode } from 'react';

type AuthUser = {
  email: string;
  displayName: string;
  role: string;
  /** Roles cabinet RBAC (P0-006): AdminCabinet | Kine | Assistant | Billing. */
  roles: string[];
  tenantId: string;
  actorId: string;
};

type AuthContextValue = {
  isAuthenticated: boolean;
  user: AuthUser | null;
  signIn: (email: string, password: string, tenantId: string, actorId: string) => void;
  signOut: () => void;
};

const storageKey = 'qine.auth.user';
const defaultRoles = ['AdminCabinet'];

const AuthContext = createContext<AuthContextValue | null>(null);

function readUser(): AuthUser | null {
  if (typeof window === 'undefined') {
    return null;
  }

  const raw = window.localStorage.getItem(storageKey);
  if (!raw) {
    return null;
  }

  try {
    const parsed = JSON.parse(raw) as AuthUser;
    // Older stored sessions predate the RBAC roles field.
    return { ...parsed, roles: parsed.roles ?? defaultRoles };
  } catch {
    window.localStorage.removeItem(storageKey);
    return null;
  }
}

export function AuthProvider({ children }: { children: ReactNode }) {
  const [user, setUser] = useState<AuthUser | null>(() => readUser());

  useEffect(() => {
    if (typeof window === 'undefined') {
      return;
    }

    if (user) {
      window.localStorage.setItem(storageKey, JSON.stringify(user));
      return;
    }

    window.localStorage.removeItem(storageKey);
  }, [user]);

  const signIn = useCallback((email: string, _password: string, tenantId: string, actorId: string) => {
    const normalizedEmail = email.trim() || 'staff@q-ine.local';
    const normalizedTenantId = tenantId.trim() || 'tenant-demo';
    const normalizedActorId = actorId.trim() || 'staff-1';

    setUser({
      email: normalizedEmail,
      displayName: 'Sophie Vandenberghe',
      role: 'Acces demo',
      roles: defaultRoles,
      tenantId: normalizedTenantId,
      actorId: normalizedActorId
    });
  }, []);

  const signOut = useCallback(() => {
    setUser(null);
  }, []);

  const value = useMemo<AuthContextValue>(
    () => ({
      isAuthenticated: user !== null,
      user,
      signIn,
      signOut
    }),
    [signIn, signOut, user]
  );

  return <AuthContext.Provider value={value}>{children}</AuthContext.Provider>;
}

export function useAuth() {
  const context = useContext(AuthContext);
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider.');
  }

  return context;
}
