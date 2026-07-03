import { requestJson, type AuthHeaders } from './httpClient';

export type MonthlyReport = {
  month: string;
  appointments: number;
  completed: number;
  cancelled: number;
  noShows: number;
  seances: number;
  invoicedAmount: number;
  reimbursedAmount: number;
  pendingInvoices: number;
  rejectedInvoices: number;
};

export type ReportingSummary = {
  patientsActive: number;
  patientsArchived: number;
  months: MonthlyReport[];
};

export function getSummary(auth: AuthHeaders) {
  return requestJson<ReportingSummary>('/api/reporting/summary', { method: 'GET' }, auth);
}

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? '';

/** Telecharge l'export CSV (blob) en transmettant les headers tenant/actor/roles. */
export async function downloadCsv(auth: AuthHeaders): Promise<Blob> {
  const response = await fetch(`${apiBaseUrl}/api/reporting/export.csv`, {
    headers: {
      'X-Tenant-Id': auth.tenantId,
      'X-Actor-Id': auth.actorId,
      'X-Roles': auth.roles?.join(',') ?? 'AdminCabinet'
    }
  });

  if (!response.ok) {
    throw new Error(await response.text());
  }

  return response.blob();
}
