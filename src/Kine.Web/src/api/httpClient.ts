/**
 * Shared fetch wrapper for all module API clients (patients, scheduling,
 * billing, clinical, reimbursement, reporting). Injects the tenant/actor
 * headers expected by TenantContextMiddleware and the X-Roles header consumed
 * by RbacMiddleware (dev/demo fallback; OIDC claims take over in prod), and
 * normalizes error handling so the per-module clients stay declarative.
 */
export type AuthHeaders = {
  tenantId: string;
  actorId: string;
  /** Roles cabinet (P0-006). Defaults to AdminCabinet for the demo user. */
  roles?: string[];
};

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? '';

const backendUnreachableMessage =
  "API injoignable : le backend Kine.Api ne repond pas. Demarre-le (run-dev.ps1 ou `dotnet run --project src/Kine.Api`) — il doit ecouter sur http://localhost:5080 (cf. vite.config.ts).";

export async function requestJson<T>(path: string, init: RequestInit, auth: AuthHeaders): Promise<T> {
  let response: Response;
  try {
    response = await fetch(`${apiBaseUrl}${path}`, {
      ...init,
      headers: {
        'Content-Type': 'application/json',
        'X-Tenant-Id': auth.tenantId,
        'X-Actor-Id': auth.actorId,
        'X-Roles': auth.roles?.join(',') ?? 'AdminCabinet',
        ...(init.headers ?? {})
      }
    });
  } catch {
    // Network-level failure (API down, no proxy).
    throw new Error(backendUnreachableMessage);
  }

  if (!response.ok) {
    const body = await response.text();
    // Vite's dev proxy answers 500 with an ECONNREFUSED-style message (or an
    // empty/HTML body) when the backend is not running: surface a clear hint
    // instead of an opaque "500".
    if (response.status === 500 && (!body || body.includes('ECONNREFUSED') || body.startsWith('<'))) {
      throw new Error(backendUnreachableMessage);
    }

    throw new Error(body || `Erreur ${response.status} sur ${path}`);
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}
