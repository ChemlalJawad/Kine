import { useEffect, useMemo, useState } from 'react';
import { useAuth } from '../auth/AuthContext';
import { formatMontant } from '../api/billingApi';
import { downloadCsv, getSummary, type ReportingSummary } from '../api/reportingApi';

export function ReportingPage() {
  const { user } = useAuth();
  const tenantId = user?.tenantId ?? '';
  const actorId = user?.actorId ?? '';
  const roles = user?.roles;
  const auth = useMemo(() => ({ tenantId, actorId, roles }), [tenantId, actorId, roles]);

  const [summary, setSummary] = useState<ReportingSummary | null>(null);
  const [error, setError] = useState('');
  const [message, setMessage] = useState('');

  useEffect(() => {
    void getSummary(auth)
      .then(setSummary)
      .catch((loadError) => setError(loadError.message));
  }, [auth]);

  const totals = summary?.months.reduce(
    (acc, month) => ({
      invoiced: acc.invoiced + month.invoicedAmount,
      reimbursed: acc.reimbursed + month.reimbursedAmount,
      seances: acc.seances + month.seances,
      noShows: acc.noShows + month.noShows
    }),
    { invoiced: 0, reimbursed: 0, seances: 0, noShows: 0 }
  );

  const handleExport = async () => {
    setError('');
    setMessage('');
    try {
      const blob = await downloadCsv(auth);
      const url = URL.createObjectURL(blob);
      const link = document.createElement('a');
      link.href = url;
      link.download = 'q-ine-reporting.csv';
      document.body.appendChild(link);
      link.click();
      link.remove();
      URL.revokeObjectURL(url);
      setMessage('Export CSV telecharge.');
    } catch (exportError) {
      setError((exportError as Error).message);
    }
  };

  return (
    <section className="stack">
      {error ? <p className="error">{error}</p> : null}
      {message ? <p className="success">{message}</p> : null}

      <div className="kpi-grid">
        <div className="kpi-card">
          <p className="kpi-label">Patients actifs</p>
          <p className="kpi-value">{summary?.patientsActive ?? '—'}</p>
        </div>
        <div className="kpi-card">
          <p className="kpi-label">CA facture (total)</p>
          <p className="kpi-value">{totals ? formatMontant(totals.invoiced) : '—'}</p>
        </div>
        <div className="kpi-card">
          <p className="kpi-label">Rembourse (total)</p>
          <p className="kpi-value">{totals ? formatMontant(totals.reimbursed) : '—'}</p>
        </div>
        <div className="kpi-card">
          <p className="kpi-label">Seances realisees</p>
          <p className="kpi-value">{totals?.seances ?? '—'}</p>
        </div>
      </div>

      <div className="panel">
        <div className="dossier-header">
          <div>
            <p className="eyebrow">Reporting</p>
            <h3 style={{ margin: '4px 0 0' }}>Activite par mois</h3>
          </div>
          <button className="ghost-button" type="button" onClick={handleExport}>
            Exporter CSV
          </button>
        </div>
        <p className="muted">Vues agregees read-only (activite, CA, remboursements) — acces AdminCabinet.</p>

        {!summary || summary.months.length === 0 ? (
          <p className="muted">Aucune donnee a agreger pour le moment.</p>
        ) : (
          <div className="invoice-table">
            <div className="report-row invoice-header">
              <span>Mois</span>
              <span>RDV</span>
              <span>Termines</span>
              <span>Annules</span>
              <span>No-show</span>
              <span>Seances</span>
              <span>Facture</span>
              <span>Rembourse</span>
            </div>
            {summary.months.map((month) => (
              <div key={month.month} className="report-row invoice-row-plain">
                <strong>{month.month}</strong>
                <span>{month.appointments}</span>
                <span>{month.completed}</span>
                <span>{month.cancelled}</span>
                <span>{month.noShows}</span>
                <span>{month.seances}</span>
                <span>{formatMontant(month.invoicedAmount)}</span>
                <span>{formatMontant(month.reimbursedAmount)}</span>
              </div>
            ))}
          </div>
        )}
      </div>
    </section>
  );
}
