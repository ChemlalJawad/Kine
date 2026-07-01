import { useEffect, useMemo, useState } from 'react';
import { useAuth } from '../auth/AuthContext';
import { listPatients, type Patient } from '../api/patientsApi';
import { listAppointments, type Appointment } from '../api/schedulingApi';

const statusLabels: Record<number, string> = {
  0: 'Planifie',
  1: 'Annule',
  2: 'No-show',
  3: 'Termine'
};

const statusBadgeClass: Record<number, string> = {
  0: 'badge badge-success',
  1: 'badge badge-danger',
  2: 'badge badge-warning',
  3: 'badge badge-neutral'
};

function isToday(isoUtc: string): boolean {
  const date = new Date(isoUtc);
  const now = new Date();
  return (
    date.getUTCFullYear() === now.getUTCFullYear() &&
    date.getUTCMonth() === now.getUTCMonth() &&
    date.getUTCDate() === now.getUTCDate()
  );
}

export function DashboardPage() {
  const { user } = useAuth();
  const tenantId = user?.tenantId ?? '';
  const actorId = user?.actorId ?? '';
  const auth = useMemo(() => ({ tenantId, actorId }), [tenantId, actorId]);

  const [patients, setPatients] = useState<Patient[]>([]);
  const [appointments, setAppointments] = useState<Appointment[]>([]);
  const [error, setError] = useState('');

  useEffect(() => {
    void Promise.all([listPatients(auth), listAppointments(auth)])
      .then(([loadedPatients, loadedAppointments]) => {
        setPatients(loadedPatients);
        setAppointments(loadedAppointments);
      })
      .catch((loadError) => setError(loadError.message));
  }, []);

  const activePatients = patients.filter((patient) => patient.status === 0);
  const todaysAppointments = appointments
    .filter((appointment) => isToday(appointment.startAtUtc))
    .sort((a, b) => a.startAtUtc.localeCompare(b.startAtUtc));

  const patientName = (patientId: string) => {
    const patient = patients.find((item) => item.id === patientId);
    return patient ? `${patient.firstName} ${patient.lastName}` : patientId;
  };

  const kpis = [
    { label: 'RDV aujourd\u2019hui', value: String(todaysAppointments.length) },
    { label: 'Patients actifs', value: String(activePatients.length) },
    { label: 'Patients archives', value: String(patients.length - activePatients.length) },
    { label: 'Total rendez-vous', value: String(appointments.length) }
  ];

  return (
    <section className="stack">
      {error ? <p className="error">{error}</p> : null}

      <div className="kpi-grid">
        {kpis.map((kpi) => (
          <div key={kpi.label} className="kpi-card">
            <p className="kpi-label">{kpi.label}</p>
            <p className="kpi-value">{kpi.value}</p>
          </div>
        ))}
      </div>

      <div className="panel" style={{ maxWidth: 'none' }}>
        <h3 style={{ marginTop: 0 }}>Planning du jour</h3>
        {todaysAppointments.length === 0 ? (
          <p className="muted">Aucun rendez-vous programme aujourd hui.</p>
        ) : (
          <div className="list">
            {todaysAppointments.map((appointment) => (
              <div key={appointment.id} className="list-item">
                <div>
                  <strong>{patientName(appointment.patientId)}</strong>
                  <p className="muted compact">
                    {appointment.practitionerId} &middot; {appointment.startAtUtc}
                  </p>
                </div>
                <span className={statusBadgeClass[appointment.status]}>{statusLabels[appointment.status]}</span>
              </div>
            ))}
          </div>
        )}
      </div>
    </section>
  );
}
