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

    public void Append(AuditEvent auditEvent)
    {
        if (auditEvent is null)
        {
            throw new ArgumentNullException(nameof(auditEvent));
        }

        lock (_lock)
        {
            if (!_eventsByTenant.TryGetValue(auditEvent.TenantId, out var events))
            {
                events = new List<AuditEvent>();
                _eventsByTenant[auditEvent.TenantId] = events;
            }

            events.Add(auditEvent);
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
