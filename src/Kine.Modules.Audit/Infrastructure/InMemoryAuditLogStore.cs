using System;
using System.Collections.Generic;
using System.Linq;
using Kine.Modules.Audit.Application;
using Kine.Modules.Audit.Domain;

namespace Kine.Modules.Audit.Infrastructure;

/// <summary>
/// In-memory append-only store, isolated per tenant. No API is exposed to
/// update or remove an event; GetChain returns a defensive snapshot copy so
/// callers cannot mutate the underlying journal.
/// </summary>
public sealed class InMemoryAuditLogStore : IAuditLogStore
{
    private readonly object _lock = new();
    private readonly Dictionary<string, List<AuditEvent>> _eventsByTenant = new();

    public AuditEvent AppendWithChain(string tenantId, Func<string, AuditEvent> buildFromPrevHash)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            throw new ArgumentException("Tenant id is required.", nameof(tenantId));
        }

        if (buildFromPrevHash is null)
        {
            throw new ArgumentNullException(nameof(buildFromPrevHash));
        }

        // The factory runs inside the lock on purpose: resolving the previous
        // hash and appending the new event must be a single atomic step, or two
        // concurrent writers fork the chain (same PrevHash twice) and
        // AuditChainVerifier reports a false tampering positive forever.
        lock (_lock)
        {
            if (!_eventsByTenant.TryGetValue(tenantId, out var events))
            {
                events = new List<AuditEvent>();
                _eventsByTenant[tenantId] = events;
            }

            var prevHash = events.Count > 0 ? events[^1].EventHash : AuditHash.GenesisHash;
            var auditEvent = buildFromPrevHash(prevHash);

            if (auditEvent is null)
            {
                throw new InvalidOperationException("Audit event factory returned null.");
            }

            if (auditEvent.TenantId != tenantId)
            {
                throw new InvalidOperationException("Audit event tenant does not match the chain tenant.");
            }

            if (auditEvent.PrevHash != prevHash)
            {
                throw new InvalidOperationException("Audit event prev hash does not match the chain tail.");
            }

            events.Add(auditEvent);
            return auditEvent;
        }
    }

    public IReadOnlyList<AuditEvent> GetChain(string tenantId)
    {
        lock (_lock)
        {
            if (!_eventsByTenant.TryGetValue(tenantId, out var events))
            {
                return Array.Empty<AuditEvent>();
            }

            return events.ToList();
        }
    }
}
