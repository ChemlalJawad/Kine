using System;
using Kine.Modules.Audit.Domain;

namespace Kine.Modules.Audit.Application;

/// <summary>
/// Produces audit events chained by hash (prev_hash, event_hash) per tenant,
/// then appends them to the append-only store.
/// </summary>
public sealed class AuditTrailService
{
    private readonly IAuditLogStore _store;

    public AuditTrailService(IAuditLogStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    public AuditEvent Record(string tenantId, string actorId, string action, string entity, string entityId, string? payload = null)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            throw new ArgumentException("Tenant id is required.", nameof(tenantId));
        }

        if (string.IsNullOrWhiteSpace(action))
        {
            throw new ArgumentException("Action is required.", nameof(action));
        }

        var id = Guid.NewGuid();
        var tsUtc = DateTime.UtcNow;
        var payloadHash = AuditHash.HashPayload(payload);

        // Hash computation happens inside the store's atomic append: the store
        // provides the authoritative prev hash while holding its lock, so
        // concurrent Record calls can never chain onto the same predecessor.
        return _store.AppendWithChain(tenantId, prevHash =>
        {
            var eventHash = AuditHash.ComputeEventHash(id, tenantId, actorId, action, entity, entityId, tsUtc, payloadHash, prevHash);

            return new AuditEvent
            {
                Id = id,
                TenantId = tenantId,
                ActorId = actorId,
                Action = action,
                Entity = entity,
                EntityId = entityId,
                TsUtc = tsUtc,
                PayloadHash = payloadHash,
                PrevHash = prevHash,
                EventHash = eventHash
            };
        });
    }
}
