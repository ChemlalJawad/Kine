using System;
using System.Collections.Generic;
using System.Linq;
using Kine.Modules.Patients.Application;
using Kine.Modules.Patients.Domain;

namespace Kine.Modules.Patients.Infrastructure;

/// <summary>
/// In-memory tenant-scoped store for patients, contacts and consents.
/// MVP persistence stand-in pending the PostgreSQL/RLS-backed implementation.
/// </summary>
public sealed class InMemoryPatientStore : IPatientStore
{
    private readonly object _lock = new();
    private readonly Dictionary<string, Dictionary<Guid, Patient>> _patientsByTenant = new();
    private readonly Dictionary<string, Dictionary<Guid, PatientContact>> _contactsByTenant = new();
    private readonly Dictionary<string, Dictionary<Guid, PatientConsent>> _consentsByTenant = new();

    public void Add(Patient patient)
    {
        lock (_lock)
        {
            var patients = GetOrCreate(_patientsByTenant, patient.TenantId);
            patients[patient.Id] = patient;
        }
    }

    public Patient? Get(string tenantId, Guid patientId)
    {
        lock (_lock)
        {
            return _patientsByTenant.TryGetValue(tenantId, out var patients) && patients.TryGetValue(patientId, out var patient)
                ? patient
                : null;
        }
    }

    public IReadOnlyList<Patient> GetAll(string tenantId)
    {
        lock (_lock)
        {
            return _patientsByTenant.TryGetValue(tenantId, out var patients)
                ? patients.Values.ToList()
                : Array.Empty<Patient>();
        }
    }

    public void Update(Patient patient)
    {
        lock (_lock)
        {
            var patients = GetOrCreate(_patientsByTenant, patient.TenantId);
            patients[patient.Id] = patient;
        }
    }

    public void AddContact(PatientContact contact)
    {
        lock (_lock)
        {
            var contacts = GetOrCreate(_contactsByTenant, contact.TenantId);
            contacts[contact.Id] = contact;
        }
    }

    public PatientContact? GetContact(string tenantId, Guid contactId)
    {
        lock (_lock)
        {
            return _contactsByTenant.TryGetValue(tenantId, out var contacts) && contacts.TryGetValue(contactId, out var contact)
                ? contact
                : null;
        }
    }

    public IReadOnlyList<PatientContact> GetContactsForPatient(string tenantId, Guid patientId)
    {
        lock (_lock)
        {
            if (!_contactsByTenant.TryGetValue(tenantId, out var contacts))
            {
                return Array.Empty<PatientContact>();
            }

            return contacts.Values.Where(c => c.PatientId == patientId).ToList();
        }
    }

    public void UpdateContact(PatientContact contact)
    {
        lock (_lock)
        {
            var contacts = GetOrCreate(_contactsByTenant, contact.TenantId);
            contacts[contact.Id] = contact;
        }
    }

    public void RemoveContact(string tenantId, Guid contactId)
    {
        lock (_lock)
        {
            if (_contactsByTenant.TryGetValue(tenantId, out var contacts))
            {
                contacts.Remove(contactId);
            }
        }
    }

    public void AddConsent(PatientConsent consent)
    {
        lock (_lock)
        {
            var consents = GetOrCreate(_consentsByTenant, consent.TenantId);
            consents[consent.Id] = consent;
        }
    }

    public PatientConsent? GetConsent(string tenantId, Guid consentId)
    {
        lock (_lock)
        {
            return _consentsByTenant.TryGetValue(tenantId, out var consents) && consents.TryGetValue(consentId, out var consent)
                ? consent
                : null;
        }
    }

    public IReadOnlyList<PatientConsent> GetConsentsForPatient(string tenantId, Guid patientId)
    {
        lock (_lock)
        {
            if (!_consentsByTenant.TryGetValue(tenantId, out var consents))
            {
                return Array.Empty<PatientConsent>();
            }

            return consents.Values.Where(c => c.PatientId == patientId).ToList();
        }
    }

    public void UpdateConsent(PatientConsent consent)
    {
        lock (_lock)
        {
            var consents = GetOrCreate(_consentsByTenant, consent.TenantId);
            consents[consent.Id] = consent;
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
