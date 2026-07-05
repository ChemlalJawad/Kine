using System;
using System.Collections.Generic;
using System.Linq;
using Kine.Modules.Scheduling.Application;
using Kine.Modules.Scheduling.Domain;

namespace Kine.Modules.Scheduling.Infrastructure;

/// <summary>
/// In-memory tenant-scoped store for practitioner slots and appointments.
/// MVP persistence stand-in pending the PostgreSQL/RLS-backed implementation.
/// </summary>
public sealed class InMemorySchedulingStore : ISchedulingStore
{
    private readonly object _lock = new();
    private readonly Dictionary<string, Dictionary<Guid, Practitioner>> _practitionersByTenant = new();
    private readonly Dictionary<string, Dictionary<Guid, PractitionerSlot>> _slotsByTenant = new();
    private readonly Dictionary<string, Dictionary<Guid, Appointment>> _appointmentsByTenant = new();

    public void AddPractitioner(Practitioner practitioner)
    {
        lock (_lock)
        {
            var practitioners = GetOrCreate(_practitionersByTenant, practitioner.TenantId);
            practitioners[practitioner.Id] = practitioner;
        }
    }

    public Practitioner? GetPractitioner(string tenantId, Guid practitionerId)
    {
        lock (_lock)
        {
            return _practitionersByTenant.TryGetValue(tenantId, out var practitioners) && practitioners.TryGetValue(practitionerId, out var practitioner)
                ? practitioner
                : null;
        }
    }

    public IReadOnlyList<Practitioner> GetAllPractitioners(string tenantId)
    {
        lock (_lock)
        {
            return _practitionersByTenant.TryGetValue(tenantId, out var practitioners)
                ? practitioners.Values.ToList()
                : Array.Empty<Practitioner>();
        }
    }

    public void AddSlot(PractitionerSlot slot)
    {
        lock (_lock)
        {
            var slots = GetOrCreate(_slotsByTenant, slot.TenantId);
            slots[slot.Id] = slot;
        }
    }

    public PractitionerSlot? GetSlot(string tenantId, Guid slotId)
    {
        lock (_lock)
        {
            return _slotsByTenant.TryGetValue(tenantId, out var slots) && slots.TryGetValue(slotId, out var slot)
                ? slot
                : null;
        }
    }

    public IReadOnlyList<PractitionerSlot> GetAllSlots(string tenantId)
    {
        lock (_lock)
        {
            return _slotsByTenant.TryGetValue(tenantId, out var slots)
                ? slots.Values.ToList()
                : Array.Empty<PractitionerSlot>();
        }
    }

    public void UpdateSlot(PractitionerSlot slot)
    {
        lock (_lock)
        {
            var slots = GetOrCreate(_slotsByTenant, slot.TenantId);
            slots[slot.Id] = slot;
        }
    }

    public SlotReservationResult TryReserveSlot(string tenantId, Guid slotId, DateTime nowUtc, out PractitionerSlot? reservedSlot)
    {
        // Check-and-set under the store lock: the read of IsBooked and the write
        // of the reserved slot are one atomic step, closing the double-booking
        // window that existed when callers did GetSlot / check / UpdateSlot.
        lock (_lock)
        {
            reservedSlot = null;

            if (!_slotsByTenant.TryGetValue(tenantId, out var slots) || !slots.TryGetValue(slotId, out var slot))
            {
                return SlotReservationResult.NotFound;
            }

            if (slot.IsBooked)
            {
                return SlotReservationResult.AlreadyBooked;
            }

            var updated = slot with { IsBooked = true, UpdatedAtUtc = nowUtc };
            slots[slotId] = updated;
            reservedSlot = updated;
            return SlotReservationResult.Reserved;
        }
    }

    public void AddAppointment(Appointment appointment)
    {
        lock (_lock)
        {
            var appointments = GetOrCreate(_appointmentsByTenant, appointment.TenantId);
            appointments[appointment.Id] = appointment;
        }
    }

    public Appointment? GetAppointment(string tenantId, Guid appointmentId)
    {
        lock (_lock)
        {
            return _appointmentsByTenant.TryGetValue(tenantId, out var appointments) && appointments.TryGetValue(appointmentId, out var appointment)
                ? appointment
                : null;
        }
    }

    public IReadOnlyList<Appointment> GetAllAppointments(string tenantId)
    {
        lock (_lock)
        {
            return _appointmentsByTenant.TryGetValue(tenantId, out var appointments)
                ? appointments.Values.ToList()
                : Array.Empty<Appointment>();
        }
    }

    public void UpdateAppointment(Appointment appointment)
    {
        lock (_lock)
        {
            var appointments = GetOrCreate(_appointmentsByTenant, appointment.TenantId);
            appointments[appointment.Id] = appointment;
        }
    }

    private static Dictionary<Guid, TEntity> GetOrCreate<TEntity>(Dictionary<string, Dictionary<Guid, TEntity>> byTenant, string tenantId)
    {
        if (!byTenant.TryGetValue(tenantId, out var entities))
        {
            entities = new Dictionary<Guid, TEntity>();
            byTenant[tenantId] = entities;
        }

        return entities;
    }
}
