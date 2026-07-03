import { FormEvent, useEffect, useMemo, useState } from 'react';
import { useAuth } from '../auth/AuthContext';
import { Modal } from '../components/Modal';
import {
  archivePatient,
  createPatient,
  createPatientConsent,
  createPatientContact,
  listPatients,
  listPatientConsents,
  listPatientContacts,
  revokePatientConsent,
  updatePatient,
  updatePatientContact,
  removePatientContact,
  type ConsentType,
  type Patient,
  type PatientContact,
  type PatientContactType,
  type PatientConsent
} from '../api/patientsApi';
import { listAppointments, type Appointment } from '../api/schedulingApi';
import { createSeance, listSeances, type Seance } from '../api/clinicalApi';

const phoneContactType: PatientContactType = 0;
const completedAppointmentStatus = 3;

const contactTypeOptions: Array<{ value: PatientContactType; label: string }> = [
  { value: 0, label: 'Telephone' },
  { value: 1, label: 'Email' },
  { value: 2, label: 'Adresse' }
];

const consentTypeOptions: Array<{ value: ConsentType; label: string }> = [
  { value: 0, label: 'Traitement données' },
  { value: 1, label: 'Partage dossier' },
  { value: 2, label: 'Communication' }
];

type Draft = {
  firstName: string;
  lastName: string;
  dateOfBirth: string;
  mutuelle: string;
  diagnosis: string;
};

function formatDate(value: string | null | undefined): string {
  if (!value) {
    return '—';
  }

  const date = new Date(value);
  return Number.isNaN(date.getTime()) ? value : date.toLocaleDateString('fr-BE');
}

function toLocalDateTimeInput(date: Date): string {
  const pad = (n: number) => String(n).padStart(2, '0');
  return `${date.getFullYear()}-${pad(date.getMonth() + 1)}-${pad(date.getDate())}T${pad(date.getHours())}:${pad(date.getMinutes())}`;
}

