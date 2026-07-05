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
    private readonly IPrescriptionStore _prescriptionStore;

    public ClinicalService(ISeanceStore store, IPrescriptionStore prescriptionStore)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
        _prescriptionStore = prescriptionStore ?? throw new ArgumentNullException(nameof(prescriptionStore));
    }

    public SeanceClinique CreateSeance(
        string tenantId,
        Guid patientId,
        DateTime dateSeanceUtc,
        string? note,
        string createdBy,
        Guid? appointmentId = null,
        Guid? prescriptionId = null)
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

        if (prescriptionId is Guid prescriptionGuid)
        {
            ValidatePrescriptionForSeance(tenantId, patientId, prescriptionGuid, dateSeanceUtc);
        }

        var seance = new SeanceClinique
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PatientId = patientId,
            AppointmentId = appointmentId,
            PrescriptionId = prescriptionId,
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

    // ----- Prescriptions (F-A4) -----

    public Prescription CreatePrescription(
        string tenantId,
        Guid patientId,
        string prescriberName,
        string? prescriberInami,
        DateTime prescribedAtUtc,
        string? diagnosis,
        int sessionsPrescribed,
        string createdBy)
    {
        RequireTenant(tenantId);

        if (patientId == Guid.Empty)
        {
            throw new ArgumentException("Patient id is required.", nameof(patientId));
        }

        if (string.IsNullOrWhiteSpace(prescriberName))
        {
            throw new ArgumentException("Prescriber name is required.", nameof(prescriberName));
        }

        if (prescribedAtUtc == default)
        {
            throw new ArgumentException("Prescription date is required.", nameof(prescribedAtUtc));
        }

        if (sessionsPrescribed <= 0)
        {
            throw new ArgumentException("Sessions prescribed must be positive.", nameof(sessionsPrescribed));
        }

        if (string.IsNullOrWhiteSpace(createdBy))
        {
            throw new ArgumentException("Value is required.", nameof(createdBy));
        }

        var prescription = new Prescription
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PatientId = patientId,
            PrescriberName = prescriberName.Trim(),
            PrescriberInami = string.IsNullOrWhiteSpace(prescriberInami) ? null : prescriberInami.Trim(),
            PrescribedAtUtc = prescribedAtUtc,
            // Regle INAMI : la prescription doit etre executee dans les 2 mois.
            ValidUntilUtc = prescribedAtUtc.AddMonths(2),
            Diagnosis = string.IsNullOrWhiteSpace(diagnosis) ? null : diagnosis.Trim(),
            SessionsPrescribed = sessionsPrescribed,
            CreatedAtUtc = DateTime.UtcNow,
            CreatedBy = createdBy
        };

        _prescriptionStore.Add(prescription);
        return prescription;
    }

    public IReadOnlyList<PrescriptionUsage> ListPrescriptions(string tenantId, Guid patientId)
    {
        RequireTenant(tenantId);

        var seances = _store.GetByPatient(tenantId, patientId);
        var nowUtc = DateTime.UtcNow;

        return _prescriptionStore.GetByPatient(tenantId, patientId)
            .OrderByDescending(prescription => prescription.PrescribedAtUtc)
            .Select(prescription =>
            {
                var used = seances.Count(seance => seance.PrescriptionId == prescription.Id);
                return new PrescriptionUsage(
                    prescription,
                    used,
                    Math.Max(0, prescription.SessionsPrescribed - used),
                    nowUtc > prescription.ValidUntilUtc);
            })
            .ToList();
    }

    /// <summary>
    /// Garde-fous F-A4 avant d'imputer une seance sur une prescription :
    /// appartenance au patient (404), validite 2 mois (409), quota (409).
    /// NOTE MVP : verification puis ajout non atomiques (comme les FK inter-modules,
    /// cf. SPEC/17 T-5) -- contrainte DB a l'arrivee de PostgreSQL.
    /// </summary>
    private void ValidatePrescriptionForSeance(string tenantId, Guid patientId, Guid prescriptionId, DateTime dateSeanceUtc)
    {
        var prescription = _prescriptionStore.Get(tenantId, prescriptionId);
        if (prescription is null || prescription.PatientId != patientId)
        {
            throw new KeyNotFoundException($"Prescription '{prescriptionId}' not found for patient '{patientId}' in tenant '{tenantId}'.");
        }

        if (dateSeanceUtc > prescription.ValidUntilUtc)
        {
            throw new InvalidOperationException($"Prescription '{prescriptionId}' expiree le {prescription.ValidUntilUtc:yyyy-MM-dd} (validite 2 mois).");
        }

        var used = _store.GetByPatient(tenantId, patientId).Count(seance => seance.PrescriptionId == prescriptionId);
        if (used >= prescription.SessionsPrescribed)
        {
            throw new InvalidOperationException($"Quota de seances atteint pour la prescription '{prescriptionId}' ({prescription.SessionsPrescribed} seances prescrites).");
        }
    }

    private static void RequireTenant(string tenantId)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            throw new ArgumentException("Tenant id is required.", nameof(tenantId));
        }
    }
}
