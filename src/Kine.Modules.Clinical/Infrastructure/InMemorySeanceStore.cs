using System;
using System.Collections.Generic;
using System.Linq;
using Kine.Modules.Clinical.Application;
using Kine.Modules.Clinical.Domain;

namespace Kine.Modules.Clinical.Infrastructure;

/// <summary>
/// In-memory tenant-scoped store for seances cliniques.
/// MVP persistence stand-in pending the PostgreSQL/RLS-backed implementation.
/// </summary>
public sealed class InMemorySeanceStore : ISeanceStore
{
    private readonly object _lock = new();
    private readonly Dictionary<string, Dictionary<Guid, SeanceClinique>> _seancesByTenant = new();

    public void Add(SeanceClinique seance)
    {
        lock (_lock)
        {
            if (!_seancesByTenant.TryGetValue(seance.TenantId, out var seances))
            {
                seances = new Dictionary<Guid, SeanceClinique>();
                _seancesByTenant[seance.TenantId] = seances;
            }

            seances[seance.Id] = seance;
        }
    }

    public SeanceClinique? Get(string tenantId, Guid seanceId)
    {
        lock (_lock)
        {
            return _seancesByTenant.TryGetValue(tenantId, out var seances) && seances.TryGetValue(seanceId, out var seance)
                ? seance
                : null;
        }
    }

    public IReadOnlyList<SeanceClinique> GetByPatient(string tenantId, Guid patientId)
    {
        lock (_lock)
        {
            return _seancesByTenant.TryGetValue(tenantId, out var seances)
                ? seances.Values.Where(seance => seance.PatientId == patientId).ToList()
                : Array.Empty<SeanceClinique>();
        }
    }
}
