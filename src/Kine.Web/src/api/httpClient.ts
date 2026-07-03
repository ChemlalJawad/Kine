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

export async function requestJson<T>(path: string, init: RequestInit, auth: AuthHeaders): Promise<T> {
  const response = await fetch(`${apiBaseUrl}${path}`, {
    ...init,
    headers: {
      'Content-Type': 'application/json',
      'X-Tenant-Id': auth.tenantId,
      'X-Actor-Id': auth.actorId,
      'X-Roles': auth.roles?.join(',') ?? 'AdminCabinet',
      ...(init.headers ?? {})
    }
  });

  if (!response.ok) {
    throw new Error(await response.text());
  }

  if (response.status === 204) {
    return undefined as T;
  }

  return (await response.json()) as T;
}
