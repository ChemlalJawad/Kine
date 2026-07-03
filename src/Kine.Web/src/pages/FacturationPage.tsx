import { FormEvent, useEffect, useMemo, useState } from 'react';
import { useAuth } from '../auth/AuthContext';
import { Modal } from '../components/Modal';
import { listPatients, type Patient } from '../api/patientsApi';
import {
  createInvoice,
  formatMontant,
  invoiceStatusBadgeClass,
  invoiceStatusLabels,
  listActes,
  listInvoices,
  markInvoiceReimbursed,
  markInvoiceRejected,
  type ActeInami,
  type Invoice
} from '../api/billingApi';
import {
  allowedTransitions,
  caseStatusBadgeClass,
  caseStatusLabels,
  createCase,
  listCases,
  transitionCase,
  type ReimbursementCase,
  type ReimbursementCaseStatus
} from '../api/reimbursementApi';

const pendingStatus = 0;

type InvoiceDraft = {
  patientId: string;
  codeInami: string;
};

export function FacturationPage() {
  const { user } = useAuth();
  const tenantId = user?.tenantId ?? '';
  const actorId = user?.actorId ?? '';
  const roles = user?.roles;
  const auth = useMemo(() => ({ tenantId, actorId, roles }), [tenantId, actorId, roles]);

  const [invoices, setInvoices] = useState<Invoice[]>([]);
  const [actes, setActes] = useState<ActeInami[]>([]);
  const [patients, setPatients] = useState<Patient[]>([]);
  const [cases, setCases] = useState<ReimbursementCase[]>([]);
  const [message, setMessage] = useState('');
  const [error, setError] = useState('');
  const [showCreateModal, setShowCreateModal] = useState(false);
  const [showCaseModal, setShowCaseModal] = useState(false);
  const [draft, setDraft] = useState<InvoiceDraft>({ patientId: '', codeInami: '' });
  const [caseInvoiceIds, setCaseInvoiceIds] = useState<string[]>([]);

  const loadAll = async () => {
    const [loadedInvoices, loadedActes, loadedPatients, loadedCases] = await Promise.all([
      listInvoices(auth),
      listActes(auth),
      listPatients(auth),
      listCases(auth)
    ]);
    setInvoices(loadedInvoices);
    setActes(loadedActes);
    setPatients(loadedPatients);
    setCases(loadedCases);
  };

  useEffect(() => {
    void loadAll().catch((loadError) => setError(loadError.message));
    // eslint-disable-next-line react-hooks/exhaustive-deps -- loadAll est recreee a chaque rendu; auth suffit
  }, [auth]);

  const patientName = (patientId: string) => {
    const patient = patients.find((item) => item.id === patientId);
    return patient ? `${patient.firstName} ${patient.lastName}` : patientId;
  };

  const invoiceLabel = (invoiceId: string) => {
    const invoice = invoices.find((item) => item.id === invoiceId);
    return invoice ? `${patientName(invoice.patientId)} · ${invoice.codeInami}` : invoiceId;
  };

  const sortedInvoices = [...invoices].sort((a, b) => b.createdAtUtc.localeCompare(a.createdAtUtc));
  const sortedCases = [...cases].sort((a, b) => b.createdAtUtc.localeCompare(a.createdAtUtc));
  const pendingInvoices = invoices.filter((invoice) => invoice.status === pendingStatus);
  const pendingCount = pendingInvoices.length;
  const totalAmount = invoices.reduce((sum, invoice) => sum + invoice.amount, 0);

  const handleCreate = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError('');
    setMessage('');
    try {
      const patient = patients.find((item) => item.id === draft.patientId);
      await createInvoice(auth, {
        patientId: draft.patientId,
        codeInami: draft.codeInami,
        mutuelle: patient?.mutuelle ?? null
      });
      setDraft({ patientId: '', codeInami: '' });
      setShowCreateModal(false);
      await loadAll();
      setMessage('Facture creee.');
    } catch (createError) {
      setError((createError as Error).message);
    }
  };

  const handleMarkReimbursed = async (invoice: Invoice) => {
    setError('');
    setMessage('');
    try {
      await markInvoiceReimbursed(auth, invoice.id);
      await loadAll();
      setMessage('Facture marquee remboursee.');
    } catch (markError) {
      setError((markError as Error).message);
    }
  };

  const handleMarkRejected = async (invoice: Invoice) => {
    setError('');
    setMessage('');
    try {
      await markInvoiceRejected(auth, invoice.id);
      await loadAll();
      setMessage('Facture marquee rejetee.');
    } catch (markError) {
      setError((markError as Error).message);
    }
  };

  const handleCreateCase = async (event: FormEvent<HTMLFormElement>) => {
    event.preventDefault();
    setError('');
    setMessage('');
    try {
      await createCase(auth, { invoiceIds: caseInvoiceIds });
      setCaseInvoiceIds([]);
      setShowCaseModal(false);
      await loadAll();
      setMessage('Dossier de remboursement cree.');
    } catch (caseError) {
      setError((caseError as Error).message);
    }
  };

  const handleTransition = async (reimbursementCase: ReimbursementCase, target: ReimbursementCaseStatus) => {
    setError('');
    setMessage('');
    try {
      await transitionCase(auth, reimbursementCase.id, target);
      await loadAll();
      setMessage('Statut du dossier mis a jour.');
    } catch (transitionError) {
      setError((transitionError as Error).message);
    }
  };

  const toggleCaseInvoice = (invoiceId: string) => {
    setCaseInvoiceIds((current) =>
      current.includes(invoiceId) ? current.filter((id) => id !== invoiceId) : [...current, invoiceId]
    );
  };

  return (
    <section className="stack">
      {error ? <p className="error">{error}</p> : null}
      {message ? <p className="success">{message}</p> : null}

      <div className="panel">
        <div className="dossier-header">
          <div>
            <p className="eyebrow">Facturation</p>
            <h3 style={{ margin: '4px 0 0' }}>
              Facturation &amp; remboursements ({formatMontant(totalAmount)} &middot; {pendingCount} en attente)
            </h3>
          </div>
          <button className="primary-button is-create" type="button" onClick={() => setShowCreateModal(true)}>
            Nouvelle facture
          </button>
        </div>
        <p className="muted">Contexte tenant: {tenantId}</p>

        {sortedInvoices.length === 0 ? (
          <p className="muted">Aucune facture pour le moment. Cree une facture pour commencer.</p>
        ) : (
          <div className="invoice-table">
            <div className="invoice-row invoice-header">
              <span>Patient</span>
              <span>Code</span>
              <span>Acte</span>
              <span>Montant</span>
              <span>Mutuelle</span>
              <span>Statut</span>
              <span></span>
            </div>
            {sortedInvoices.map((invoice) => (
              <div key={invoice.id} className="invoice-row">
                <strong>{patientName(invoice.patientId)}</strong>
                <span className="muted">{invoice.codeInami}</span>
                <span>{invoice.label}</span>
                <span>{formatMontant(invoice.amount)}</span>
                <span className="muted">{invoice.mutuelle ?? '—'}</span>
                <span className={invoiceStatusBadgeClass[invoice.status]}>{invoiceStatusLabels[invoice.status]}</span>
                {invoice.status === pendingStatus ? (
                  <div className="toolbar invoice-actions">
                    <button className="ghost-button" type="button" onClick={() => handleMarkReimbursed(invoice)}>
                      Remboursé
                    </button>
                    <button className="ghost-button" type="button" onClick={() => handleMarkRejected(invoice)}>
                      Rejeté
                    </button>
                  </div>
                ) : (
                  <span></span>
                )}
              </div>
            ))}
          </div>
        )}
      </div>

      <div className="panel">
        <div className="dossier-header">
          <div>
            <p className="eyebrow">Remboursement INAMI</p>
            <h3 style={{ margin: '4px 0 0' }}>Dossiers de remboursement ({sortedCases.length})</h3>
          </div>
          <button
            className="primary-button is-create"
            type="button"
            onClick={() => setShowCaseModal(true)}
            disabled={pendingInvoices.length === 0}
          >
            Nouveau dossier
          </button>
        </div>
        <p className="muted">
          Workflow SPEC/14 — la soumission eFact est simulee tant que l&rsquo;integration eHealth n&rsquo;est pas
          validee (Q-B03).
        </p>

        {sortedCases.length === 0 ? (
          <p className="muted">Aucun dossier. Regroupe des factures en attente pour en creer un.</p>
        ) : (
          <div className="list">
            {sortedCases.map((reimbursementCase) => (
              <div key={reimbursementCase.id} className="list-item">
                <div style={{ flex: 1 }}>
                  <strong>
                    Dossier {reimbursementCase.submissionRef ?? `#${reimbursementCase.id.slice(0, 8)}`}
                  </strong>
                  <p className="muted compact">
                    {reimbursementCase.invoiceIds.length} facture(s) ·{' '}
                    {reimbursementCase.invoiceIds.map(invoiceLabel).join(' · ')}
                  </p>
                </div>
                <span className={caseStatusBadgeClass[reimbursementCase.status]}>
                  {caseStatusLabels[reimbursementCase.status]}
                </span>
                {allowedTransitions[reimbursementCase.status].length > 0 ? (
                  <div className="toolbar invoice-actions">
                    {allowedTransitions[reimbursementCase.status].map((transition) => (
                      <button
                        key={transition.target}
                        className="ghost-button"
                        type="button"
                        onClick={() => handleTransition(reimbursementCase, transition.target)}
                      >
                        {transition.label}
                      </button>
                    ))}
                  </div>
                ) : null}
              </div>
            ))}
          </div>
        )}
      </div>

      {showCreateModal ? (
        <Modal title="Nouvelle facture" onClose={() => setShowCreateModal(false)}>
          <form className="stack" onSubmit={handleCreate}>
            <div className="form-grid">
              <label>
                Patient
                <select
                  value={draft.patientId}
                  onChange={(event) => setDraft({ ...draft, patientId: event.target.value })}
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
                Acte INAMI
                <select
                  value={draft.codeInami}
                  onChange={(event) => setDraft({ ...draft, codeInami: event.target.value })}
                >
                  <option value="">Selectionner</option>
                  {actes.map((acte) => (
                    <option key={acte.code} value={acte.code}>
                      {acte.code} · {acte.label} · {formatMontant(acte.amount)}
                    </option>
                  ))}
                </select>
              </label>
            </div>
            <p className="muted compact">
              La mutuelle est reprise automatiquement du dossier patient; le montant est fixe par l&rsquo;acte INAMI.
            </p>
            <button
              className="primary-button is-create"
              type="submit"
              disabled={!draft.patientId || !draft.codeInami}
            >
              Creer la facture
            </button>
          </form>
        </Modal>
      ) : null}

      {showCaseModal ? (
        <Modal title="Nouveau dossier de remboursement" onClose={() => setShowCaseModal(false)}>
          <form className="stack" onSubmit={handleCreateCase}>
            <p className="muted compact">Selectionne les factures en attente a regrouper dans le dossier :</p>
            <div className="list">
              {pendingInvoices.map((invoice) => (
                <label key={invoice.id} className="list-item inline-check" style={{ cursor: 'pointer' }}>
                  <input
                    type="checkbox"
                    checked={caseInvoiceIds.includes(invoice.id)}
                    onChange={() => toggleCaseInvoice(invoice.id)}
                  />
                  <div style={{ flex: 1 }}>
                    <strong>{patientName(invoice.patientId)}</strong>
                    <p className="muted compact">
                      {invoice.codeInami} · {invoice.label} · {formatMontant(invoice.amount)}
                    </p>
                  </div>
                </label>
              ))}
            </div>
            <button className="primary-button is-create" type="submit" disabled={caseInvoiceIds.length === 0}>
              Creer le dossier
            </button>
          </form>
        </Modal>
      ) : null}
    </section>
  );
}
