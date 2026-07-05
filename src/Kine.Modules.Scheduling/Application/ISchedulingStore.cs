using System;
using System.Collections.Generic;
using Kine.Modules.Scheduling.Domain;

namespace Kine.Modules.Scheduling.Application;

/// <summary>
/// Tenant-scoped persistence contract for practitioner slots and appointments.
/// </summary>
public interface ISchedulingStore
{
    void AddPractitioner(Practitioner practitioner);
    Practitioner? GetPractitioner(string tenantId, Guid practitionerId);
    IReadOnlyList<Practitioner> GetAllPractitioners(string tenantId);

    void AddSlot(PractitionerSlot slot);
    PractitionerSlot? GetSlot(string tenantId, Guid slotId);
    IReadOnlyList<PractitionerSlot> GetAllSlots(string tenantId);
    void UpdateSlot(PractitionerSlot slot);

    /// <summary>
    /// Atomically checks and reserves a free slot (IsBooked = true) in a single
    /// synchronized step, so two concurrent bookings of the same slot can never
    /// both succeed. On success, <paramref name="reservedSlot"/> carries the
    /// updated slot snapshot.
    /// </summary>
    SlotReservationResult TryReserveSlot(string tenantId, Guid slotId, DateTime nowUtc, out PractitionerSlot? reservedSlot);

    void AddAppointment(Appointment appointment);
    Appointment? GetAppointment(string tenantId, Guid appointmentId);
    IReadOnlyList<Appointment> GetAllAppointments(string tenantId);
    void UpdateAppointment(Appointment appointment);
}

/// <summary>
/// Outcome of an atomic slot reservation attempt.
/// </summary>
public enum SlotReservationResult
{
    NotFound,
    AlreadyBooked,
    Reserved
}
