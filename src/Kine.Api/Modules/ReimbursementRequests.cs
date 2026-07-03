using System;
using System.Collections.Generic;
using Kine.Modules.Reimbursement.Domain;

namespace Kine.Api.Modules;

/// <summary>
/// Request payloads for the Reimbursement HTTP surface (Kine.Api layer).
/// </summary>
public sealed record CreateReimbursementCaseRequest(IReadOnlyList<Guid> InvoiceIds);
public sealed record TransitionReimbursementCaseRequest(ReimbursementCaseStatus Target);
