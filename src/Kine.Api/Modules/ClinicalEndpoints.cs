using System;
using Kine.Modules.Audit.Application;
using Kine.Modules.Clinical.Application;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Kine.Api.Modules;

/// <summary>
/// Minimal HTTP surface for the Clinical module v1: seances cliniques par patient.
/// Tenant id is read from the request context (set by TenantContextMiddleware).
/// Creation is audited (seance_created) per SPEC dev-standards.
/// </summary>
public static class ClinicalEndpoints
{
    private const string TenantItemKey = "TenantId";
    private const string ActorHeaderName = "X-Actor-Id";
    private const string DefaultActor = "system";

    public static void MapClinicalEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/clinical");

        group.MapGet("/patients/{patientId:guid}/seances", (Guid patientId, ClinicalService service, HttpContext context) =>
        {
            return Results.Ok(service.ListSeances(Tenant(context), patientId));
        });

        group.MapPost("/patients/{patientId:guid}/seances", (
            Guid patientId,
            CreateSeanceRequest request,
            ClinicalService service,
            AuditTrailService audit,
            HttpContext context) =>
        {
            // Erreurs mappees par ExceptionMappingMiddleware: 400 (invalide),
            // 404 (prescription inconnue/autre patient), 409 (expiree/quota).
            var seance = service.CreateSeance(
                Tenant(context), patientId, request.DateSeanceUtc, request.Note, Actor(context), request.AppointmentId, request.PrescriptionId);
            audit.Record(Tenant(context), Actor(context), "seance_created", "SeanceClinique", seance.Id.ToString(),
                $"patient={patientId}" + (request.PrescriptionId is Guid p ? $"; prescription={p}" : string.Empty));
            return Results.Created($"/api/clinical/patients/{patientId}/seances/{seance.Id}", seance);
        });

        // ----- Prescriptions (F-A4) -----

        group.MapGet("/patients/{patientId:guid}/prescriptions", (Guid patientId, ClinicalService service, HttpContext context) =>
        {
            return Results.Ok(service.ListPrescriptions(Tenant(context), patientId));
        });

        group.MapPost("/patients/{patientId:guid}/prescriptions", (
            Guid patientId,
            CreatePrescriptionRequest request,
            ClinicalService service,
            AuditTrailService audit,
            HttpContext context) =>
        {
            var prescription = service.CreatePrescription(
                Tenant(context),
                patientId,
                request.PrescriberName,
                request.PrescriberInami,
                request.PrescribedAtUtc,
                request.Diagnosis,
                request.SessionsPrescribed,
                Actor(context));
            audit.Record(Tenant(context), Actor(context), "prescription_created", "Prescription", prescription.Id.ToString(),
                $"patient={patientId}; sessions={request.SessionsPrescribed}");
            return Results.Created($"/api/clinical/patients/{patientId}/prescriptions/{prescription.Id}", prescription);
        });
    }

    private static string Tenant(HttpContext context) => (string)context.Items[TenantItemKey]!;

    private static string Actor(HttpContext context) =>
        context.Request.Headers.TryGetValue(ActorHeaderName, out var actor) && !string.IsNullOrWhiteSpace(actor)
            ? actor.ToString()
            : DefaultActor;
}
