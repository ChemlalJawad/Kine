using System;
using System.Collections.Generic;

namespace Kine.Modules.Reimbursement.Domain;

/// <summary>
/// Dossier remboursement INAMI (table reimbursement_cases). Tenant-scoped.
/// Regroupe une ou plusieurs factures (ids opaques du module Billing, pas de
/// reference projet croisee) et suit la machine a etats de SPEC/14.
/// Toute transition est auditee (reimbursement_case_status_changed).
/// </summary>
public sealed record ReimbursementCase
{
    public required Guid Id { get; init; }
    public required string TenantId { get; init; }

    /// <summary>Factures rattachees (ids du module Billing).</summary>
    public required IReadOnlyList<Guid> InvoiceIds { get; init; }

    public required ReimbursementCaseStatus Status { get; init; }

    /// <summary>Reference de soumission eFact (mock "EFACT-yyyy-xxxxxxxx" tant que Q-B03 est ouvert).</summary>
    public string? SubmissionRef { get; init; }

    /// <summary>Reponse INAMI brute si recue (JSON texte libre; null en mock).</summary>
    public string? InamiResponse { get; init; }

    public required DateTime CreatedAtUtc { get; init; }
    public required DateTime UpdatedAtUtc { get; init; }
    public required string CreatedBy { get; init; }
}
