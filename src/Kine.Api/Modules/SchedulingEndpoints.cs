using System;
using System.Collections.Generic;
using Kine.Modules.Audit.Application;
using Kine.Modules.Scheduling.Application;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Routing;

namespace Kine.Api.Modules;

/// <summary>
/// Minimal HTTP surface for the Scheduling module: slots (disponibilites), rdv,
/// annulation, no-show. Tenant id is read from the request context (set by
/// TenantContextMiddleware); requests reaching here are already tenant-scoped.
/// Sensitive mutations are recorded in the append-only audit journal (P0-008).
/// </summary>
public static class SchedulingEndpoints
{
    private const string TenantItemKey = "TenantId";
    private const string ActorHeaderName = "X-Actor-Id";
    private const string DefaultActor = "system";

    public static void MapSchedulingEndpoints(this WebApplication app)
    {
        var group = app.MapGroup("/api/scheduling");

        // F-B2 multi-praticiens: registre des kines du cabinet. Erreurs mappees
        // par ExceptionMappingMiddleware (400/404/409).
        group.MapPost("/practitioners", (CreatePractitionerRequest request, SchedulingService service, AuditTrailService audit, HttpContext context) =>
        {
            var practitioner = service.CreatePractitioner(Tenant(context), request.FirstName, request.LastName, request.InamiNumber, Actor(context));
            audit.Record(Tenant(context), Actor(context), "practitioner_created", "Practitioner", practitioner.Id.ToString());
            return Results.Created($"/api/scheduling/practitioners/{practitioner.Id}", practitioner);
        });

        group.MapGet("/practitioners", (SchedulingService service, HttpContext context) =>
        {
            return Results.Ok(service.ListPractitioners(Tenant(context)));
        });

        group.MapPost("/slots", (CreateSlotRequest request, SchedulingService service, AuditTrailService audit, HttpContext context) =>
        {
            try
            {
                var slot = service.CreateSlot(Tenant(context), request.PractitionerId, request.StartAtUtc, request.EndAtUtc, Actor(context));
                audit.Record(Tenant(context), Actor(context), "slot_created", "PractitionerSlot", slot.Id.ToString());
                return Results.Created($"/api/scheduling/slots/{slot.Id}", slot);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
            }
        });

        group.MapGet("/slots", (SchedulingService service, HttpContext context) =>
        {
            return Results.Ok(service.ListSlots(Tenant(context)));
        });

        group.MapPost("/appointments", (BookAppointmentRequest request, SchedulingService service, AuditTrailService audit, HttpContext context) =>
        {
            try
            {
                var appointment = service.BookAppointment(Tenant(context), request.SlotId, request.PatientId, Actor(context));
                audit.Record(Tenant(context), Actor(context), "appointment_booked", "Appointment", appointment.Id.ToString(), $"patient={request.PatientId}; slot={request.SlotId}");
                return Results.Created($"/api/scheduling/appointments/{appointment.Id}", appointment);
            }
            catch (ArgumentException ex)
            {
                return Results.BadRequest(new { error = ex.Message });
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

        group.MapGet("/appointments", (SchedulingService service, HttpContext context) =>
        {
            return Results.Ok(service.ListAppointments(Tenant(context)));
        });

        group.MapGet("/appointments/{id:guid}", (Guid id, SchedulingService service, HttpContext context) =>
        {
            var appointment = service.GetAppointment(Tenant(context), id);
            return appointment is null ? Results.NotFound() : Results.Ok(appointment);
        });

        group.MapPost("/appointments/{id:guid}/cancel", (Guid id, SchedulingService service, AuditTrailService audit, HttpContext context) =>
        {
            try
            {
                var appointment = service.CancelAppointment(Tenant(context), id);
                audit.Record(Tenant(context), Actor(context), "appointment_cancelled", "Appointment", appointment.Id.ToString());
                return Results.Ok(appointment);
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

        group.MapPost("/appointments/{id:guid}/no-show", (Guid id, SchedulingService service, AuditTrailService audit, HttpContext context) =>
        {
            try
            {
                var appointment = service.MarkNoShow(Tenant(context), id);
                audit.Record(Tenant(context), Actor(context), "appointment_no_show", "Appointment", appointment.Id.ToString());
                return Results.Ok(appointment);
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
