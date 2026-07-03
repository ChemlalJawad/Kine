import { requestJson, type AuthHeaders } from './httpClient';

export type AppointmentStatus = 0 | 1 | 2 | 3;

export type PractitionerSlot = {
  id: string;
  tenantId: string;
  practitionerId: string;
  startAtUtc: string;
  endAtUtc: string;
  isBooked: boolean;
  createdAtUtc: string;
  updatedAtUtc: string;
  createdBy: string;
};

export type Appointment = {
  id: string;
  tenantId: string;
  patientId: string;
  practitionerId: string;
  slotId: string;
  startAtUtc: string;
  endAtUtc: string;
  status: AppointmentStatus;
  createdAtUtc: string;
  updatedAtUtc: string;
  createdBy: string;
};

export function listSlots(auth: AuthHeaders) {
  return requestJson<PractitionerSlot[]>('/api/scheduling/slots', { method: 'GET' }, auth);
}

export function createSlot(
  auth: AuthHeaders,
  payload: { practitionerId: string; startAtUtc: string; endAtUtc: string }
) {
  return requestJson<PractitionerSlot>('/api/scheduling/slots', {
    method: 'POST',
    body: JSON.stringify(payload)
  }, auth);
}

export function listAppointments(auth: AuthHeaders) {
  return requestJson<Appointment[]>('/api/scheduling/appointments', { method: 'GET' }, auth);
}

export function bookAppointment(auth: AuthHeaders, payload: { slotId: string; patientId: string }) {
  return requestJson<Appointment>('/api/scheduling/appointments', {
    method: 'POST',
    body: JSON.stringify(payload)
  }, auth);
}

export function cancelAppointment(auth: AuthHeaders, appointmentId: string) {
  return requestJson<Appointment>(`/api/scheduling/appointments/${appointmentId}/cancel`, {
    method: 'POST'
  }, auth);
}

export function markAppointmentNoShow(auth: AuthHeaders, appointmentId: string) {
  return requestJson<Appointment>(`/api/scheduling/appointments/${appointmentId}/no-show`, {
    method: 'POST'
  }, auth);
}
