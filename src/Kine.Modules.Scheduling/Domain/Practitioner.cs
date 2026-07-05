using System;

namespace Kine.Modules.Scheduling.Domain;

/// <summary>
/// Kinesitherapeute du cabinet (table practitioners). Tenant-scoped.
/// Registre minimal pour le multi-praticiens (F-B2): les slots et rendez-vous
/// referencent un praticien existant via PractitionerId (= Id.ToString()).
/// L'identite/authentification du staff reste du ressort du module Identity (OIDC).
/// </summary>
public sealed record Practitioner
{
    public required Guid Id { get; init; }
    public required string TenantId { get; init; }
    public required string FirstName { get; init; }
    public required string LastName { get; init; }

    /// <summary>Numero INAMI du kinesitherapeute (optionnel au MVP).</summary>
    public string? InamiNumber { get; init; }

    public required DateTime CreatedAtUtc { get; init; }
    public required DateTime UpdatedAtUtc { get; init; }
    public required string CreatedBy { get; init; }
}
