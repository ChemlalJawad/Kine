using System;
using System.Collections.Generic;
using Kine.Modules.Audit.Application;
using Kine.Modules.Reimbursement.Application;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Kine.Api.Modules;

/// <summary>
/// Minimal HTTP surface for the Reimbursement module: dossiers remboursement et
/// transitions d'etat (state machine SPEC/14, soumission eFact mockee - Q-B03).
/// Chaque creation/transition est auditee, conformement au format de SPEC/14.
/// </summary>
public static class ReimbursementEndpoints
{
    private const string TenantItemKey = "TenantId";
    private const string ActorHeaderName = "X-Actor-Id";
    private const string DefaultActor = "system";

    public static void MapReimbursementEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/reimbursement");

        group.MapGet("/cases", (ReimbursementService service, HttpContext context) =>
        {
            return Results.Ok(service.ListCases(Tenant(context)));
        });

        group.MapGet("/cases/{id:guid}", (Guid id, ReimbursementService service, HttpContext context) =>
        {
            var reimbursementCase = service.GetCase(Tenant(context), id);
            return reimbursementCase is null ? Results.NotFound() : Results.Ok(reimbursementCase);
        });

        group.MapPost("/cases", (
            CreateReimbursementCaseRequest request,
            ReimbursementService service,
            AuditTrailService audit,
            HttpContext context) =>
        {
            try
            {
                var reimbursementCase = service.CreateCase(Tenant(context), request.InvoiceIds, Actor(context));
                audit.Record(Tenant(context), Actor(context), "reimbursement_case_created", "ReimbursementCase",
                    reimbursementCase.Id.ToString(), $"invoices={reimbursementCase.InvoiceIds.Count}");
                return Results.Created($"/api/reimbursement/cases/{reimbursementCase.Id}", reimbursementCase);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        group.MapPost("/cases/{id:guid}/status", (
            Guid id,
            TransitionReimbursementCaseRequest request,
            ReimbursementService service,
            AuditTrailService audit,
            HttpContext context) =>
        {
            try
            {
                var before = service.GetCase(Tenant(context), id);
                var updated = service.Transition(Tenant(context), id, request.Target);
                audit.Record(Tenant(context), Actor(context), "reimbursement_case_status_changed", "ReimbursementCase",
                    updated.Id.ToString(),
                    $"previous_status={before?.Status}; new_status={updated.Status}; submissionRef={updated.SubmissionRef}");
                return Results.Ok(updated);
            }
            catch (KeyNotFoundException)
            {
                return Results.NotFound();
            }
            catch (InvalidOperationException ex)
            {
                return Results.Conflict(new { error = ex.Message });
            }
        });
    }

    private static string Tenant(HttpContext context) => (string)context.Items[TenantItemKey]!;

    private static string Actor(HttpContext context) =>
        context.Request.Headers.TryGetValue(ActorHeaderName, out var actor) && !string.IsNullOrWhiteSpace(actor)
            ? actor.ToString()
            : DefaultActor;
}
