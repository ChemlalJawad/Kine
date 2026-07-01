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
    public required DateTime CreatedAtUtc { get; init; }
    public required DateTime UpdatedAtUtc { get; init; }
    public required string CreatedBy { get; init; }
}
