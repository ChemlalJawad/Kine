using System;
using System.Collections.Generic;
using System.Linq;
using Kine.Modules.Clinical.Domain;

namespace Kine.Modules.Clinical.Application;

/// <summary>
/// Orchestration du dossier clinique v1 : enregistrement et consultation des
/// seances realisees. Tenant scoping is enforced on every operation.
/// </summary>
public sealed class ClinicalService
{
    private readonly ISeanceStore _store;

    public ClinicalService(ISeanceStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    public SeanceClinique CreateSeance(
        string tenantId,
        Guid patientId,
        DateTime dateSeanceUtc,
        string? note,
        string createdBy,
        Guid? appointmentId = null)
    {
        RequireTenant(tenantId);

        if (patientId == Guid.Empty)
        {
            throw new ArgumentException("Patient id is required.", nameof(patientId));
        }

        if (string.IsNullOrWhiteSpace(createdBy))
        {
            throw new ArgumentException("Value is required.", nameof(createdBy));
        }

        if (dateSeanceUtc == default)
        {
            throw new ArgumentException("Seance date is required.", nameof(dateSeanceUtc));
        }

        var seance = new SeanceClinique
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PatientId = patientId,
            AppointmentId = appointmentId,
            DateSeanceUtc = dateSeanceUtc,
            Note = string.IsNullOrWhiteSpace(note) ? null : note.Trim(),
            CreatedAtUtc = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        _store.Add(seance);
        return seance;
    }

    public IReadOnlyList<SeanceClinique> ListSeances(string tenantId, Guid patientId)
    {
        RequireTenant(tenantId);
        return _store.GetByPatient(tenantId, patientId)
            .OrderByDescending(seance => seance.DateSeanceUtc)
            .ToList();
    }

    public int CountSeances(string tenantId, Guid patientId)
    {
        RequireTenant(tenantId);
        return _store.GetByPatient(tenantId, patientId).Count;
    }

    private static void RequireTenant(string tenantId)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            throw new ArgumentException("Tenant id is required.", nameof(tenantId));
        }
    }
}
