import { requestJson, type AuthHeaders } from './httpClient';

/**
 * Statuts du dossier remboursement (machine a etats SPEC/14):
 * 0=Draft, 1=Submitted, 2=Pending, 3=Approved, 4=Rejected,
 * 5=CorrectionRequired, 6=Corrected, 7=Completed, 8=Archived.
 */
export type ReimbursementCaseStatus = 0 | 1 | 2 | 3 | 4 | 5 | 6 | 7 | 8;

export type ReimbursementCase = {
  id: string;
  tenantId: string;
  invoiceIds: string[];
  status: ReimbursementCaseStatus;
  submissionRef: string | null;
  inamiResponse: string | null;
  createdAtUtc: string;
  updatedAtUtc: string;
  createdBy: string;
};

export function listCases(auth: AuthHeaders) {
  return requestJson<ReimbursementCase[]>('/api/reimbursement/cases', { method: 'GET' }, auth);
}

export function createCase(auth: AuthHeaders, payload: { invoiceIds: string[] }) {
  return requestJson<ReimbursementCase>('/api/reimbursement/cases', {
    method: 'POST',
    body: JSON.stringify(payload)
  }, auth);
}

export function transitionCase(auth: AuthHeaders, caseId: string, target: ReimbursementCaseStatus) {
  return requestJson<ReimbursementCase>(`/api/reimbursement/cases/${caseId}/status`, {
    method: 'POST',
    body: JSON.stringify({ target })
  }, auth);
}

export const caseStatusLabels: Record<ReimbursementCaseStatus, string> = {
  0: 'Brouillon',
  1: 'Soumis',
  2: 'En cours INAMI',
  3: 'Approuvé',
  4: 'Rejeté',
  5: 'Correction requise',
  6: 'Corrigé',
  7: 'Finalisé',
  8: 'Archivé'
};

export const caseStatusBadgeClass: Record<ReimbursementCaseStatus, string> = {
  0: 'badge badge-neutral',
  1: 'badge badge-warning',
  2: 'badge badge-warning',
  3: 'badge badge-success',
  4: 'badge badge-danger',
  5: 'badge badge-danger',
  6: 'badge badge-warning',
  7: 'badge badge-success',
  8: 'badge badge-neutral'
};

/** Transitions autorisees (mirroir de la state machine backend / SPEC/14) pour piloter les boutons UI. */
export const allowedTransitions: Record<ReimbursementCaseStatus, Array<{ target: ReimbursementCaseStatus; label: string }>> = {
  0: [{ target: 1, label: 'Soumettre' }],
  1: [{ target: 2, label: 'Marquer en cours' }],
  2: [
    { target: 3, label: 'Approuver' },
    { target: 4, label: 'Rejeter' },
    { target: 5, label: 'Correction requise' }
  ],
  3: [{ target: 7, label: 'Finaliser' }],
  4: [{ target: 7, label: 'Finaliser' }],
  5: [{ target: 6, label: 'Marquer corrigé' }],
  6: [{ target: 1, label: 'Resoumettre' }],
  7: [{ target: 8, label: 'Archiver' }],
  8: []
};
