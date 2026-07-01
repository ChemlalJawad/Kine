using System;
using System.Collections.Generic;
using Kine.Modules.Scheduling.Domain;

namespace Kine.Modules.Scheduling.Application;

/// <summary>
/// Tenant-scoped persistence contract for practitioner slots and appointments.
/// </summary>
public interface ISchedulingStore
{
    void AddSlot(PractitionerSlot slot);
    PractitionerSlot? GetSlot(string tenantId, Guid slotId);
    IReadOnlyList<PractitionerSlot> GetAllSlots(string tenantId);
    void UpdateSlot(PractitionerSlot slot);

    void AddAppointment(Appointment appointment);
    Appointment? GetAppointment(string tenantId, Guid appointmentId);
    IReadOnlyList<Appointment> GetAllAppointments(string tenantId);
    void UpdateAppointment(Appointment appointment);
}
