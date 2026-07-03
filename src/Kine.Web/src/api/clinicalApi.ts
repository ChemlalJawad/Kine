import { requestJson, type AuthHeaders } from './httpClient';

export type Seance = {
  id: string;
  tenantId: string;
  patientId: string;
  appointmentId: string | null;
  dateSeanceUtc: string;
  note: string | null;
  createdAtUtc: string;
  createdBy: string;
};

export function listSeances(auth: AuthHeaders, patientId: string) {
  return requestJson<Seance[]>(`/api/clinical/patients/${patientId}/seances`, { method: 'GET' }, auth);
}

export function createSeance(
  auth: AuthHeaders,
  patientId: string,
  payload: { dateSeanceUtc: string; note: string | null; appointmentId?: string | null }
) {
  return requestJson<Seance>(`/api/clinical/patients/${patientId}/seances`, {
    method: 'POST',
    body: JSON.stringify(payload)
  }, auth);
}
