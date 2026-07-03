using System;
using System.Collections.Generic;
using Kine.Modules.Reimbursement.Domain;

namespace Kine.Modules.Reimbursement.Application;

/// <summary>
/// Persistence contract for reimbursement cases. In-memory for MVP; the
/// PostgreSQL/RLS-backed implementation will plug in behind the same interface.
/// </summary>
public interface IReimbursementCaseStore
{
    void Add(ReimbursementCase reimbursementCase);
    ReimbursementCase? Get(string tenantId, Guid caseId);
    IReadOnlyList<ReimbursementCase> GetAll(string tenantId);
    void Update(ReimbursementCase reimbursementCase);
}
