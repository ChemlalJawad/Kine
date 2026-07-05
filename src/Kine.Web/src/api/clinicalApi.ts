import { requestJson, type AuthHeaders } from './httpClient';

export type Seance = {
  id: string;
  tenantId: string;
  patientId: string;
  appointmentId: string | null;
  prescriptionId: string | null;
  dateSeanceUtc: string;
  note: string | null;
  createdAtUtc: string;
  createdBy: string;
};

export type Prescription = {
  id: string;
  tenantId: string;
  patientId: string;
  prescriberName: string;
  prescriberInami: string | null;
  prescribedAtUtc: string;
  validUntilUtc: string;
  diagnosis: string | null;
  sessionsPrescribed: number;
  createdAtUtc: string;
  createdBy: string;
};

/** Prescription enrichie de son utilisation (F-A4) telle que renvoyee par l'API. */
export type PrescriptionUsage = {
  prescription: Prescription;
  seancesUsed: number;
  seancesRemaining: number;
  isExpired: boolean;
};

export function listSeances(auth: AuthHeaders, patientId: string) {
  return requestJson<Seance[]>(`/api/clinical/patients/${patientId}/seances`, { method: 'GET' }, auth);
}

export function createSeance(
  auth: AuthHeaders,
  patientId: string,
  payload: { dateSeanceUtc: string; note: string | null; appointmentId?: string | null; prescriptionId?: string | null }
) {
  return requestJson<Seance>(`/api/clinical/patients/${patientId}/seances`, {
    method: 'POST',
    body: JSON.stringify(payload)
  }, auth);
}

export function listPrescriptions(auth: AuthHeaders, patientId: string) {
  return requestJson<PrescriptionUsage[]>(`/api/clinical/patients/${patientId}/prescriptions`, { method: 'GET' }, auth);
}

export function createPrescription(
  auth: AuthHeaders,
  patientId: string,
  payload: {
    prescriberName: string;
    prescribedAtUtc: string;
    sessionsPrescribed: number;
    prescriberInami?: string | null;
    diagnosis?: string | null;
  }
) {
  return requestJson<Prescription>(`/api/clinical/patients/${patientId}/prescriptions`, {
    method: 'POST',
    body: JSON.stringify(payload)
  }, auth);
}
