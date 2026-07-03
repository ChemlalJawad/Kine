using System;
using System.Collections.Generic;
using System.Linq;
using Kine.Modules.Billing.Application;
using Kine.Modules.Billing.Domain;
using Kine.Modules.Billing.Infrastructure;
using Xunit;

namespace Kine.UnitTests;

public class BillingServiceTests
{
    private const string TenantId = "tenant-001";
    private const string Actor = "staff-1";
    private const string KnownCode = "558014";

    private static BillingService CreateService() => new(new InMemoryInvoiceStore());

    [Fact]
    public void Catalog_contains_actes_with_positive_amounts()
    {
        var actes = CreateService().ListActes();

        Assert.NotEmpty(actes);
        Assert.All(actes, acte =>
        {
            Assert.False(string.IsNullOrWhiteSpace(acte.Code));
            Assert.False(string.IsNullOrWhiteSpace(acte.Label));
            Assert.True(acte.Amount > 0);
        });
    }

    [Fact]
    public void Create_invoice_freezes_label_and_amount_from_catalog()
    {
        var service = CreateService();
        var acte = ActeInamiCatalog.Find(KnownCode)!;

        var invoice = service.CreateInvoice(TenantId, Guid.NewGuid(), KnownCode, "Solidaris", Actor);

        Assert.Equal(acte.Label, invoice.Label);
        Assert.Equal(acte.Amount, invoice.Amount);
        Assert.Equal("Solidaris", invoice.Mutuelle);
        Assert.Equal(InvoiceStatus.Pending, invoice.Status);
        Assert.Equal(Actor, invoice.CreatedBy);
    }

    [Fact]
    public void Create_invoice_rejects_unknown_inami_code()
    {
        var service = CreateService();

        Assert.Throws<ArgumentException>(() =>
            service.CreateInvoice(TenantId, Guid.NewGuid(), "000000", null, Actor));
    }

    [Fact]
    public void Create_invoice_rejects_empty_patient()
    {
        var service = CreateService();

        Assert.Throws<ArgumentException>(() =>
            service.CreateInvoice(TenantId, Guid.Empty, KnownCode, null, Actor));
    }

    [Fact]
    public void Create_invoice_rejects_missing_tenant()
    {
        var service = CreateService();

        Assert.Throws<ArgumentException>(() =>
            service.CreateInvoice(" ", Guid.NewGuid(), KnownCode, null, Actor));
    }

    [Fact]
    public void Blank_mutuelle_is_stored_as_null()
    {
        var service = CreateService();

        var invoice = service.CreateInvoice(TenantId, Guid.NewGuid(), KnownCode, "  ", Actor);

        Assert.Null(invoice.Mutuelle);
    }

    [Fact]
    public void Mark_reimbursed_transitions_pending_invoice()
    {
        var service = CreateService();
        var invoice = service.CreateInvoice(TenantId, Guid.NewGuid(), KnownCode, null, Actor);

        var updated = service.MarkReimbursed(TenantId, invoice.Id);

        Assert.Equal(InvoiceStatus.Reimbursed, updated.Status);
    }

    [Fact]
    public void Mark_rejected_transitions_pending_invoice()
    {
        var service = CreateService();
        var invoice = service.CreateInvoice(TenantId, Guid.NewGuid(), KnownCode, null, Actor);

        var updated = service.MarkRejected(TenantId, invoice.Id);

        Assert.Equal(InvoiceStatus.Rejected, updated.Status);
    }

    [Fact]
    public void Transition_from_non_pending_status_is_rejected()
    {
        var service = CreateService();
        var invoice = service.CreateInvoice(TenantId, Guid.NewGuid(), KnownCode, null, Actor);
        service.MarkReimbursed(TenantId, invoice.Id);

        Assert.Throws<InvalidOperationException>(() => service.MarkRejected(TenantId, invoice.Id));
    }

    [Fact]
    public void Transition_on_unknown_invoice_throws_not_found()
    {
        var service = CreateService();

        Assert.Throws<KeyNotFoundException>(() => service.MarkReimbursed(TenantId, Guid.NewGuid()));
    }

    [Fact]
    public void Invoices_are_isolated_between_tenants()
    {
        var service = CreateService();
        var invoice = service.CreateInvoice(TenantId, Guid.NewGuid(), KnownCode, null, Actor);

        Assert.Null(service.GetInvoice("tenant-other", invoice.Id));
        Assert.Empty(service.ListInvoices("tenant-other"));
        Assert.Single(service.ListInvoices(TenantId));
        Assert.Throws<KeyNotFoundException>(() => service.MarkReimbursed("tenant-other", invoice.Id));
    }

    [Fact]
    public void List_invoices_returns_all_for_tenant()
    {
        var service = CreateService();
        service.CreateInvoice(TenantId, Guid.NewGuid(), KnownCode, null, Actor);
        service.CreateInvoice(TenantId, Guid.NewGuid(), "558310", null, Actor);

        var invoices = service.ListInvoices(TenantId);

        Assert.Equal(2, invoices.Count);
        Assert.Equal(2, invoices.Select(invoice => invoice.CodeInami).Distinct().Count());
    }
}
