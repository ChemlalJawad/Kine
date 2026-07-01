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
    private readonly Dictionary<string, Dictionary<Guid, PractitionerSlot>> _slotsByTenant = new();
    private readonly Dictionary<string, Dictionary<Guid, Appointment>> _appointmentsByTenant = new();

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
