using System;
using System.Collections.Generic;
using System.Linq;
using Kine.Modules.Reimbursement.Application;
using Kine.Modules.Reimbursement.Domain;

namespace Kine.Modules.Reimbursement.Infrastructure;

/// <summary>
/// In-memory tenant-scoped store for reimbursement cases.
/// MVP persistence stand-in pending the PostgreSQL/RLS-backed implementation.
/// </summary>
public sealed class InMemoryReimbursementCaseStore : IReimbursementCaseStore
{
    private readonly object _lock = new();
    private readonly Dictionary<string, Dictionary<Guid, ReimbursementCase>> _casesByTenant = new();

    public void Add(ReimbursementCase reimbursementCase)
    {
        lock (_lock)
        {
            GetOrCreate(reimbursementCase.TenantId)[reimbursementCase.Id] = reimbursementCase;
        }
    }

    public ReimbursementCase? Get(string tenantId, Guid caseId)
    {
        lock (_lock)
        {
            return _casesByTenant.TryGetValue(tenantId, out var cases) && cases.TryGetValue(caseId, out var reimbursementCase)
                ? reimbursementCase
                : null;
        }
    }

    public IReadOnlyList<ReimbursementCase> GetAll(string tenantId)
    {
        lock (_lock)
        {
            return _casesByTenant.TryGetValue(tenantId, out var cases)
                ? cases.Values.ToList()
                : Array.Empty<ReimbursementCase>();
        }
    }

    public void Update(ReimbursementCase reimbursementCase)
    {
        lock (_lock)
        {
            GetOrCreate(reimbursementCase.TenantId)[reimbursementCase.Id] = reimbursementCase;
        }
    }

    private Dictionary<Guid, ReimbursementCase> GetOrCreate(string tenantId)
    {
        if (!_casesByTenant.TryGetValue(tenantId, out var cases))
        {
            cases = new Dictionary<Guid, ReimbursementCase>();
            _casesByTenant[tenantId] = cases;
        }

        return cases;
    }
}
