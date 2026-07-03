using System;
using System.Collections.Generic;
using Kine.Modules.Billing.Domain;

namespace Kine.Modules.Billing.Application;

/// <summary>
/// Orchestration de la facturation des seances : creation d'une facture par acte
/// INAMI et transitions de statut de remboursement (Pending -> Reimbursed/Rejected).
/// Tenant scoping is enforced on every operation (tenantId is a mandatory argument).
/// </summary>
public sealed class BillingService
{
    private readonly IInvoiceStore _store;

    public BillingService(IInvoiceStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    public IReadOnlyList<ActeInami> ListActes() => ActeInamiCatalog.All;

    public Invoice CreateInvoice(string tenantId, Guid patientId, string codeInami, string? mutuelle, string createdBy)
    {
        RequireTenant(tenantId);
        RequireNonEmpty(createdBy, nameof(createdBy));

        if (patientId == Guid.Empty)
        {
            throw new ArgumentException("Patient id is required.", nameof(patientId));
        }

        if (string.IsNullOrWhiteSpace(codeInami))
        {
            throw new ArgumentException("Code INAMI is required.", nameof(codeInami));
        }

        var acte = ActeInamiCatalog.Find(codeInami)
            ?? throw new ArgumentException($"Unknown INAMI code '{codeInami}'.", nameof(codeInami));

        var now = DateTime.UtcNow;
        var invoice = new Invoice
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PatientId = patientId,
            CodeInami = acte.Code,
            Label = acte.Label,
            Amount = acte.Amount,
            Mutuelle = string.IsNullOrWhiteSpace(mutuelle) ? null : mutuelle,
            Status = InvoiceStatus.Pending,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            CreatedBy = createdBy
        };

        _store.Add(invoice);
        return invoice;
    }

    public Invoice? GetInvoice(string tenantId, Guid invoiceId)
    {
        RequireTenant(tenantId);
        return _store.Get(tenantId, invoiceId);
    }

    public IReadOnlyList<Invoice> ListInvoices(string tenantId)
    {
        RequireTenant(tenantId);
        return _store.GetAll(tenantId);
    }

    /// <summary>Marque une facture en attente comme remboursee.</summary>
    public Invoice MarkReimbursed(string tenantId, Guid invoiceId) =>
        Transition(tenantId, invoiceId, InvoiceStatus.Reimbursed);

    /// <summary>Marque une facture en attente comme rejetee par l'organisme.</summary>
    public Invoice MarkRejected(string tenantId, Guid invoiceId) =>
        Transition(tenantId, invoiceId, InvoiceStatus.Rejected);

    private Invoice Transition(string tenantId, Guid invoiceId, InvoiceStatus target)
    {
        RequireTenant(tenantId);

        var existing = _store.Get(tenantId, invoiceId)
            ?? throw new KeyNotFoundException($"Invoice '{invoiceId}' not found for tenant '{tenantId}'.");

        if (existing.Status != InvoiceStatus.Pending)
        {
            throw new InvalidOperationException(
                $"Invoice '{invoiceId}' cannot transition to '{target}' from status '{existing.Status}'.");
        }

        var updated = existing with { Status = target, UpdatedAtUtc = DateTime.UtcNow };
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

    private static void RequireNonEmpty(string value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value is required.", paramName);
        }
    }
}
