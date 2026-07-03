import { FormEvent, useEffect, useMemo, useState } from 'react';
import { useAuth } from '../auth/AuthContext';
import { Modal } from '../components/Modal';
import { appointmentStatusBadgeClass, appointmentStatusLabels } from '../data/appointmentStatus';
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

type SlotDraft = {
  practitionerId: string;
  startAtUtc: string;
  endAtUtc: string;
};

type BookingDraft = {
  slotId: string;
  patientId: string;
};

type ScheduleRow = {
  slot: PractitionerSlot;
  appointment: Appointment | null;
};

function toIsoUtc(localDateTimeValue: string): string {
  return localDateTimeValue ? `${localDateTimeValue}:00Z` : '';
}

function formatTime(value: string): string {
  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? value : date.toLocaleTimeString('fr-BE', { hour: '2-digit', minute: '2-digit' });
}

function formatDay(value: string): string {
  const date = new Date(value);
  return Number.isNaN(date.getTime())
    ? value
    : date.toLocaleDateString('fr-BE', { weekday: 'short', day: '2-digit', month: '2-digit' });
}

/** En-tete de journee facon mockup : "Mardi 1 juillet 2026". */
function formatFullDay(value: string): string {
  const date = new Date(value);
  if (Number.isNaN(date.getTime())) {
    return value;
  }

  const formatted = date.toLocaleDateString('fr-BE', {
    weekday: 'long',
    day: 'numeric',
    month: 'long',
    year: 'numeric'
  });
  return formatted.charAt(0).toUpperCase() + formatted.slice(1);
}

function dayKey(value: string): string {
  return value.slice(0, 10);
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
  const [showSlotModal, setShowSlotModal] = useState(false);
  const [showBookingModal, setShowBookingModal] = useState(false);
  const [slotDraft, setSlotDraft] = useState<SlotDraft>({ practitionerId: '', startAtUtc: '', endAtUtc: '' });
  const [bookingDraft, setBookingDraft] = useState<BookingDraft>({ slotId: '', patientId: '' });

  const freeSlots = slots.filter((slot) => !slot.isBooked);

  const scheduleRows: ScheduleRow[] = useMemo(() => {
    const sortedSlots = [...slots].sort((a, b) => a.startAtUtc.localeCompare(b.startAtUtc));
    return sortedSlots.map((slot) => {
      const slotAppointments = appointments
        .filter((appointment) => appointment.slotId === slot.id)
        .sort((a, b) => b.createdAtUtc.localeCompare(a.createdAtUtc));
      return { slot, appointment: slotAppointments[0] ?? null };
    });
  }, [slots, appointments]);

  // Vue journee (mockup 1b) : creneaux regroupes par jour, en-tete "Mardi 1 juillet 2026".
  const rowsByDay = useMemo(() => {
    const groups = new Map<string, ScheduleRow[]>();
    for (const row of scheduleRows) {
      const key = dayKey(row.slot.startAtUtc);
      const group = groups.get(key);
      if (group) {
        group.push(row);
      } else {
        groups.set(key, [row]);
      }
    }
    return [...groups.entries()];
  }, [scheduleRows]);

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
    // eslint-disable-next-line react-hooks/exhaustive-deps -- loadAll est recreee a chaque rendu; auth suffit
  }, [auth]);

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
      setShowSlotModal(false);
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
      setShowBookingModal(false);
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
    <section className="stack">
      {error ? <p className="error">{error}</p> : null}
      {message ? <p className="success">{message}</p> : null}

      <div className="panel">
        <div className="dossier-header">
          <div>
            <p className="eyebrow">Agenda</p>
            <h3 style={{ margin: '4px 0 0' }}>Planning ({scheduleRows.length} creneaux)</h3>
          </div>
          <div className="toolbar">
            <button className="ghost-button" type="button" onClick={() => setShowSlotModal(true)}>
              + Nouveau creneau
            </button>
            <button className="primary-button is-create" type="button" onClick={() => setShowBookingModal(true)}>
              Nouveau rendez-vous
            </button>
          </div>
        </div>
        <p className="muted">Contexte tenant: {tenantId}</p>

        {scheduleRows.length === 0 ? (
          <p className="muted">Aucun creneau pour le moment. Cree un creneau pour commencer.</p>
        ) : (
          <div className="stack">
            {rowsByDay.map(([key, rows]) => (
              <section key={key} className="agenda-day">
                <h4 className="agenda-day-title">{formatFullDay(rows[0].slot.startAtUtc)}</h4>
                <div className="list">
                  {rows.map(({ slot, appointment }) => (
                    <div key={slot.id} className="list-item">
                      <span className="agenda-time">{formatTime(slot.startAtUtc)}</span>
                      <div style={{ flex: 1 }}>
                        <strong>{appointment ? patientName(appointment.patientId) : '—'}</strong>
                        <p className="muted compact">
                          {appointment
                            ? `${slot.practitionerId} · jusqu'a ${formatTime(slot.endAtUtc)}`
                            : `Creneau libre · jusqu'a ${formatTime(slot.endAtUtc)}`}
                        </p>
                      </div>
                      <span
                        className={appointment ? appointmentStatusBadgeClass[appointment.status] : 'badge badge-neutral'}
                      >
                        {appointment ? appointmentStatusLabels[appointment.status] : 'Libre'}
                      </span>
                      {appointment && appointment.status === 0 ? (
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
              </section>
            ))}
          </div>
        )}
      </div>

      {showSlotModal ? (
        <Modal title="Nouveau creneau" onClose={() => setShowSlotModal(false)}>
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
            <button
              className="primary-button is-create"
              type="submit"
              disabled={!slotDraft.practitionerId.trim() || !slotDraft.startAtUtc || !slotDraft.endAtUtc}
            >
              Creer creneau
            </button>
          </form>
        </Modal>
      ) : null}

      {showBookingModal ? (
        <Modal title="Nouveau rendez-vous" onClose={() => setShowBookingModal(false)}>
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
                      {slot.practitionerId} · {formatDay(slot.startAtUtc)} {formatTime(slot.startAtUtc)}
                    </option>
                  ))}
                </select>
              </label>
            </div>
            <button
              className="primary-button is-create"
              type="submit"
              disabled={!bookingDraft.patientId || !bookingDraft.slotId}
            >
              Creer rendez-vous
            </button>
          </form>
        </Modal>
      ) : null}
    </section>
  );
}
