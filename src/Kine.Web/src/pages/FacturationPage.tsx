type InvoiceStatus = 'Rembourse' | 'En attente' | 'Rejete';

type Invoice = {
  patient: string;
  code: string;
  label: string;
  montant: string;
  mutuelle: string;
  status: InvoiceStatus;
};

const statusBadgeClass: Record<InvoiceStatus, string> = {
  Rembourse: 'badge badge-success',
  'En attente': 'badge badge-warning',
  Rejete: 'badge badge-danger'
};

const invoices: Invoice[] = [
  {
    patient: 'Sophie Vandenberghe',
    code: '558014',
    label: 'Seance kine — pathologie lourde',
    montant: '23,45€',
    mutuelle: 'Mutualite Chretienne',
    status: 'Rembourse'
  },
  {
    patient: 'Marc Lemmens',
    code: '558310',
    label: 'Reeducation post-operatoire',
    montant: '21,80€',
    mutuelle: 'Solidaris',
    status: 'En attente'
  },
  {
    patient: 'Amina El Amrani',
    code: '558891',
    label: 'Kinesitherapie respiratoire',
    montant: '19,60€',
    mutuelle: 'Partena',
    status: 'En attente'
  },
  {
    patient: 'Louis Dupont',
    code: '558014',
    label: 'Seance kine — pathologie lourde',
    montant: '23,45€',
    mutuelle: 'Helan',
    status: 'Rejete'
  }
];

export function FacturationPage() {
  return (
    <section className="stack">
      <p className="muted">
        Apercu de demonstration — le module Facturation &amp; Remboursement (INAMI) n&rsquo;est pas encore
        connecte a un backend.
      </p>

      <div className="panel" style={{ maxWidth: 'none' }}>
        <h3 style={{ marginTop: 0 }}>Facturation &amp; remboursements</h3>
        <div className="invoice-table">
          <div className="invoice-row invoice-header">
            <span>Patient</span>
            <span>Code</span>
            <span>Acte</span>
            <span>Montant</span>
            <span>Mutuelle</span>
            <span>Statut</span>
          </div>
          {invoices.map((invoice) => (
            <div key={`${invoice.patient}-${invoice.code}`} className="invoice-row">
              <strong>{invoice.patient}</strong>
              <span className="muted">{invoice.code}</span>
              <span>{invoice.label}</span>
              <span>{invoice.montant}</span>
              <span className="muted">{invoice.mutuelle}</span>
              <span className={statusBadgeClass[invoice.status]}>{invoice.status}</span>
            </div>
          ))}
        </div>
      </div>
    </section>
  );
}
