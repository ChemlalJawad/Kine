using System;

namespace Kine.Modules.Patients.Domain;

/// <summary>
/// Patient contact record (table patient_contacts). Tenant-scoped, linked to a patient.
/// </summary>
public sealed record PatientContact
{
    public required Guid Id { get; init; }
    public required string TenantId { get; init; }
    public required Guid PatientId { get; init; }
    public required PatientContactType Type { get; init; }
    public required string Value { get; init; }
    public bool IsPrimary { get; init; }
    public required DateTime CreatedAtUtc { get; init; }
    public required DateTime UpdatedAtUtc { get; init; }
    public required string CreatedBy { get; init; }
}
