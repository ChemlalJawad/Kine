export type PatientStatus = 0 | 1;
export type PatientContactType = 0 | 1 | 2;
export type ConsentType = 0 | 1 | 2;

export type Patient = {
  id: string;
  tenantId: string;
  firstName: string;
  lastName: string;
  dateOfBirth: string | null;
  status: PatientStatus;
  createdAtUtc: string;
  updatedAtUtc: string;
  createdBy: string;
};

export type PatientContact = {
  id: string;
  tenantId: string;
  patientId: string;
  type: PatientContactType;
  value: string;
  isPrimary: boolean;
  createdAtUtc: string;
  updatedAtUtc: string;
  createdBy: string;
};

export type PatientConsent = {
  id: string;
  tenantId: string;
  patientId: string;
  type: ConsentType;
  granted: boolean;
  grantedAtUtc: string;
  revokedAtUtc: string | null;
  createdAtUtc: string;
  updatedAtUtc: string;
  createdBy: string;
};

type AuthHeaders = {
  tenantId: string;
  actorId: string;
};

const apiBaseUrl = import.meta.env.VITE_API_BASE_URL ?? '';

async function requestJson<T>(path: string, init: RequestInit, auth: AuthHeaders): Promise<T> {
  const response = await fetch(`${apiBaseUrl}${path}`, {
    ...init,
    headers: {
      'Content-Type': 'application/json',
      'X-Tenant-Id': auth.tenantId,
      'X-Actor-Id': auth.actorId,
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

export function listPatients(auth: AuthHeaders) {
  return requestJson<Patient[]>('/api/patients', { method: 'GET' }, auth);
}

export function createPatient(
  auth: AuthHeaders,
  payload: { firstName: string; lastName: string; dateOfBirth: string | null }
) {
  return requestJson<Patient>('/api/patients', {
    method: 'POST',
    body: JSON.stringify(payload)
  }, auth);
}

export function updatePatient(
  auth: AuthHeaders,
  patientId: string,
  payload: { firstName: string; lastName: string; dateOfBirth: string | null }
) {
  return requestJson<Patient>(`/api/patients/${patientId}`, {
    method: 'PUT',
    body: JSON.stringify(payload)
  }, auth);
}

export function archivePatient(auth: AuthHeaders, patientId: string) {
  return requestJson<void>(`/api/patients/${patientId}`, { method: 'DELETE' }, auth);
}

export function listPatientContacts(auth: AuthHeaders, patientId: string) {
  return requestJson<PatientContact[]>(`/api/patients/${patientId}/contacts`, { method: 'GET' }, auth);
}

export function createPatientContact(
  auth: AuthHeaders,
  patientId: string,
  payload: { type: PatientContactType; value: string; isPrimary: boolean }
) {
  return requestJson<PatientContact>(`/api/patients/${patientId}/contacts`, {
    method: 'POST',
    body: JSON.stringify(payload)
  }, auth);
}

export function updatePatientContact(
  auth: AuthHeaders,
  patientId: string,
  contactId: string,
  payload: { value: string; isPrimary: boolean }
) {
  return requestJson<PatientContact>(`/api/patients/${patientId}/contacts/${contactId}`, {
    method: 'PUT',
    body: JSON.stringify(payload)
  }, auth);
}

export function removePatientContact(auth: AuthHeaders, patientId: string, contactId: string) {
  return requestJson<void>(`/api/patients/${patientId}/contacts/${contactId}`, { method: 'DELETE' }, auth);
}

export function listPatientConsents(auth: AuthHeaders, patientId: string) {
  return requestJson<PatientConsent[]>(`/api/patients/${patientId}/consents`, { method: 'GET' }, auth);
}

export function createPatientConsent(
  auth: AuthHeaders,
  patientId: string,
  payload: { type: ConsentType; granted: boolean }
) {
  return requestJson<PatientConsent>(`/api/patients/${patientId}/consents`, {
    method: 'POST',
    body: JSON.stringify(payload)
  }, auth);
}

export function revokePatientConsent(auth: AuthHeaders, patientId: string, consentId: string) {
  return requestJson<PatientConsent>(`/api/patients/${patientId}/consents/${consentId}/revoke`, {
    method: 'POST'
  }, auth);
}
