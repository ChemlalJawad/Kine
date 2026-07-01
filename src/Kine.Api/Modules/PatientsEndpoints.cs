using System;
using Kine.Modules.Patients.Application;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Kine.Api.Modules;

/// <summary>
/// Minimal HTTP surface for the Patients module: CRUD patient, contacts, consents.
/// Tenant id is read from the request context (set by TenantContextMiddleware);
/// requests reaching here are already tenant-scoped.
/// </summary>
public static class PatientsEndpoints
{
    private const string TenantItemKey = "TenantId";
    private const string ActorHeaderName = "X-Actor-Id";
    private const string DefaultActor = "system";

    public static void MapPatientsEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/patients");

        group.MapPost("/", (CreatePatientRequest request, PatientService service, HttpContext context) =>
        {
            var patient = service.CreatePatient(Tenant(context), request.FirstName, request.LastName, request.DateOfBirth, Actor(context));
            return Results.Created($"/api/patients/{patient.Id}", patient);
        });

        group.MapGet("/", (PatientService service, HttpContext context) =>
        {
            return Results.Ok(service.ListPatients(Tenant(context)));
        });

        group.MapGet("/{id:guid}", (Guid id, PatientService service, HttpContext context) =>
        {
            var patient = service.GetPatient(Tenant(context), id);
            return patient is null ? Results.NotFound() : Results.Ok(patient);
        });

        group.MapPut("/{id:guid}", (Guid id, UpdatePatientRequest request, PatientService service, HttpContext context) =>
        {
            try
            {
                var patient = service.UpdatePatient(Tenant(context), id, request.FirstName, request.LastName, request.DateOfBirth);
                return Results.Ok(patient);
            }
            catch (InvalidOperationException)
            {
                return Results.NotFound();
            }
        });

        group.MapDelete("/{id:guid}", (Guid id, PatientService service, HttpContext context) =>
        {
            try
            {
                service.ArchivePatient(Tenant(context), id);
                return Results.NoContent();
            }
            catch (InvalidOperationException)
            {
                return Results.NotFound();
            }
        });

        group.MapPost("/{id:guid}/contacts", (Guid id, CreatePatientContactRequest request, PatientService service, HttpContext context) =>
        {
            try
            {
                var contact = service.AddContact(Tenant(context), id, request.Type, request.Value, request.IsPrimary, Actor(context));
                return Results.Created($"/api/patients/{id}/contacts/{contact.Id}", contact);
            }
            catch (InvalidOperationException)
            {
                return Results.NotFound();
            }
        });

        group.MapGet("/{id:guid}/contacts", (Guid id, PatientService service, HttpContext context) =>
        {
            return Results.Ok(service.ListContacts(Tenant(context), id));
        });

        group.MapPut("/{id:guid}/contacts/{contactId:guid}", (Guid id, Guid contactId, UpdatePatientContactRequest request, PatientService service, HttpContext context) =>
        {
            try
            {
                var contact = service.UpdateContact(Tenant(context), contactId, request.Value, request.IsPrimary);
                return Results.Ok(contact);
            }
            catch (InvalidOperationException)
            {
                return Results.NotFound();
            }
        });

        group.MapDelete("/{id:guid}/contacts/{contactId:guid}", (Guid id, Guid contactId, PatientService service, HttpContext context) =>
        {
            service.RemoveContact(Tenant(context), contactId);
            return Results.NoContent();
        });

        group.MapPost("/{id:guid}/consents", (Guid id, CreatePatientConsentRequest request, PatientService service, HttpContext context) =>
        {
            try
            {
                var consent = service.RecordConsent(Tenant(context), id, request.Type, request.Granted, Actor(context));
                return Results.Created($"/api/patients/{id}/consents/{consent.Id}", consent);
            }
            catch (InvalidOperationException)
            {
                return Results.NotFound();
            }
        });

        group.MapGet("/{id:guid}/consents", (Guid id, PatientService service, HttpContext context) =>
        {
            return Results.Ok(service.ListConsents(Tenant(context), id));
        });

        group.MapPost("/{id:guid}/consents/{consentId:guid}/revoke", (Guid id, Guid consentId, PatientService service, HttpContext context) =>
        {
            try
            {
                var consent = service.RevokeConsent(Tenant(context), consentId);
                return Results.Ok(consent);
            }
            catch (InvalidOperationException)
            {
                return Results.NotFound();
            }
        });
    }

    private static string Tenant(HttpContext context) => (string)context.Items[TenantItemKey]!;

    private static string Actor(HttpContext context) =>
        context.Request.Headers.TryGetValue(ActorHeaderName, out var actor) && !string.IsNullOrWhiteSpace(actor)
            ? actor.ToString()
            : DefaultActor;
}
