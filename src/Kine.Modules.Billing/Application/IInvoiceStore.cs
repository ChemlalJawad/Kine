using System;
using System.Collections.Generic;
using Kine.Modules.Billing.Domain;

namespace Kine.Modules.Billing.Application;

/// <summary>
/// Persistence contract for invoices. In-memory for MVP; the PostgreSQL/RLS-backed
/// implementation will plug in behind the same interface.
/// </summary>
public interface IInvoiceStore
{
    void Add(Invoice invoice);
    Invoice? Get(string tenantId, Guid invoiceId);
    IReadOnlyList<Invoice> GetAll(string tenantId);
    void Update(Invoice invoice);
}
