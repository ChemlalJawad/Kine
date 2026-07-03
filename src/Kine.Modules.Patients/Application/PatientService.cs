using System;
using System.Collections.Generic;
using Kine.Modules.Patients.Domain;

namespace Kine.Modules.Patients.Application;

/// <summary>
/// CRUD orchestration for patients, their contacts and their consents.
/// Tenant scoping is enforced on every operation (tenantId is a mandatory argument).
/// </summary>
public sealed class PatientService
{
    private readonly IPatientStore _store;

    public PatientService(IPatientStore store)
    {
        _store = store ?? throw new ArgumentNullException(nameof(store));
    }

    public Patient CreatePatient(
        string tenantId,
        string firstName,
        string lastName,
        DateOnly? dateOfBirth,
        string createdBy,
        string? mutuelle = null,
        string? diagnosis = null,
        int sessionsPrescribed = 0,
        int sessionsDone = 0)
    {
        RequireTenant(tenantId);
        RequireNonEmpty(firstName, nameof(firstName));
        RequireNonEmpty(lastName, nameof(lastName));
        RequireNonEmpty(createdBy, nameof(createdBy));

        var now = DateTime.UtcNow;
        var patient = new Patient
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            FirstName = firstName,
            LastName = lastName,
            DateOfBirth = dateOfBirth,
            Status = PatientStatus.Active,
            Mutuelle = mutuelle,
            Diagnosis = diagnosis,
            SessionsPrescribed = sessionsPrescribed,
            SessionsDone = sessionsDone,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            CreatedBy = createdBy
        };

        _store.Add(patient);
        return patient;
    }

    public Patient? GetPatient(string tenantId, Guid patientId)
    {
        RequireTenant(tenantId);
        return _store.Get(tenantId, patientId);
    }

    public IReadOnlyList<Patient> ListPatients(string tenantId)
    {
        RequireTenant(tenantId);
        return _store.GetAll(tenantId);
    }

    public Patient UpdatePatient(
        string tenantId,
        Guid patientId,
        string firstName,
        string lastName,
        DateOnly? dateOfBirth,
        string? mutuelle = null,
        string? diagnosis = null,
        int? sessionsPrescribed = null,
        int? sessionsDone = null)
    {
        RequireTenant(tenantId);
        RequireNonEmpty(firstName, nameof(firstName));
        RequireNonEmpty(lastName, nameof(lastName));

        var existing = _store.Get(tenantId, patientId)
            ?? throw new InvalidOperationException($"Patient '{patientId}' not found for tenant '{tenantId}'.");

        var updated = existing with
        {
            FirstName = firstName,
            LastName = lastName,
            DateOfBirth = dateOfBirth,
            Mutuelle = mutuelle ?? existing.Mutuelle,
            Diagnosis = diagnosis ?? existing.Diagnosis,
            SessionsPrescribed = sessionsPrescribed ?? existing.SessionsPrescribed,
            SessionsDone = sessionsDone ?? existing.SessionsDone,
            UpdatedAtUtc = DateTime.UtcNow
        };

        _store.Update(updated);
        return updated;
    }

    /// <summary>
    /// Delete is a soft-archive: sets Status = Archived instead of removing the row,
    /// to preserve historique (SPEC/02) pending RGPD erasure design (Q-B15, ouvert).
    /// </summary>
    public Patient ArchivePatient(string tenantId, Guid patientId)
    {
        RequireTenant(tenantId);

        var existing = _store.Get(tenantId, patientId)
            ?? throw new InvalidOperationException($"Patient '{patientId}' not found for tenant '{tenantId}'.");

        var archived = existing with
        {
            Status = PatientStatus.Archived,
            UpdatedAtUtc = DateTime.UtcNow
        };

        _store.Update(archived);
        return archived;
    }

    public PatientContact AddContact(string tenantId, Guid patientId, PatientContactType type, string value, bool isPrimary, string createdBy)
    {
        RequireTenant(tenantId);
        RequireNonEmpty(value, nameof(value));
        RequireNonEmpty(createdBy, nameof(createdBy));
        RequirePatientExists(tenantId, patientId);

        var now = DateTime.UtcNow;
        var contact = new PatientContact
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PatientId = patientId,
            Type = type,
            Value = value,
            IsPrimary = isPrimary,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            CreatedBy = createdBy
        };

        _store.AddContact(contact);
        return contact;
    }

    public IReadOnlyList<PatientContact> ListContacts(string tenantId, Guid patientId)
    {
        RequireTenant(tenantId);
        return _store.GetContactsForPatient(tenantId, patientId);
    }

    public PatientContact UpdateContact(string tenantId, Guid contactId, string value, bool isPrimary)
    {
        RequireTenant(tenantId);
        RequireNonEmpty(value, nameof(value));

        var existing = _store.GetContact(tenantId, contactId)
            ?? throw new InvalidOperationException($"Contact '{contactId}' not found for tenant '{tenantId}'.");

        var updated = existing with
        {
            Value = value,
            IsPrimary = isPrimary,
            UpdatedAtUtc = DateTime.UtcNow
        };

        _store.UpdateContact(updated);
        return updated;
    }

    public void RemoveContact(string tenantId, Guid contactId)
    {
        RequireTenant(tenantId);
        _store.RemoveContact(tenantId, contactId);
    }

    public PatientConsent RecordConsent(string tenantId, Guid patientId, ConsentType type, bool granted, string createdBy)
    {
        RequireTenant(tenantId);
        RequireNonEmpty(createdBy, nameof(createdBy));
        RequirePatientExists(tenantId, patientId);

        var now = DateTime.UtcNow;
        var consent = new PatientConsent
        {
            Id = Guid.NewGuid(),
            TenantId = tenantId,
            PatientId = patientId,
            Type = type,
            Granted = granted,
            GrantedAtUtc = now,
            RevokedAtUtc = null,
            CreatedAtUtc = now,
            UpdatedAtUtc = now,
            CreatedBy = createdBy
        };

        _store.AddConsent(consent);
        return consent;
    }

    /// <summary>
    /// Revokes a previously granted consent. The record is kept (RevokedAtUtc set)
    /// rather than deleted, to preserve historique.
    /// </summary>
    public PatientConsent RevokeConsent(string tenantId, Guid consentId)
    {
        RequireTenant(tenantId);

        var existing = _store.GetConsent(tenantId, consentId)
            ?? throw new InvalidOperationException($"Consent '{consentId}' not found for tenant '{tenantId}'.");

        var revoked = existing with
        {
            RevokedAtUtc = DateTime.UtcNow,
            UpdatedAtUtc = DateTime.UtcNow
        };

        _store.UpdateConsent(revoked);
        return revoked;
    }

    public IReadOnlyList<PatientConsent> ListConsents(string tenantId, Guid patientId)
    {
        RequireTenant(tenantId);
        return _store.GetConsentsForPatient(tenantId, patientId);
    }

    private void RequirePatientExists(string tenantId, Guid patientId)
    {
        if (_store.Get(tenantId, patientId) is null)
        {
            throw new InvalidOperationException($"Patient '{patientId}' not found for tenant '{tenantId}'.");
        }
    }

    private static void RequireTenant(string tenantId)
    {
        if (string.IsNullOrWhiteSpace(tenantId))
        {
            throw new ArgumentException("Tenant id is required.", nameof(tenantId));
        }
    }

    private static void RequireNonEmpty(string value, string paramName)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ArgumentException("Value is required.", paramName);
        }
    }
}
