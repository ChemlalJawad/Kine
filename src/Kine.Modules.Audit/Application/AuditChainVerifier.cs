using System.Collections.Generic;
using Kine.Modules.Audit.Domain;

namespace Kine.Modules.Audit.Application;

/// <summary>
/// Verifies the integrity of an audit chain: each event must reference the
/// previous event's hash, and its own event_hash must recompute identically.
/// Any mismatch indicates alteration of the journal.
/// </summary>
public static class AuditChainVerifier
{
    public static bool IsValid(IReadOnlyList<AuditEvent> chain)
    {
        var expectedPrevHash = AuditHash.GenesisHash;

        foreach (var auditEvent in chain)
        {
            if (auditEvent.PrevHash != expectedPrevHash)
            {
                return false;
            }

            var recomputedEventHash = AuditHash.ComputeEventHash(
                auditEvent.Id,
                auditEvent.TenantId,
                auditEvent.ActorId,
                auditEvent.Action,
                auditEvent.Entity,
                auditEvent.EntityId,
                auditEvent.TsUtc,
                auditEvent.PayloadHash,
                auditEvent.PrevHash);

            if (recomputedEventHash != auditEvent.EventHash)
            {
                return false;
            }

            expectedPrevHash = auditEvent.EventHash;
        }

        return true;
    }
}
