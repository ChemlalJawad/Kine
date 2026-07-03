import { useEffect, useMemo, useState } from 'react';
import { useAuth } from '../auth/AuthContext';
import { appointmentStatusBadgeClass, appointmentStatusLabels } from '../data/appointmentStatus';
import { listPatients, type Patient } from '../api/patientsApi';
import { listAppointments, type Appointment } from '../api/schedulingApi';
import { formatMontant, listInvoices, type Invoice } from '../api/billingApi';

const pendingInvoiceStatus = 0;

function isToday(isoUtc: string): boolean {
  const date = new Date(isoUtc);
  const now = new Date();
  return (
    date.getUTCFullYear() === now.getUTCFullYear() &&
    date.getUTCMonth() === now.getUTCMonth() &&
    date.getUTCDate() === now.getUTCDate()
  );
}

function isCurrentMonth(isoUtc: string): boolean {
  const date = new Date(isoUtc);
  const now = new Date();
  return date.getUTCFullYear() === now.getUTCFullYear() && date.getUTCMonth() === now.getUTCMonth();
}

function formatTime(value: string): string {
  const date = new Date(value);
  return Number.isNaN(date.getTime())
    ? value
    : date.toLocaleTimeString('fr-BE', { hour: '2-digit', minute: '2-digit' });
}

export function DashboardPage() {
  const { user } = useAuth();
  const tenantId = user?.tenantId ?? '';
  const actorId = user?.actorId ?? '';
  const auth = useMemo(() => ({ tenantId, actorId }), [tenantId, actorId]);

  const [patients, setPatients] = useState<Patient[]>([]);
  const [appointments, setAppointments] = useState<Appointment[]>([]);
  const [invoices, setInvoices] = useState<Invoice[]>([]);
  const [error, setError] = useState('');

  useEffect(() => {
    void Promise.all([listPatients(auth), listAppointments(auth), listInvoices(auth)])
      .then(([loadedPatients, loadedAppointments, loadedInvoices]) => {
        setPatients(loadedPatients);
        setAppointments(loadedAppointments);
        setInvoices(loadedInvoices);
      })
      .catch((loadError) => setError(loadError.message));
  }, [auth]);

  const activePatients = patients.filter((patient) => patient.status === 0);
  const todaysAppointments = appointments
    .filter((appointment) => isToday(appointment.startAtUtc))
    .sort((a, b) => a.startAtUtc.localeCompare(b.startAtUtc));

  const patientName = (patientId: string) => {
    const patient = patients.find((item) => item.id === patientId);
    return patient ? `${patient.firstName} ${patient.lastName}` : patientId;
  };

  // KPIs mockup 1b, desormais calcules depuis le module Billing reel:
  // CA du mois = somme des factures emises ce mois-ci; remboursements en
  // attente = factures au statut Pending.
  const monthRevenue = invoices
    .filter((invoice) => isCurrentMonth(invoice.createdAtUtc))
    .reduce((sum, invoice) => sum + invoice.amount, 0);
  const pendingReimbursements = invoices.filter((invoice) => invoice.status === pendingInvoiceStatus).length;

  const kpis = [
    { label: 'RDV aujourd’hui', value: String(todaysAppointments.length) },
    { label: 'Patients actifs', value: String(activePatients.length) },
    { label: 'CA du mois', value: formatMontant(monthRevenue) },
    { label: 'Remboursements en attente', value: String(pendingReimbursements) }
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

      <div className="panel">
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
                    {formatTime(appointment.startAtUtc)} – {formatTime(appointment.endAtUtc)} &middot;{' '}
                    {appointment.practitionerId}
                  </p>
                </div>
                <span className={appointmentStatusBadgeClass[appointment.status]}>
                  {appointmentStatusLabels[appointment.status]}
                </span>
              </div>
            ))}
          </div>
        )}
      </div>
    </section>
  );
}
