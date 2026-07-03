using System;

namespace Kine.Modules.Billing.Domain;

/// <summary>
/// Facture de seance (table invoices). Tenant-scoped. MVP : 1 facture = 1 acte
/// INAMI unique (pas de cumul d'actes, montant officiel fixe, arrondi 0.01 EUR),
/// conformement a SPEC/14-reimbursement-rules.md.
/// La mutuelle est copiee depuis le dossier patient au moment de la facturation
/// (denormalisation volontaire : pas d'acces direct au module Patients, et la
/// facture doit rester fidele a la situation du patient a la date de l'acte).
/// </summary>
public sealed record Invoice
{
    public required Guid Id { get; init; }
    public required string TenantId { get; init; }
    public required Guid PatientId { get; init; }

    /// <summary>Code acte INAMI (ex. 560011). Valide contre ActeInamiCatalog a la creation.</summary>
    public required string CodeInami { get; init; }

    /// <summary>Libelle de l'acte, fige depuis le catalogue a la creation.</summary>
    public required string Label { get; init; }

    /// <summary>Montant facture en EUR, fige depuis le catalogue a la creation.</summary>
    public required decimal Amount { get; init; }

    /// <summary>Mutuelle du patient au moment de la facturation (texte libre, voir Patient.Mutuelle).</summary>
    public string? Mutuelle { get; init; }

    public required InvoiceStatus Status { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public required DateTime UpdatedAtUtc { get; init; }
    public required string CreatedBy { get; init; }
}
