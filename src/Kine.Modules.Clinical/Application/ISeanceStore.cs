using System;
using System.Collections.Generic;
using Kine.Modules.Clinical.Domain;

namespace Kine.Modules.Clinical.Application;

/// <summary>
/// Persistence contract for seances cliniques. In-memory for MVP; the
/// PostgreSQL/RLS-backed implementation will plug in behind the same interface.
/// Append-only by design (no update/delete): une seance realisee ne se modifie
/// pas, elle se corrige par une nouvelle entree si besoin (a cadrer post-MVP).
/// </summary>
public interface ISeanceStore
{
    void Add(SeanceClinique seance);
    SeanceClinique? Get(string tenantId, Guid seanceId);
    IReadOnlyList<SeanceClinique> GetByPatient(string tenantId, Guid patientId);
}
