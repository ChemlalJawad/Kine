using System;

namespace Kine.Modules.Patients.Domain;

/// <summary>
/// Patient consent record (table patient_consents). Tenant-scoped, linked to a patient.
/// Consent is never deleted: revocation is tracked via RevokedAtUtc to preserve historique.
/// </summary>
public sealed record PatientConsent
{
    public required Guid Id { get; init; }
    public required string TenantId { get; init; }
    public required Guid PatientId { get; init; }
    public required ConsentType Type { get; init; }
    public required bool Granted { get; init; }
    public required DateTime GrantedAtUtc { get; init; }
    public DateTime? RevokedAtUtc { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public required DateTime UpdatedAtUtc { get; init; }
    public required string CreatedBy { get; init; }
}
