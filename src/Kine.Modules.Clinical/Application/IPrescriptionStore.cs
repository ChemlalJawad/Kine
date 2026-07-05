using System;
using System.Collections.Generic;
using Kine.Modules.Clinical.Domain;

namespace Kine.Modules.Clinical.Application;

/// <summary>
/// Persistence contract for prescriptions medicales. In-memory for MVP; the
/// PostgreSQL/RLS-backed implementation will plug in behind the same interface.
/// Append-only by design at the MVP (une prescription ne se modifie pas; une
/// nouvelle prescription remplace l'ancienne cote metier).
/// </summary>
public interface IPrescriptionStore
{
    void Add(Prescription prescription);
    Prescription? Get(string tenantId, Guid prescriptionId);
    IReadOnlyList<Prescription> GetByPatient(string tenantId, Guid patientId);
}
