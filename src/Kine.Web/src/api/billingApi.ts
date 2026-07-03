import { requestJson, type AuthHeaders } from './httpClient';

/** 0=Pending (en attente), 1=Reimbursed (rembourse), 2=Rejected (rejete). */
export type InvoiceStatus = 0 | 1 | 2;

export type Invoice = {
  id: string;
  tenantId: string;
  patientId: string;
  codeInami: string;
  label: string;
  amount: number;
  mutuelle: string | null;
  status: InvoiceStatus;
  createdAtUtc: string;
  updatedAtUtc: string;
  createdBy: string;
};

export type ActeInami = {
  code: string;
  label: string;
  amount: number;
};

export function listActes(auth: AuthHeaders) {
  return requestJson<ActeInami[]>('/api/billing/actes', { method: 'GET' }, auth);
}

export function listInvoices(auth: AuthHeaders) {
  return requestJson<Invoice[]>('/api/billing/invoices', { method: 'GET' }, auth);
}

export function createInvoice(
  auth: AuthHeaders,
  payload: { patientId: string; codeInami: string; mutuelle: string | null }
) {
  return requestJson<Invoice>('/api/billing/invoices', {
    method: 'POST',
    body: JSON.stringify(payload)
  }, auth);
}

export function markInvoiceReimbursed(auth: AuthHeaders, invoiceId: string) {
  return requestJson<Invoice>(`/api/billing/invoices/${invoiceId}/mark-reimbursed`, { method: 'POST' }, auth);
}

export function markInvoiceRejected(auth: AuthHeaders, invoiceId: string) {
  return requestJson<Invoice>(`/api/billing/invoices/${invoiceId}/mark-rejected`, { method: 'POST' }, auth);
}

export const invoiceStatusLabels: Record<InvoiceStatus, string> = {
  0: 'En attente',
  1: 'Remboursé',
  2: 'Rejeté'
};

export const invoiceStatusBadgeClass: Record<InvoiceStatus, string> = {
  0: 'badge badge-warning',
  1: 'badge badge-success',
  2: 'badge badge-danger'
};

/** Format EUR "23,45€" comme dans le mockup. */
export function formatMontant(value: number): string {
  return `${value.toFixed(2).replace('.', ',')}€`;
}
