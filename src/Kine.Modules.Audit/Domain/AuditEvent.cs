using System;

namespace Kine.Modules.Audit.Domain;

/// <summary>
/// Immutable audit event. Represents one entry of the append-only journal
/// (audit_logs_append_only). Never updated or deleted once produced.
/// </summary>
public sealed record AuditEvent
{
    public required Guid Id { get; init; }
    public required string TenantId { get; init; }
    public required string ActorId { get; init; }
    public required string Action { get; init; }
    public required string Entity { get; init; }
    public required string EntityId { get; init; }
    public required DateTime TsUtc { get; init; }
    public required string PayloadHash { get; init; }
    public required string PrevHash { get; init; }
    public required string EventHash { get; init; }
}
