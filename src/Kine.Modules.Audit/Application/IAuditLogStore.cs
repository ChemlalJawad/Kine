using System.Collections.Generic;
using Kine.Modules.Audit.Domain;

namespace Kine.Modules.Audit.Application;

/// <summary>
/// Append-only journal contract. Intentionally exposes no update or delete
/// operation: the only mutation allowed is appending a new event.
/// </summary>
public interface IAuditLogStore
{
    void Append(AuditEvent auditEvent);

    IReadOnlyList<AuditEvent> GetChain(string tenantId);
}
