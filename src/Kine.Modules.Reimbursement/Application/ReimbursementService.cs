using System;
using System.Collections.Generic;
using System.Linq;
using Kine.Modules.Reimbursement.Domain;

namespace Kine.Modules.Reimbursement.Application;

/// <summary>
/// Orchestration des dossiers remboursement : creation (Draft) et transitions
/// d'etat strictement conformes a la machine a etats de SPEC/14.
/// La soumission eFact est mockee (Q-B03 ouvert) : passer a Submitted genere une
/// SubmissionRef locale, aucune integration eHealth reelle n'est appelee.
/// Tenant scoping is enforced on every operation.
/// </summary>
public sealed class ReimbursementService
{
    /// <summary>Transitions autorisees (SPEC/14-reimbursement-rules.md).</summary>
    private static readonly IReadOnlyDictionary<ReimbursementCaseStatus, ReimbursementCaseStatus[]> AllowedTransitions =
        new Dictionary<ReimbursementCaseStatus, ReimbursementCaseStatus[]>
        {
            [ReimbursementCaseStatus.Draft] = new[] { ReimbursementCaseStatus.Submitted },
            [ReimbursementCaseStatus.Submitted] = new[] { ReimbursementCaseStatus.Pending },
            [ReimbursementCaseStatus.Pending] = new[]
            {
                ReimbursementCaseStatus.Approved,
                ReimbursementCaseStatus.Rejected,
                ReimbursementCaseStatus.CorrectionRequired
            },
            [ReimbursementCaseStatus.CorrectionRequired] = new[] { ReimbursementCaseStatus.Corrected },
            [ReimbursementCaseStatus.Corrected] = new[] { ReimbursementCaseStatus.Submitted },
            [ReimbursementCaseStatus.Approved] = new[] { ReimbursementCaseStatus.Completed },
            [ReimbursementCaseStatus.Rejected] = new[] { ReimbursementCaseStatus.Completed },
            [ReimbursementCaseStatus.Completed] = new[] { ReimbursementCaseStatus.Archived },
            [ReimbursementCaseStatus.Archived] = Array.Empty<ReimbursementCaseStatus>()
        };

    private readonly IReimbursementCaseStore _store;

    public ReimbursementService(IReimbursementCaseStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    public static bool IsTransitionAllowed(ReimbursementCaseStatus from, ReimbursementCaseStatus to) =>
        AllowedTransitions.TryGetValue(from, out var targets) && targets.Contains(to);

    public ReimbursementCase CreateCase(string tenantId, IReadOnlyList<Guid> invoiceIds, string createdBy)
    {
        RequireTenant(tenantId);

        if (string.IsNullOrWhiteSpace(createdBy))
        {
            throw new ArgumentException("Value is required.", nameof(createdBy));
        }

        if (invoiceIds is null || invoiceIds.Count == 0 || invoiceIds.Any(id => id == Guid.Empty))
        {
            throw new ArgumentException("At least one valid invoice id is required.", nameof(invoiceIds));
        }

        var now = DateTime.UtcNow;
        var reimbursementCase = new ReimbursementCase
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            InvoiceIds = invoiceIds.Distinct().ToList(),
            Status = ReimbursementCaseStatus.Draft,
            SubmissionRef = null,
            InamiResponse = null,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            CreatedBy = createdBy
        };

        _store.Add(reimbursementCase);
        return reimbursementCase;
    }

    public ReimbursementCase? GetCase(string tenantId, Guid caseId)
    {
        RequireTenant(tenantId);
        return _store.Get(tenantId, caseId);
    }

    public IReadOnlyList<ReimbursementCase> ListCases(string tenantId)
    {
        RequireTenant(tenantId);
        return _store.GetAll(tenantId);
    }

    /// <summary>
    /// Applique une transition d'etat validee par la state machine SPEC/14.
    /// Le passage a Submitted genere une SubmissionRef mock si absente (Q-B03).
    /// </summary>
    public ReimbursementCase Transition(string tenantId, Guid caseId, ReimbursementCaseStatus target)
    {
        RequireTenant(tenantId);

        var existing = _store.Get(tenantId, caseId)
            ?? throw new KeyNotFoundException($"Reimbursement case '{caseId}' not found for tenant '{tenantId}'.");

        if (!IsTransitionAllowed(existing.Status, target))
        {
            throw new InvalidOperationException(
                $"Reimbursement case '{caseId}' cannot transition from '{existing.Status}' to '{target}'.");
        }

        var now = DateTime.UtcNow;
        var submissionRef = existing.SubmissionRef;
        if (target == ReimbursementCaseStatus.Submitted && string.IsNullOrEmpty(submissionRef))
        {
            // Mock eFact reference (Q-B03 open: no real eHealth integration allowed yet).
            submissionRef = $"EFACT-{now:yyyy}-{Guid.NewGuid().ToString("N")[..8].ToUpperInvariant()}";
        }

        var updated = existing with
        {
            Status = target,
            SubmissionRef = submissionRef,
            UpdatedAtUtc = now
        };

        _store.Update(updated);
        return updated;
    }

    private static void RequireTenant(string tenantId)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            throw new ArgumentException("Tenant id is required.", nameof(tenantId));
        }
    }
}