export function PatientsPage() {
  const { user } = useAuth();
  const tenantId = user?.tenantId ?? '';
  const actorId = user?.actorId ?? '';
  const roles = user?.roles;
  const auth = useMemo(() => ({ tenantId, actorId, roles }), [tenantId, actorId, roles]);
  const [patients, setPatients] = useState<Patient[]>([]);
  const [appointments, setAppointments] = useState<Appointment[]>([]);
  const [selectedPatientId, setSelectedPatientId] = useState<string>('');
  const [contacts, setContacts] = useState<PatientContact[]>([]);
  const [consents, setConsents] = useState<PatientConsent[]>([]);
  const [seances, setSeances] = useState<Seance[]>([]);
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [showEditModal, setShowEditModal] = useState(false);
  const [showSeanceModal, setShowSeanceModal] = useState(false);
  const [draft, setDraft] = useState<Draft>({ firstName: '', lastName: '', dateOfBirth: '', mutuelle: '', diagnosis: '' });
  const [contactDraft, setContactDraft] = useState({ type: 0 as PatientContactType, value: '', isPrimary: false });
  const [consentDraft, setConsentDraft] = useState({ type: 0 as ConsentType, granted: true });
  const [seanceDraft, setSeanceDraft] = useState({ dateSeance: toLocalDateTimeInput(new Date()), note: '' });

  const selectedPatient = patients.find((patient) => patient.id === selectedPatientId) ?? null;

  // Telephone et derniere seance sont derives des vraies donnees (contacts /
  // rendez-vous Termine), jamais stockes en doublon. Depuis Q-B20 (tranche), le
  // nombre de seances effectuees est lui aussi derive: c'est le nombre de
  // SeanceClinique reelles du module Clinical, plus un compteur manuel.
  const selectedPatientPhone = (() => {
    const phoneContacts = contacts.filter((contact) => contact.type === phoneContactType);
    const primary = phoneContacts.find((contact) => contact.isPrimary);
    return (primary ?? phoneContacts[0])?.value ?? null;
  })();

  const selectedPatientLastSession = (() => {
    if (seances.length > 0) {
      return seances[0].dateSeanceUtc;
    }

    if (!selectedPatient) {
      return null;
    }

    return (
      appointments
        .filter(
          (appointment) =>
            appointment.patientId === selectedPatient.id && appointment.status === completedAppointmentStatus
        )
        .sort((a, b) => b.startAtUtc.localeCompare(a.startAtUtc))[0]?.startAtUtc ?? null
    );
  })();

  const sessionsDone = seances.length;

  const loadPatients = async () => {
    const items = await listPatients(auth);
    setPatients(items);
    if (!selectedPatientId && items.length > 0) {
      setSelectedPatientId(items[0].id);
    }
  };

  const loadDetails = async (patientId: string) => {
    const [loadedContacts, loadedConsents, loadedSeances] = await Promise.all([
      listPatientContacts(auth, patientId),
      listPatientConsents(auth, patientId),
      listSeances(auth, patientId)
    ]);
    setContacts(loadedContacts);
    setConsents(loadedConsents);
    setSeances(loadedSeances);
  };

  useEffect(() => {
    void loadPatients().catch((loadError) => setError(loadError.message));
    void listAppointments(auth)
      .then(setAppointments)
      .catch((loadError) => setError(loadError.message));
    // eslint-disable-next-line react-hooks/exhaustive-deps -- loadPatients est recreee a chaque rendu; auth suffit
  }, [auth]);

  useEffect(() => {
    if (!selectedPatientId) {
      setContacts([]);
      setConsents([]);
      setSeances([]);
      return;
    }

    void loadDetails(selectedPatientId).catch((loadError) => setError(loadError.message));
    // eslint-disable-next-line react-hooks/exhaustive-deps -- loadDetails est recreee a chaque rendu
  }, [selectedPatientId]);

  const handleCreate = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError('');
    setMessage('');
    try {
      await createPatient(auth, {
        firstName: draft.firstName,
        lastName: draft.lastName,
        dateOfBirth: draft.dateOfBirth || null,
        mutuelle: draft.mutuelle || null,
        diagnosis: draft.diagnosis || null
      });
      setDraft({ firstName: '', lastName: '', dateOfBirth: '', mutuelle: '', diagnosis: '' });
      setShowCreateModal(false);
      await loadPatients();
      setMessage('Patient cree.');
    } catch (createError) {
      setError((createError as Error).message);
    }
  };

  const handleCreateSeance = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!selectedPatient) {
      return;
    }

    setError('');
    setMessage('');
    try {
      await createSeance(auth, selectedPatient.id, {
        dateSeanceUtc: seanceDraft.dateSeance ? `${seanceDraft.dateSeance}:00Z` : new Date().toISOString(),
        note: seanceDraft.note || null
      });
      setSeanceDraft({ dateSeance: toLocalDateTimeInput(new Date()), note: '' });
      setShowSeanceModal(false);
      await loadDetails(selectedPatient.id);
      setMessage('Seance enregistree.');
    } catch (seanceError) {
      setError((seanceError as Error).message);
    }
  };

  const handleSaveSelected = async () => {
    if (!selectedPatient) {
      return;
    }

    setError('');
    setMessage('');
    try {
      await updatePatient(auth, selectedPatient.id, {
        firstName: selectedPatient.firstName,
        lastName: selectedPatient.lastName,
        dateOfBirth: selectedPatient.dateOfBirth,
        mutuelle: selectedPatient.mutuelle,
        diagnosis: selectedPatient.diagnosis,
        sessionsPrescribed: selectedPatient.sessionsPrescribed,
        sessionsDone: selectedPatient.sessionsDone
      });
      await loadPatients();
      await loadDetails(selectedPatient.id);
      setShowEditModal(false);
      setMessage('Patient mis a jour.');
    } catch (saveError) {
      setError((saveError as Error).message);
    }
  };

  const handleArchiveSelected = async () => {
    if (!selectedPatient) {
      return;
    }

    setError('');
    setMessage('');
    try {
      await archivePatient(auth, selectedPatient.id);
      await loadPatients();
      setShowEditModal(false);
      setMessage('Patient archive.');
    } catch (archiveError) {
      setError((archiveError as Error).message);
    }
  };

  const handleCreateContact = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!selectedPatient) {
      return;
    }

    setError('');
    setMessage('');
    try {
      await createPatientContact(auth, selectedPatient.id, contactDraft);
      setContactDraft({ type: 0, value: '', isPrimary: false });
      await loadDetails(selectedPatient.id);
      setMessage('Contact ajoute.');
    } catch (createContactError) {
      setError((createContactError as Error).message);
    }
  };

  const handleEditContact = async (contact: PatientContact) => {
    if (!selectedPatient) {
      return;
    }

    setError('');
    setMessage('');
    try {
      await updatePatientContact(auth, selectedPatient.id, contact.id, {
        value: contact.value,
        isPrimary: contact.isPrimary
      });
      await loadDetails(selectedPatient.id);
      setMessage('Contact mis a jour.');
    } catch (editContactError) {
      setError((editContactError as Error).message);
    }
  };

  const handleRemoveContact = async (contact: PatientContact) => {
    if (!selectedPatient) {
      return;
    }

    setError('');
    setMessage('');
    try {
      await removePatientContact(auth, selectedPatient.id, contact.id);
      await loadDetails(selectedPatient.id);
      setMessage('Contact supprime.');
    } catch (removeContactError) {
      setError((removeContactError as Error).message);
    }
  };

  const handleCreateConsent = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!selectedPatient) {
      return;
    }

    setError('');
    setMessage('');
    try {
      await createPatientConsent(auth, selectedPatient.id, consentDraft);
      await loadDetails(selectedPatient.id);
      setMessage('Consentement enregistre.');
    } catch (createConsentError) {
      setError((createConsentError as Error).message);
    }
  };

  const handleRevokeConsent = async (consent: PatientConsent) => {
    if (!selectedPatient) {
      return;
    }

    setError('');
    setMessage('');
    try {
      await revokePatientConsent(auth, selectedPatient.id, consent.id);
      await loadDetails(selectedPatient.id);
      setMessage('Consentement revoque.');
    } catch (revokeConsentError) {
      setError((revokeConsentError as Error).message);
    }
  };

  const updateSelectedPatient = (
    field: 'firstName' | 'lastName' | 'dateOfBirth' | 'mutuelle' | 'diagnosis',
    value: string
  ) => {
    if (!selectedPatient) {
      return;
    }

    setPatients((current) =>
      current.map((patient) => (patient.id === selectedPatient.id ? { ...patient, [field]: value } : patient))
    );
  };

  const updateSessionsPrescribed = (value: number) => {
    if (!selectedPatient) {
      return;
    }

    setPatients((current) =>
      current.map((patient) =>
        patient.id === selectedPatient.id
          ? { ...patient, sessionsPrescribed: Number.isNaN(value) ? 0 : value }
          : patient
      )
    );
  };

  return (
    <section className="stack">
      {error ? <p className="error">{error}</p> : null}
      {message ? <p className="success">{message}</p> : null}

      <div className="patients-layout">
        <div className="panel">
          <div className="dossier-header">
            <h3 style={{ margin: 0 }}>Patients</h3>
            <button className="primary-button is-create" type="button" onClick={() => setShowCreateModal(true)}>
              Nouveau patient
            </button>
          </div>
          <p className="muted">Contexte tenant: {tenantId}</p>

          <div className="list">
            {patients.map((patient) => (
              <button
                key={patient.id}
                type="button"
                className={patient.id === selectedPatientId ? 'list-item active' : 'list-item'}
                onClick={() => setSelectedPatientId(patient.id)}
              >
                <div>
                  <strong style={{ display: 'block' }}>
                    {patient.firstName} {patient.lastName}
                  </strong>
                  <span className="muted compact" style={{ display: 'block' }}>
                    {patient.diagnosis || `Ne(e) le ${formatDate(patient.dateOfBirth)}`}
                  </span>
                </div>
                <span className={patient.status === 0 ? 'badge badge-success' : 'badge badge-neutral'}>
                  {patient.status === 0 ? 'Actif' : 'Archive'}
                </span>
              </button>
            ))}
          </div>
        </div>

        <div className="panel">
          <p className="eyebrow">Dossier patient</p>
          <h3 style={{ margin: '4px 0 16px' }}>
            {selectedPatient ? `${selectedPatient.firstName} ${selectedPatient.lastName}` : 'Aucun patient selectionne'}
          </h3>

          {selectedPatient ? (
            <div className="stack">
              <div className="detail-grid">
                <div>
                  <span className="detail-label">Naissance</span>
                  <strong>{formatDate(selectedPatient.dateOfBirth)}</strong>
                </div>
                <div>
                  <span className="detail-label">Mutuelle</span>
                  <strong>{selectedPatient.mutuelle || '—'}</strong>
                </div>
                <div>
                  <span className="detail-label">Telephone</span>
                  <strong>{selectedPatientPhone || '—'}</strong>
                </div>
                <div>
                  <span className="detail-label">Derniere seance</span>
                  <strong>{formatDate(selectedPatientLastSession)}</strong>
                </div>
              </div>

              <div className="progress-block">
                <div className="progress-label-row">
                  <span className="muted">Seances prescrites</span>
                  <strong>
                    {sessionsDone} / {selectedPatient.sessionsPrescribed}
                  </strong>
                </div>
                <div className="progress-track">
                  <div
                    className="progress-fill"
                    style={{
                      width: `${
                        selectedPatient.sessionsPrescribed > 0
                          ? Math.min(100, Math.round((100 * sessionsDone) / selectedPatient.sessionsPrescribed))
                          : 0
                      }%`
                    }}
                  />
                </div>
                <div className="toolbar" style={{ marginTop: '0.75rem' }}>
                  <button className="ghost-button" type="button" onClick={() => setShowSeanceModal(true)}>
                    + Ajouter une seance
                  </button>
                  <button className="ghost-button" type="button" onClick={() => setShowEditModal(true)}>
                    Modifier
                  </button>
                </div>
              </div>

              <section className="subpanel">
                <h4>Seances ({seances.length})</h4>
                {seances.length === 0 ? (
                  <p className="muted compact">Aucune seance enregistree.</p>
                ) : (
                  <div className="list">
                    {seances.slice(0, 5).map((seance) => (
                      <div key={seance.id} className="list-item">
                        <div>
                          <strong>{formatDate(seance.dateSeanceUtc)}</strong>
                          <p className="muted compact">{seance.note ?? 'Sans note'}</p>
                        </div>
                        <span className="badge badge-success">Effectuee</span>
                      </div>
                    ))}
                    {seances.length > 5 ? (
                      <p className="muted compact">+ {seances.length - 5} seances plus anciennes</p>
                    ) : null}
                  </div>
                )}
              </section>

              <section className="subpanel">
                <h4>Contacts</h4>
                <form className="stack" onSubmit={handleCreateContact}>
                  <div className="form-grid">
                    <label>
                      Type
                      <select
                        value={contactDraft.type}
                        onChange={(event) =>
                          setContactDraft({
                            ...contactDraft,
                            type: Number(event.target.value) as PatientContactType
                          })
                        }
                      >
                        {contactTypeOptions.map((option) => (
                          <option key={option.value} value={option.value}>
                            {option.label}
                          </option>
                        ))}
                      </select>
                    </label>
                    <label>
                      Valeur
                      <input
                        value={contactDraft.value}
                        onChange={(event) => setContactDraft({ ...contactDraft, value: event.target.value })}
                      />
                    </label>
                    <label className="inline-check">
                      <input
                        type="checkbox"
                        checked={contactDraft.isPrimary}
                        onChange={(event) =>
                          setContactDraft({ ...contactDraft, isPrimary: event.target.checked })
                        }
                      />
                      Primaire
                    </label>
                  </div>
                  <button className="primary-button is-create" type="submit" disabled={!contactDraft.value.trim()}>
                    Ajouter contact
                  </button>
                </form>

                <div className="list">
                  {contacts.map((contact) => (
                    <div key={contact.id} className="list-item">
                      <div>
                        <strong>{contactTypeOptions.find((item) => item.value === contact.type)?.label}</strong>
                        <p className="muted compact">
                          {contact.value} {contact.isPrimary ? '· primaire' : ''}
                        </p>
                      </div>
                      <div className="toolbar">
                        <button className="ghost-button" type="button" onClick={() => handleEditContact(contact)}>
                          Sauver
                        </button>
                        <button className="ghost-button" type="button" onClick={() => handleRemoveContact(contact)}>
                          Supprimer
                        </button>
                      </div>
                    </div>
                  ))}
                </div>
              </section>

              <section className="subpanel">
                <h4>Consentements</h4>
                <form className="stack" onSubmit={handleCreateConsent}>
                  <div className="form-grid">
                    <label>
                      Type
                      <select
                        value={consentDraft.type}
                        onChange={(event) =>
                          setConsentDraft({
                            ...consentDraft,
                            type: Number(event.target.value) as ConsentType
                          })
                        }
                      >
                        {consentTypeOptions.map((option) => (
                          <option key={option.value} value={option.value}>
                            {option.label}
                          </option>
                        ))}
                      </select>
                    </label>
                    <label className="inline-check">
                      <input
                        type="checkbox"
                        checked={consentDraft.granted}
                        onChange={(event) =>
                          setConsentDraft({ ...consentDraft, granted: event.target.checked })
                        }
                      />
                      Accord donne
                    </label>
                  </div>
                  <button className="primary-button is-create" type="submit">
                    Ajouter consentement
                  </button>
                </form>

                <div className="list">
                  {consents.map((consent) => (
                    <div key={consent.id} className="list-item">
                      <div>
                        <strong>{consentTypeOptions.find((item) => item.value === consent.type)?.label}</strong>
                        <p className="muted compact">
                          revocation {consent.revokedAtUtc ? 'oui' : 'non'}
                        </p>
                      </div>
                      <span className={consent.granted ? 'badge badge-success' : 'badge badge-neutral'}>
                        {consent.granted ? 'Accord' : 'Refuse'}
                      </span>
                      <button className="ghost-button" type="button" onClick={() => handleRevokeConsent(consent)}>
                        Revoquer
                      </button>
                    </div>
                  ))}
                </div>
              </section>
            </div>
          ) : (
            <p className="muted">Selectionne un patient ou cree-en un.</p>
          )}
        </div>
      </div>

      {showCreateModal ? (
        <Modal title="Nouveau patient" onClose={() => setShowCreateModal(false)}>
          <form className="stack" onSubmit={handleCreate}>
            <div className="form-grid">
              <label>
                Prenom
                <input
                  value={draft.firstName}
                  onChange={(event) => setDraft({ ...draft, firstName: event.target.value })}
                />
              </label>
              <label>
                Nom
                <input
                  value={draft.lastName}
                  onChange={(event) => setDraft({ ...draft, lastName: event.target.value })}
                />
              </label>
              <label>
                Date de naissance
                <input
                  type="date"
                  value={draft.dateOfBirth}
                  onChange={(event) => setDraft({ ...draft, dateOfBirth: event.target.value })}
                />
              </label>
              <label>
                Mutuelle
                <input
                  value={draft.mutuelle}
                  onChange={(event) => setDraft({ ...draft, mutuelle: event.target.value })}
                />
              </label>
              <label>
                Diagnostic
                <input
                  value={draft.diagnosis}
                  onChange={(event) => setDraft({ ...draft, diagnosis: event.target.value })}
                />
              </label>
            </div>
            <button
              className="primary-button is-create"
              type="submit"
              disabled={!draft.firstName.trim() || !draft.lastName.trim()}
            >
              Creer
            </button>
          </form>
        </Modal>
      ) : null}

      {showSeanceModal && selectedPatient ? (
        <Modal title="Nouvelle seance" onClose={() => setShowSeanceModal(false)}>
          <form className="stack" onSubmit={handleCreateSeance}>
            <div className="form-grid">
              <label>
                Date et heure
                <input
                  type="datetime-local"
                  value={seanceDraft.dateSeance}
                  onChange={(event) => setSeanceDraft({ ...seanceDraft, dateSeance: event.target.value })}
                />
              </label>
              <label>
                Note clinique
                <input
                  value={seanceDraft.note}
                  placeholder="Ex: Suivi lombalgie, exercices renforcement"
                  onChange={(event) => setSeanceDraft({ ...seanceDraft, note: event.target.value })}
                />
              </label>
            </div>
            <p className="muted compact">
              La seance est enregistree dans le dossier clinique (tracee et auditee); la progression du patient est
              derivee du nombre de seances reelles.
            </p>
            <button className="primary-button is-create" type="submit" disabled={!seanceDraft.dateSeance}>
              Enregistrer la seance
            </button>
          </form>
        </Modal>
      ) : null}

      {showEditModal && selectedPatient ? (
        <Modal title="Modifier le patient" onClose={() => setShowEditModal(false)}>
          <div className="stack">
            <div className="form-grid">
              <label>
                Prenom
                <input
                  value={selectedPatient.firstName}
                  onChange={(event) => updateSelectedPatient('firstName', event.target.value)}
                />
              </label>
              <label>
                Nom
                <input
                  value={selectedPatient.lastName}
                  onChange={(event) => updateSelectedPatient('lastName', event.target.value)}
                />
              </label>
              <label>
                Date de naissance
                <input
                  type="date"
                  value={selectedPatient.dateOfBirth ?? ''}
                  onChange={(event) => updateSelectedPatient('dateOfBirth', event.target.value)}
                />
              </label>
              <label>
                Mutuelle
                <input
                  value={selectedPatient.mutuelle ?? ''}
                  onChange={(event) => updateSelectedPatient('mutuelle', event.target.value)}
                />
              </label>
              <label>
                Diagnostic
                <input
                  value={selectedPatient.diagnosis ?? ''}
                  onChange={(event) => updateSelectedPatient('diagnosis', event.target.value)}
                />
              </label>
              <label>
                Seances prescrites
                <input
                  type="number"
                  min={0}
                  value={selectedPatient.sessionsPrescribed}
                  onChange={(event) => updateSessionsPrescribed(event.target.valueAsNumber)}
                />
              </label>
            </div>

            <p className="muted compact">
              Les seances effectuees ne se modifient plus ici: elles sont derivees des seances cliniques reelles
              (voir la section Seances du dossier).
            </p>

            <div className="toolbar">
              <button className="primary-button" type="button" onClick={handleSaveSelected}>
                Enregistrer
              </button>
              <button className="ghost-button" type="button" onClick={handleArchiveSelected}>
                Archiver
              </button>
            </div>
          </div>
        </Modal>
      ) : null}
    </section>
  );
}
