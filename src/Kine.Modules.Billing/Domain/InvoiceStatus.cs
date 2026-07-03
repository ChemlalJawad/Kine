namespace Kine.Modules.Billing.Domain;

/// <summary>
/// Statut de remboursement d'une facture (vision cabinet, MVP).
/// Version simplifiee du cycle ReimbursementCase de SPEC/14-reimbursement-rules.md :
/// le workflow complet (DRAFT/SUBMITTED/PENDING/...) appartient au futur module
/// Reimbursement (dependant de Q-B03 eFact/eAttest); ici on ne trace que l'etat
/// observable cote cabinet.
/// </summary>
public enum InvoiceStatus
{
    /// <summary>En attente de remboursement mutuelle/INAMI.</summary>
    Pending = 0,

    /// <summary>Remboursement recu.</summary>
    Reimbursed = 1,

    /// <summary>Rejete par l'organisme assureur.</summary>
    Rejected = 2
}
