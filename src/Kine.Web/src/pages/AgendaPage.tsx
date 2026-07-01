import { FormEvent, useEffect, useMemo, useState } from 'react';
import { useAuth } from '../auth/AuthContext';
import { listPatients, type Patient } from '../api/patientsApi';
import {
  bookAppointment,
  cancelAppointment,
  createSlot,
  listAppointments,
  listSlots,
  markAppointmentNoShow,
  type Appointment,
  type PractitionerSlot
} from '../api/schedulingApi';

const statusLabels: Record<number, string> = {
  0: 'Planifie',
  1: 'Annule',
  2: 'No-show',
  3: 'Termine'
};

type SlotDraft = {
  practitionerId: string;
  startAtUtc: string;
  endAtUtc: string;
};

type BookingDraft = {
  slotId: string;
  patientId: string;
};

function toIsoUtc(localDateTimeValue: string): string {
  return localDateTimeValue ? `${localDateTimeValue}:00Z` : '';
}

export function AgendaPage() {
  const { user } = useAuth();
  const tenantId = user?.tenantId ?? '';
  const actorId = user?.actorId ?? '';
  const auth = useMemo(() => ({ tenantId, actorId }), [tenantId, actorId]);

  const [patients, setPatients] = useState<Patient[]>([]);
  const [slots, setSlots] = useState<PractitionerSlot[]>([]);
  const [appointments, setAppointments] = useState<Appointment[]>([]);
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');
  const [slotDraft, setSlotDraft] = useState<SlotDraft>({ practitionerId: '', startAtUtc: '', endAtUtc: '' });
  const [bookingDraft, setBookingDraft] = useState<BookingDraft>({ slotId: '', patientId: '' });

  const freeSlots = slots.filter((slot) => !slot.isBooked);

  const loadAll = async () => {
    const [loadedPatients, loadedSlots, loadedAppointments] = await Promise.all([
      listPatients(auth),
      listSlots(auth),
      listAppointments(auth)
    ]);
    setPatients(loadedPatients);
    setSlots(loadedSlots);
    setAppointments(loadedAppointments);
  };

  useEffect(() => {
    void loadAll().catch((loadError) => setError(loadError.message));
  }, []);

  const patientName = (patientId: string) => {
    const patient = patients.find((item) => item.id === patientId);
    return patient ? `${patient.firstName} ${patient.lastName}` : patientId;
  };

  const handleCreateSlot = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError('');
    setMessage('');
    try {
      await createSlot(auth, {
        practitionerId: slotDraft.practitionerId,
        startAtUtc: toIsoUtc(slotDraft.startAtUtc),
        endAtUtc: toIsoUtc(slotDraft.endAtUtc)
      });
      setSlotDraft({ practitionerId: '', startAtUtc: '', endAtUtc: '' });
      await loadAll();
      setMessage('Disponibilite creee.');
    } catch (createError) {
      setError((createError as Error).message);
    }
  };

  const handleBookAppointment = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError('');
    setMessage('');
    try {
      await bookAppointment(auth, bookingDraft);
      setBookingDraft({ slotId: '', patientId: '' });
      await loadAll();
      setMessage('Rendez-vous cree.');
    } catch (bookError) {
      setError((bookError as Error).message);
    }
  };

  const handleCancel = async (appointment: Appointment) => {
    setError('');
    setMessage('');
    try {
      await cancelAppointment(auth, appointment.id);
      await loadAll();
      setMessage('Rendez-vous annule.');
    } catch (cancelError) {
      setError((cancelError as Error).message);
    }
  };

  const handleNoShow = async (appointment: Appointment) => {
    setError('');
    setMessage('');
    try {
      await markAppointmentNoShow(auth, appointment.id);
      await loadAll();
      setMessage('Rendez-vous marque no-show.');
    } catch (noShowError) {
      setError((noShowError as Error).message);
    }
  };

  return (
    <section className="patients-grid">
      <div className="panel">
        <p className="eyebrow">Module Agenda</p>
        <h3>Disponibilites</h3>
        <p className="muted">Creation de creneaux praticien (tenant: {tenantId})</p>

        <form className="stack" onSubmit={handleCreateSlot}>
          <div className="form-grid">
            <label>
              Praticien
              <input
                value={slotDraft.practitionerId}
                onChange={(event) => setSlotDraft({ ...slotDraft, practitionerId: event.target.value })}
              />
            </label>
            <label>
              Debut
              <input
                type="datetime-local"
                value={slotDraft.startAtUtc}
                onChange={(event) => setSlotDraft({ ...slotDraft, startAtUtc: event.target.value })}
              />
            </label>
            <label>
              Fin
              <input
                type="datetime-local"
                value={slotDraft.endAtUtc}
                onChange={(event) => setSlotDraft({ ...slotDraft, endAtUtc: event.target.value })}
              />
            </label>
          </div>
          <button className="primary-button" type="submit">
            Creer creneau
          </button>
        </form>

        <div className="list">
          {slots.map((slot) => (
            <div key={slot.id} className="list-item">
              <div>
                <strong>{slot.practitionerId}</strong>
                <p className="muted compact">
                  {slot.startAtUtc} → {slot.endAtUtc} · {slot.isBooked ? 'reserve' : 'libre'}
                </p>
              </div>
            </div>
          ))}
        </div>
      </div>

      <div className="panel">
        <p className="eyebrow">Rendez-vous</p>
        <h3>Creation et suivi</h3>
        <p className="muted">Parcours patient → rendez-vous</p>

        <form className="stack" onSubmit={handleBookAppointment}>
          <div className="form-grid">
            <label>
              Patient
              <select
                value={bookingDraft.patientId}
                onChange={(event) => setBookingDraft({ ...bookingDraft, patientId: event.target.value })}
              >
                <option value="">Selectionner</option>
                {patients.map((patient) => (
                  <option key={patient.id} value={patient.id}>
                    {patient.firstName} {patient.lastName}
                  </option>
                ))}
              </select>
            </label>
            <label>
              Creneau libre
              <select
                value={bookingDraft.slotId}
                onChange={(event) => setBookingDraft({ ...bookingDraft, slotId: event.target.value })}
              >
                <option value="">Selectionner</option>
                {freeSlots.map((slot) => (
                  <option key={slot.id} value={slot.id}>
                    {slot.practitionerId} · {slot.startAtUtc}
                  </option>
                ))}
              </select>
            </label>
          </div>
          <button className="primary-button" type="submit">
            Creer rendez-vous
          </button>
        </form>

        <div className="list">
          {appointments.map((appointment) => (
            <div key={appointment.id} className="list-item">
              <div>
                <strong>{patientName(appointment.patientId)}</strong>
                <p className="muted compact">
                  {appointment.practitionerId} · {appointment.startAtUtc} · {statusLabels[appointment.status]}
                </p>
              </div>
              {appointment.status === 0 ? (
                <div className="toolbar">
                  <button className="ghost-button" type="button" onClick={() => handleCancel(appointment)}>
                    Annuler
                  </button>
                  <button className="ghost-button" type="button" onClick={() => handleNoShow(appointment)}>
                    No-show
                  </button>
                </div>
              ) : null}
            </div>
          ))}
        </div>
      </div>

      <div className="panel status-panel">
        <h4>Etat</h4>
        {message ? <p className="success">{message}</p> : null}
        {error ? <p className="error">{error}</p> : null}
      </div>
    </section>
  );
}
