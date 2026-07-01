using System;
using System.Collections.Generic;
using Kine.Modules.Scheduling.Domain;

namespace Kine.Modules.Scheduling.Application;

/// <summary>
/// Orchestration for practitioner slots (disponibilites) and appointments (rdv),
/// including cancellation and no-show transitions. Tenant scoping is enforced on
/// every operation (tenantId is a mandatory argument).
/// </summary>
public sealed class SchedulingService
{
    private readonly ISchedulingStore _store;

    public SchedulingService(ISchedulingStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    public PractitionerSlot CreateSlot(string tenantId, string practitionerId, DateTime startAtUtc, DateTime endAtUtc, string createdBy)
    {
        RequireTenant(tenantId);
        RequireNonEmpty(practitionerId, nameof(practitionerId));
        RequireNonEmpty(createdBy, nameof(createdBy));

        if (endAtUtc <= startAtUtc)
        {
            throw new ArgumentException("Slot end must be after slot start.", nameof(endAtUtc));
        }

        var now = DateTime.UtcNow;
        var slot = new PractitionerSlot
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PractitionerId = practitionerId,
            StartAtUtc = startAtUtc,
            EndAtUtc = endAtUtc,
            IsBooked = false,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            CreatedBy = createdBy
        };

        _store.AddSlot(slot);
        return slot;
    }

    public IReadOnlyList<PractitionerSlot> ListSlots(string tenantId)
    {
        RequireTenant(tenantId);
        return _store.GetAllSlots(tenantId);
    }

    public Appointment BookAppointment(string tenantId, Guid slotId, Guid patientId, string createdBy)
    {
        RequireTenant(tenantId);
        RequireNonEmpty(createdBy, nameof(createdBy));

        if (patientId == Guid.Empty)
        {
            throw new ArgumentException("Patient id is required.", nameof(patientId));
        }

        var slot = _store.GetSlot(tenantId, slotId)
            ?? throw new KeyNotFoundException($"Slot '{slotId}' not found for tenant '{tenantId}'.");

        if (slot.IsBooked)
        {
            throw new InvalidOperationException($"Slot '{slotId}' is already booked.");
        }

        var now = DateTime.UtcNow;
        var appointment = new Appointment
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PatientId = patientId,
            PractitionerId = slot.PractitionerId,
            SlotId = slot.Id,
            StartAtUtc = slot.StartAtUtc,
            EndAtUtc = slot.EndAtUtc,
            Status = AppointmentStatus.Scheduled,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            CreatedBy = createdBy
        };

        _store.AddAppointment(appointment);
        _store.UpdateSlot(slot with { IsBooked = true, UpdatedAtUtc = now });

        return appointment;
    }

    public Appointment? GetAppointment(string tenantId, Guid appointmentId)
    {
        RequireTenant(tenantId);
        return _store.GetAppointment(tenantId, appointmentId);
    }

    public IReadOnlyList<Appointment> ListAppointments(string tenantId)
    {
        RequireTenant(tenantId);
        return _store.GetAllAppointments(tenantId);
    }

    /// <summary>
    /// Cancels a scheduled appointment and frees its slot for rebooking.
    /// </summary>
    public Appointment CancelAppointment(string tenantId, Guid appointmentId)
    {
        RequireTenant(tenantId);

        var existing = _store.GetAppointment(tenantId, appointmentId)
            ?? throw new KeyNotFoundException($"Appointment '{appointmentId}' not found for tenant '{tenantId}'.");

        if (existing.Status != AppointmentStatus.Scheduled)
        {
            throw new InvalidOperationException($"Appointment '{appointmentId}' cannot be cancelled from status '{existing.Status}'.");
        }

        var now = DateTime.UtcNow;
        var cancelled = existing with { Status = AppointmentStatus.Cancelled, UpdatedAtUtc = now };
        _store.UpdateAppointment(cancelled);

        var slot = _store.GetSlot(tenantId, existing.SlotId);
        if (slot is not null)
        {
            _store.UpdateSlot(slot with { IsBooked = false, UpdatedAtUtc = now });
        }

        return cancelled;
    }

    /// <summary>
    /// Marks a scheduled appointment as a no-show. The slot stays consumed since the
    /// appointment time has already passed.
    /// </summary>
    public Appointment MarkNoShow(string tenantId, Guid appointmentId)
    {
        RequireTenant(tenantId);

        var existing = _store.GetAppointment(tenantId, appointmentId)
            ?? throw new KeyNotFoundException($"Appointment '{appointmentId}' not found for tenant '{tenantId}'.");

        if (existing.Status != AppointmentStatus.Scheduled)
        {
            throw new InvalidOperationException($"Appointment '{appointmentId}' cannot be marked no-show from status '{existing.Status}'.");
        }

        var noShow = existing with { Status = AppointmentStatus.NoShow, UpdatedAtUtc = DateTime.UtcNow };
        _store.UpdateAppointment(noShow);
        return noShow;
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
