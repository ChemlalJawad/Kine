using System;
using System.Collections.Generic;
using Kine.Modules.Audit.Domain;

namespace Kine.Modules.Audit.Application;

/// <summary>
/// Append-only journal contract. Intentionally exposes no update or delete
/// operation: the only mutation allowed is appending a new event.
/// </summary>
public interface IAuditLogStore
{
    /// <summary>
    /// Atomically appends an event built from the current tail of the tenant's
    /// chain. The store resolves the previous event hash (or the genesis hash for
    /// an empty chain) and invokes <paramref name="buildFromPrevHash"/> while
    /// holding its internal synchronization, so two concurrent appends can never
    /// observe the same predecessor (which would fork the chain and make
    /// verification fail as a false tampering positive).
    /// </summary>
    AuditEvent AppendWithChain(string tenantId, Func<string, AuditEvent> buildFromPrevHash);

    IReadOnlyList<AuditEvent> GetChain(string tenantId);
}
