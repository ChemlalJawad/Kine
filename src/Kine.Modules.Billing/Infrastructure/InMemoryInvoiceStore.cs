using System;
using System.Collections.Generic;
using System.Linq;
using Kine.Modules.Billing.Application;
using Kine.Modules.Billing.Domain;

namespace Kine.Modules.Billing.Infrastructure;

/// <summary>
/// In-memory tenant-scoped store for invoices.
/// MVP persistence stand-in pending the PostgreSQL/RLS-backed implementation.
/// </summary>
public sealed class InMemoryInvoiceStore : IInvoiceStore
{
    private readonly object _lock = new();
    private readonly Dictionary<string, Dictionary<Guid, Invoice>> _invoicesByTenant = new();

    public void Add(Invoice invoice)
    {
        lock (_lock)
        {
            GetOrCreate(invoice.TenantId)[invoice.Id] = invoice;
        }
    }

    public Invoice? Get(string tenantId, Guid invoiceId)
    {
        lock (_lock)
        {
            return _invoicesByTenant.TryGetValue(tenantId, out var invoices) && invoices.TryGetValue(invoiceId, out var invoice)
                ? invoice
                : null;
        }
    }

    public IReadOnlyList<Invoice> GetAll(string tenantId)
    {
        lock (_lock)
        {
            return _invoicesByTenant.TryGetValue(tenantId, out var invoices)
                ? invoices.Values.ToList()
                : Array.Empty<Invoice>();
        }
    }

    public void Update(Invoice invoice)
    {
        lock (_lock)
        {
            GetOrCreate(invoice.TenantId)[invoice.Id] = invoice;
        }
    }

    private Dictionary<Guid, Invoice> GetOrCreate(string tenantId)
    {
        if (!_invoicesByTenant.TryGetValue(tenantId, out var invoices))
        {
            invoices = new Dictionary<Guid, Invoice>();
            _invoicesByTenant[tenantId] = invoices;
        }

        return invoices;
    }
}
