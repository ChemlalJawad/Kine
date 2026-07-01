using System;

namespace Kine.Modules.Scheduling.Domain;

/// <summary>
/// Appointment record (table appointments). Tenant-scoped, status-driven lifecycle
/// (Scheduled -> Cancelled | NoShow | Completed). Cancellation/no-show are recorded
/// as status transitions rather than deletions to preserve historique (SPEC/02).
/// </summary>
public sealed record Appointment
{
    public required Guid Id { get; init; }
    public required string TenantId { get; init; }
    public required Guid PatientId { get; init; }
    public required string PractitionerId { get; init; }
    public required Guid SlotId { get; init; }
    public required DateTime StartAtUtc { get; init; }
    public required DateTime EndAtUtc { get; init; }
    public required AppointmentStatus Status { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public required DateTime UpdatedAtUtc { get; init; }
    public required string CreatedBy { get; init; }
}
