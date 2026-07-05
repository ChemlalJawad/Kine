import { FormEvent, useEffect, useMemo, useState } from 'react';
import { useAuth } from '../auth/AuthContext';
import { DateTimeField } from '../components/DateTimeField';
import { Modal } from '../components/Modal';
import { appointmentStatusBadgeClass, appointmentStatusLabels } from '../data/appointmentStatus';
import { listPatients, type Patient } from '../api/patientsApi';
import {
  bookAppointment,
  cancelAppointment,
  createPractitioner,
  createSlot,
  listAppointments,
  listPractitioners,
  listSlots,
  markAppointmentNoShow,
  type Appointment,
  type Practitioner,
  type PractitionerSlot
} from '../api/schedulingApi';

type SlotDraft = {
  practitionerId: string;
  startAt: Date | null;
  endAt: Date | null;
};

type BookingDraft = {
  slotId: string;
  patientId: string;
};

type ScheduleRow = {
  slot: PractitionerSlot;
  appointment: Appointment | null;
};

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
  const [practitioners, setPractitioners] = useState<Practitioner[]>([]);
  const [slots, setSlots] = useState<PractitionerSlot[]>([]);
  const [appointments, setAppointments] = useState<Appointment[]>([]);
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');
  const [showSlotModal, setShowSlotModal] = useState(false);
  const [showBookingModal, setShowBookingModal] = useState(false);
  const [showPractitionerModal, setShowPractitionerModal] = useState(false);
  const [slotDraft, setSlotDraft] = useState<SlotDraft>({ practitionerId: '', startAt: null, endAt: null });
  const [bookingDraft, setBookingDraft] = useState<BookingDraft>({ slotId: '', patientId: '' });
  const [practitionerDraft, setPractitionerDraft] = useState({ firstName: '', lastName: '', inamiNumber: '' });
  // F-B2 : filtre agenda par praticien ('' = tous les praticiens).
  const [practitionerFilter, setPractitionerFilter] = useState('');

  const freeSlots = slots.filter((slot) => !slot.isBooked);

  const scheduleRows: ScheduleRow[] = useMemo(() => {
    const visibleSlots = practitionerFilter
      ? slots.filter((slot) => slot.practitionerId === practitionerFilter)
      : slots;
    const sortedSlots = [...visibleSlots].sort((a, b) => a.startAtUtc.localeCompare(b.startAtUtc));
    return sortedSlots.map((slot) => {
      const slotAppointments = appointments
        .filter((appointment) => appointment.slotId === slot.id)
        .sort((a, b) => b.createdAtUtc.localeCompare(a.createdAtUtc));
      return { slot, appointment: slotAppointments[0] ?? null };
    });
  }, [slots, appointments, practitionerFilter]);

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
    const [loadedPatients, loadedPractitioners, loadedSlots, loadedAppointments] = await Promise.all([
      listPatients(auth),
      listPractitioners(auth),
      listSlots(auth),
      listAppointments(auth)
    ]);
    setPatients(loadedPatients);
    setPractitioners(loadedPractitioners);
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

  const practitionerName = (practitionerId: string) => {
    const practitioner = practitioners.find((item) => item.id === practitionerId);
    return practitioner ? `${practitioner.firstName} ${practitioner.lastName}` : practitionerId;
  };

  const handleCreatePractitioner = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError('');
    setMessage('');
    try {
      await createPractitioner(auth, {
        firstName: practitionerDraft.firstName,
        lastName: practitionerDraft.lastName,
        inamiNumber: practitionerDraft.inamiNumber.trim() || null
      });
      setPractitionerDraft({ firstName: '', lastName: '', inamiNumber: '' });
      setShowPractitionerModal(false);
      await loadAll();
      setMessage('Praticien ajoute.');
    } catch (createError) {
      setError((createError as Error).message);
    }
  };

  const handleCreateSlot = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!slotDraft.startAt || !slotDraft.endAt) {
      return;
    }

    setError('');
    setMessage('');
    try {
      // toISOString convertit l'heure locale choisie en vrai UTC (l'ancien
      // format "YYYY-MM-DDTHH:mm" + "Z" naif decalait l'heure affichee).
      await createSlot(auth, {
        practitionerId: slotDraft.practitionerId,
        startAtUtc: slotDraft.startAt.toISOString(),
        endAtUtc: slotDraft.endAt.toISOString()
      });
      setSlotDraft({ practitionerId: '', startAt: null, endAt: null });
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
            <select
              value={practitionerFilter}
              onChange={(event) => setPractitionerFilter(event.target.value)}
              aria-label="Filtrer par praticien"
            >
              <option value="">Tous les praticiens</option>
              {practitioners.map((practitioner) => (
                <option key={practitioner.id} value={practitioner.id}>
                  {practitioner.firstName} {practitioner.lastName}
                </option>
              ))}
            </select>
            <button className="ghost-button" type="button" onClick={() => setShowPractitionerModal(true)}>
              + Praticien
            </button>
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
                            ? `${practitionerName(slot.practitionerId)} · jusqu'a ${formatTime(slot.endAtUtc)}`
                            : `Creneau libre (${practitionerName(slot.practitionerId)}) · jusqu'a ${formatTime(slot.endAtUtc)}`}
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
                <select
                  value={slotDraft.practitionerId}
                  onChange={(event) => setSlotDraft({ ...slotDraft, practitionerId: event.target.value })}
                >
                  <option value="">Selectionner</option>
                  {practitioners.map((practitioner) => (
                    <option key={practitioner.id} value={practitioner.id}>
                      {practitioner.firstName} {practitioner.lastName}
                    </option>
                  ))}
                </select>
              </label>
              <label>
                Debut
                <DateTimeField
                  value={slotDraft.startAt}
                  onChange={(startAt) =>
                    setSlotDraft((current) => ({
                      ...current,
                      startAt,
                      // Pre-remplit la fin a +30 min (duree seance kine standard) si vide.
                      endAt:
                        current.endAt ?? (startAt ? new Date(startAt.getTime() + 30 * 60 * 1000) : null)
                    }))
                  }
                  showTime
                  minDate={new Date()}
                />
              </label>
              <label>
                Fin
                <DateTimeField
                  value={slotDraft.endAt}
                  onChange={(endAt) => setSlotDraft({ ...slotDraft, endAt })}
                  showTime
                  minDate={slotDraft.startAt ?? new Date()}
                />
              </label>
            </div>
            <button
              className="primary-button is-create"
              type="submit"
              disabled={
                !slotDraft.practitionerId.trim() ||
                !slotDraft.startAt ||
                !slotDraft.endAt ||
                slotDraft.endAt <= slotDraft.startAt
              }
            >
              Creer creneau
            </button>
          </form>
        </Modal>
      ) : null}

      {showPractitionerModal ? (
        <Modal title="Nouveau praticien" onClose={() => setShowPractitionerModal(false)}>
          <form className="stack" onSubmit={handleCreatePractitioner}>
            <div className="form-grid">
              <label>
                Prenom
                <input
                  value={practitionerDraft.firstName}
                  onChange={(event) => setPractitionerDraft({ ...practitionerDraft, firstName: event.target.value })}
                />
              </label>
              <label>
                Nom
                <input
                  value={practitionerDraft.lastName}
                  onChange={(event) => setPractitionerDraft({ ...practitionerDraft, lastName: event.target.value })}
                />
              </label>
              <label>
                Numero INAMI (optionnel)
                <input
                  value={practitionerDraft.inamiNumber}
                  onChange={(event) => setPractitionerDraft({ ...practitionerDraft, inamiNumber: event.target.value })}
                />
              </label>
            </div>
            <button
              className="primary-button is-create"
              type="submit"
              disabled={!practitionerDraft.firstName.trim() || !practitionerDraft.lastName.trim()}
            >
              Ajouter praticien
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
                      {practitionerName(slot.practitionerId)} · {formatDay(slot.startAtUtc)} {formatTime(slot.startAtUtc)}
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
