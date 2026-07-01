using System;
using System.Collections.Generic;
using Kine.Modules.Patients.Domain;

namespace Kine.Modules.Patients.Application;

/// <summary>
/// Tenant-scoped persistence contract for patients, contacts and consents.
/// </summary>
public interface IPatientStore
{
    void Add(Patient patient);
    Patient? Get(string tenantId, Guid patientId);
    IReadOnlyList<Patient> GetAll(string tenantId);
    void Update(Patient patient);

    void AddContact(PatientContact contact);
    PatientContact? GetContact(string tenantId, Guid contactId);
    IReadOnlyList<PatientContact> GetContactsForPatient(string tenantId, Guid patientId);
    void UpdateContact(PatientContact contact);
    void RemoveContact(string tenantId, Guid contactId);

    void AddConsent(PatientConsent consent);
    PatientConsent? GetConsent(string tenantId, Guid consentId);
    IReadOnlyList<PatientConsent> GetConsentsForPatient(string tenantId, Guid patientId);
    void UpdateConsent(PatientConsent consent);
}
