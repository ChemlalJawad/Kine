using System;

namespace Kine.Modules.Patients.Domain;

/// <summary>
/// Patient record (table patients). Tenant-scoped, status-driven lifecycle.
/// Delete is a soft-archive (Status = Archived) to preserve historique and
/// avoid conflicting with the unresolved RGPD erasure workflow (SPEC Q-B15).
/// </summary>
public sealed record Patient
{
    public required Guid Id { get; init; }
    public required string TenantId { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }
    public DateOnly? DateOfBirth { get; init; }
    public required PatientStatus Status { get; init; }

    /// <summary>Mutuelle (organisme assureur) declaree par le patient. Aucune validation INAMI pour l'instant.</summary>
    public string? Mutuelle { get; init; }

    /// <summary>Diagnostic/motif de prise en charge en texte libre, en attendant le module Clinical.</summary>
    public string? Diagnosis { get; init; }

    /// <summary>Nombre de seances prescrites; placeholder MVP en attendant un vrai plan de traitement (module Clinical).</summary>
    public int SessionsPrescribed { get; init; }

    /// <summary>Nombre de seances deja realisees; placeholder MVP, non derive des rendez-vous reels pour l'instant.</summary>
    public int SessionsDone { get; init; }

    public required DateTime CreatedAtUtc { get; init; }
    public required DateTime UpdatedAtUtc { get; init; }
    public required string CreatedBy { get; init; }
}
