using System;
using System.Collections.Generic;
using Kine.Modules.Audit.Application;
using Kine.Modules.Billing.Application;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Kine.Api.Modules;

/// <summary>
/// Minimal HTTP surface for the Billing module: catalogue d'actes INAMI,
/// creation de factures, transitions de remboursement. Tenant id is read from
/// the request context (set by TenantContextMiddleware).
/// Sensitive mutations are recorded in the append-only audit journal (P0-008).
/// </summary>
public static class BillingEndpoints
{
    private const string TenantItemKey = "TenantId";
    private const string ActorHeaderName = "X-Actor-Id";
    private const string DefaultActor = "system";

    public static void MapBillingEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/billing");

        group.MapGet("/actes", (BillingService service) =>
        {
            return Results.Ok(service.ListActes());
        });

        group.MapGet("/invoices", (BillingService service, HttpContext context) =>
        {
            return Results.Ok(service.ListInvoices(Tenant(context)));
        });

        group.MapGet("/invoices/{id:guid}", (Guid id, BillingService service, HttpContext context) =>
        {
            var invoice = service.GetInvoice(Tenant(context), id);
            return invoice is null ? Results.NotFound() : Results.Ok(invoice);
        });

        group.MapPost("/invoices", (CreateInvoiceRequest request, BillingService service, AuditTrailService audit, HttpContext context) =>
        {
            try
            {
                var invoice = service.CreateInvoice(
                    Tenant(context), request.PatientId, request.CodeInami, request.Mutuelle, Actor(context));
                audit.Record(Tenant(context), Actor(context), "invoice_created", "Invoice", invoice.Id.ToString(),
                    $"patient={request.PatientId}; code={invoice.CodeInami}; amount={invoice.Amount}");
                return Results.Created($"/api/billing/invoices/{invoice.Id}", invoice);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        group.MapPost("/invoices/{id:guid}/mark-reimbursed", (Guid id, BillingService service, AuditTrailService audit, HttpContext context) =>
        {
            return Transition(context, audit, "invoice_reimbursed", id, () => service.MarkReimbursed(Tenant(context), id));
        });

        group.MapPost("/invoices/{id:guid}/mark-rejected", (Guid id, BillingService service, AuditTrailService audit, HttpContext context) =>
        {
            return Transition(context, audit, "invoice_rejected", id, () => service.MarkRejected(Tenant(context), id));
        });
    }

    private static IResult Transition(HttpContext context, AuditTrailService audit, string action, Guid invoiceId, Func<object> transition)
    {
        try
        {
            var result = transition();
            audit.Record(Tenant(context), Actor(context), action, "Invoice", invoiceId.ToString());
            return Results.Ok(result);
        }
        catch (KeyNotFoundException)
        {
            return Results.NotFound();
        }
        catch (InvalidOperationException ex)
        {
            return Results.Conflict(new { error = ex.Message });
        }
    }

    private static string Tenant(HttpContext context) => (string)context.Items[TenantItemKey]!;

    private static string Actor(HttpContext context) =>
        context.Request.Headers.TryGetValue(ActorHeaderName, out var actor) && !string.IsNullOrWhiteSpace(actor)
            ? actor.ToString()
            : DefaultActor;
}
