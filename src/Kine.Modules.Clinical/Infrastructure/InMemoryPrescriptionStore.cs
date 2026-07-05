using System;
using System.Collections.Generic;
using System.Linq;
using Kine.Modules.Clinical.Application;
using Kine.Modules.Clinical.Domain;

namespace Kine.Modules.Clinical.Infrastructure;

/// <summary>
/// In-memory tenant-scoped store for prescriptions medicales.
/// MVP persistence stand-in pending the PostgreSQL/RLS-backed implementation.
/// </summary>
public sealed class InMemoryPrescriptionStore : IPrescriptionStore
{
    private readonly object _lock = new();
    private readonly Dictionary<string, Dictionary<Guid, Prescription>> _prescriptionsByTenant = new();

    public void Add(Prescription prescription)
    {
        lock (_lock)
        {
            if (!_prescriptionsByTenant.TryGetValue(prescription.TenantId, out var prescriptions))
            {
                prescriptions = new Dictionary<Guid, Prescription>();
                _prescriptionsByTenant[prescription.TenantId] = prescriptions;
            }

            prescriptions[prescription.Id] = prescription;
        }
    }

    public Prescription? Get(string tenantId, Guid prescriptionId)
    {
        lock (_lock)
        {
            return _prescriptionsByTenant.TryGetValue(tenantId, out var prescriptions) && prescriptions.TryGetValue(prescriptionId, out var prescription)
                ? prescription
                : null;
        }
    }

    public IReadOnlyList<Prescription> GetByPatient(string tenantId, Guid patientId)
    {
        lock (_lock)
        {
            return _prescriptionsByTenant.TryGetValue(tenantId, out var prescriptions)
                ? prescriptions.Values.Where(prescription => prescription.PatientId == patientId).ToList()
                : Array.Empty<Prescription>();
        }
    }
}
