using System;

namespace Kine.Modules.Scheduling.Domain;

/// <summary>
/// Bookable availability slot for a practitioner (table practitioner_slots). Tenant-scoped.
/// A slot is either free (IsBooked = false) or consumed by an appointment (IsBooked = true).
/// </summary>
public sealed record PractitionerSlot
{
    public required Guid Id { get; init; }
    public required string TenantId { get; init; }
    public required string PractitionerId { get; init; }
    public required DateTime StartAtUtc { get; init; }
    public required DateTime EndAtUtc { get; init; }
    public required bool IsBooked { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public required DateTime UpdatedAtUtc { get; init; }
    public required string CreatedBy { get; init; }
}
