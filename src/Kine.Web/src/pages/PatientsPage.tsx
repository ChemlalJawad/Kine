import { FormEvent, useEffect, useMemo, useState } from 'react';
import { useAuth } from '../auth/AuthContext';
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
};

export function PatientsPage() {
  const { tenantId, actorId } = useAuth();
  const auth = useMemo(() => ({ tenantId, actorId }), [tenantId, actorId]);
  const [patients, setPatients] = useState<Patient[]>([]);
  const [selectedPatientId, setSelectedPatientId] = useState<string>('');
  const [contacts, setContacts] = useState<PatientContact[]>([]);
  const [consents, setConsents] = useState<PatientConsent[]>([]);
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');
  const [draft, setDraft] = useState<Draft>({ firstName: '', lastName: '', dateOfBirth: '' });
  const [contactDraft, setContactDraft] = useState({ type: 0 as PatientContactType, value: '', isPrimary: false });
  const [consentDraft, setConsentDraft] = useState({ type: 0 as ConsentType, granted: true });

  const selectedPatient = patients.find((patient) => patient.id === selectedPatientId) ?? null;

  const loadPatients = async () => {
    const items = await listPatients(auth);
    setPatients(items);
    if (!selectedPatientId && items.length > 0) {
      setSelectedPatientId(items[0].id);
    }
  };

  const loadDetails = async (patientId: string) => {
    const [loadedContacts, loadedConsents] = await Promise.all([
      listPatientContacts(auth, patientId),
      listPatientConsents(auth, patientId)
    ]);
    setContacts(loadedContacts);
    setConsents(loadedConsents);
  };

  useEffect(() => {
    void loadPatients().catch((loadError) => setError(loadError.message));
  }, []);

  useEffect(() => {
    if (!selectedPatientId) {
      setContacts([]);
      setConsents([]);
      return;
    }

    void loadDetails(selectedPatientId).catch((loadError) => setError(loadError.message));
  }, [selectedPatientId]);

  const handleCreate = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError('');
    setMessage('');
    await createPatient(auth, {
      firstName: draft.firstName,
      lastName: draft.lastName,
      dateOfBirth: draft.dateOfBirth || null
    });
    setDraft({ firstName: '', lastName: '', dateOfBirth: '' });
    await loadPatients();
    setMessage('Patient cree.');
  };

  const handleSaveSelected = async () => {
    if (!selectedPatient) {
      return;
    }

    setError('');
    setMessage('');
    await updatePatient(auth, selectedPatient.id, {
      firstName: selectedPatient.firstName,
      lastName: selectedPatient.lastName,
      dateOfBirth: selectedPatient.dateOfBirth
    });
    await loadPatients();
    await loadDetails(selectedPatient.id);
    setMessage('Patient mis a jour.');
  };

  const handleArchiveSelected = async () => {
    if (!selectedPatient) {
      return;
    }

    setError('');
    setMessage('');
    await archivePatient(auth, selectedPatient.id);
    await loadPatients();
    setMessage('Patient archive.');
  };

  const handleCreateContact = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!selectedPatient) {
      return;
    }

    setError('');
    setMessage('');
    await createPatientContact(auth, selectedPatient.id, contactDraft);
    setContactDraft({ type: 0, value: '', isPrimary: false });
    await loadDetails(selectedPatient.id);
    setMessage('Contact ajoute.');
  };

  const handleEditContact = async (contact: PatientContact) => {
    if (!selectedPatient) {
      return;
    }

    setError('');
    setMessage('');
    await updatePatientContact(auth, selectedPatient.id, contact.id, {
      value: contact.value,
      isPrimary: contact.isPrimary
    });
    await loadDetails(selectedPatient.id);
    setMessage('Contact mis a jour.');
  };

  const handleRemoveContact = async (contact: PatientContact) => {
    if (!selectedPatient) {
      return;
    }

    setError('');
    setMessage('');
    await removePatientContact(auth, selectedPatient.id, contact.id);
    await loadDetails(selectedPatient.id);
    setMessage('Contact supprime.');
  };

  const handleCreateConsent = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    if (!selectedPatient) {
      return;
    }

    setError('');
    setMessage('');
    await createPatientConsent(auth, selectedPatient.id, consentDraft);
    await loadDetails(selectedPatient.id);
    setMessage('Consentement enregistre.');
  };

  const handleRevokeConsent = async (consent: PatientConsent) => {
    if (!selectedPatient) {
      return;
    }

    setError('');
    setMessage('');
    await revokePatientConsent(auth, selectedPatient.id, consent.id);
    await loadDetails(selectedPatient.id);
    setMessage('Consentement revoque.');
  };

  const updateSelectedPatient = (field: 'firstName' | 'lastName' | 'dateOfBirth', value: string) => {
    if (!selectedPatient) {
      return;
    }

    setPatients((current) =>
      current.map((patient) => (patient.id === selectedPatient.id ? { ...patient, [field]: value } : patient))
    );
  };

  return (
    <section className="patients-grid">
      <div className="panel">
        <p className="eyebrow">Module Patients</p>
        <h3>Patients</h3>
        <p className="muted">CRUD staff minimal avec contexte tenant: {tenantId}</p>

        <form className="stack" onSubmit={handleCreate}>
          <h4>Nouveau patient</h4>
          <div className="form-grid">
            <label>
              Prenom
              <input value={draft.firstName} onChange={(event) => setDraft({ ...draft, firstName: event.target.value })} />
            </label>
            <label>
              Nom
              <input value={draft.lastName} onChange={(event) => setDraft({ ...draft, lastName: event.target.value })} />
            </label>
            <label>
              Date de naissance
              <input
                type="date"
                value={draft.dateOfBirth}
                onChange={(event) => setDraft({ ...draft, dateOfBirth: event.target.value })}
              />
            </label>
          </div>
          <button className="primary-button" type="submit">
            Creer
          </button>
        </form>

        <div className="list">
          {patients.map((patient) => (
            <button
              key={patient.id}
              type="button"
              className={patient.id === selectedPatientId ? 'list-item active' : 'list-item'}
              onClick={() => setSelectedPatientId(patient.id)}
            >
              <strong>
                {patient.firstName} {patient.lastName}
              </strong>
              <span className="muted">
                {patient.status === 0 ? 'Actif' : 'Archive'} · {patient.createdBy}
              </span>
            </button>
          ))}
        </div>
      </div>

      <div className="panel">
        <p className="eyebrow">Details</p>
        <h3>Patient selectionne</h3>

        {selectedPatient ? (
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
            </div>

            <div className="toolbar">
              <button className="primary-button" type="button" onClick={handleSaveSelected}>
                Enregistrer
              </button>
              <button className="ghost-button" type="button" onClick={handleArchiveSelected}>
                Archiver
              </button>
            </div>

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
                <button className="primary-button" type="submit">
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
                <button className="primary-button" type="submit">
                  Ajouter consentement
                </button>
              </form>

              <div className="list">
                {consents.map((consent) => (
                  <div key={consent.id} className="list-item">
                    <div>
                      <strong>{consentTypeOptions.find((item) => item.value === consent.type)?.label}</strong>
                      <p className="muted compact">
                        {consent.granted ? 'Accord' : 'Refuse'} · revocation{' '}
                        {consent.revokedAtUtc ? 'oui' : 'non'}
                      </p>
                    </div>
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

      <div className="panel status-panel">
        <h4>Etat</h4>
        {message ? <p className="success">{message}</p> : null}
        {error ? <p className="error">{error}</p> : null}
      </div>
    </section>
  );
}
