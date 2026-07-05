using System;
using System.Collections.Generic;
using System.Linq;
using Kine.Modules.Patients.Application;
using Kine.Modules.Patients.Domain;
using Kine.Modules.Patients.Infrastructure;
using Xunit;

namespace Kine.UnitTests;

public class PatientServiceTests
{
    private static PatientService CreateService() => new(new InMemoryPatientStore());

    [Fact]
    public void CreatePatient_persists_active_patient_scoped_to_tenant()
    {
        var service = CreateService();

        var patient = service.CreatePatient("tenant-a", "Jean", "Dupont", new DateOnly(1990, 1, 1), "staff-1");

        Assert.Equal(PatientStatus.Active, patient.Status);
        Assert.Equal("tenant-a", patient.TenantId);
        Assert.Same(patient, service.GetPatient("tenant-a", patient.Id));
    }

    [Fact]
    public void GetPatient_returns_null_for_other_tenant()
    {
        var service = CreateService();
        var patient = service.CreatePatient("tenant-a", "Jean", "Dupont", null, "staff-1");

        Assert.Null(service.GetPatient("tenant-b", patient.Id));
    }

    [Fact]
    public void ListPatients_isolates_by_tenant()
    {
        var service = CreateService();
        service.CreatePatient("tenant-a", "Jean", "Dupont", null, "staff-1");
        service.CreatePatient("tenant-b", "Marie", "Curie", null, "staff-2");

        Assert.Single(service.ListPatients("tenant-a"));
        Assert.Single(service.ListPatients("tenant-b"));
    }

    [Fact]
    public void UpdatePatient_changes_fields_and_bumps_updated_at()
    {
        var service = CreateService();
        var patient = service.CreatePatient("tenant-a", "Jean", "Dupont", null, "staff-1");

        var updated = service.UpdatePatient("tenant-a", patient.Id, "Jean-Paul", "Dupont", new DateOnly(1985, 5, 5));

        Assert.Equal("Jean-Paul", updated.FirstName);
        Assert.Equal(new DateOnly(1985, 5, 5), updated.DateOfBirth);
        Assert.True(updated.UpdatedAtUtc >= patient.UpdatedAtUtc);
    }

    [Fact]
    public void UpdatePatient_throws_when_not_found()
    {
        var service = CreateService();

        Assert.Throws<KeyNotFoundException>(() =>
            service.UpdatePatient("tenant-a", Guid.NewGuid(), "Jean", "Dupont", null));
    }

    [Fact]
    public void ArchivePatient_sets_status_archived_instead_of_deleting()
    {
        var service = CreateService();
        var patient = service.CreatePatient("tenant-a", "Jean", "Dupont", null, "staff-1");

        var archived = service.ArchivePatient("tenant-a", patient.Id);

        Assert.Equal(PatientStatus.Archived, archived.Status);
        Assert.NotNull(service.GetPatient("tenant-a", patient.Id));
    }

    [Fact]
    public void AddContact_requires_existing_patient_in_same_tenant()
    {
        var service = CreateService();

        Assert.Throws<KeyNotFoundException>(() =>
            service.AddContact("tenant-a", Guid.NewGuid(), PatientContactType.Phone, "+32123456", true, "staff-1"));
    }

    [Fact]
    public void AddContact_then_ListContacts_returns_it_scoped_to_patient_and_tenant()
    {
        var service = CreateService();
        var patient = service.CreatePatient("tenant-a", "Jean", "Dupont", null, "staff-1");

        var contact = service.AddContact("tenant-a", patient.Id, PatientContactType.Email, "jean@example.com", true, "staff-1");

        var contacts = service.ListContacts("tenant-a", patient.Id);
        Assert.Single(contacts);
        Assert.Equal(contact.Id, contacts[0].Id);
    }

    [Fact]
    public void UpdateContact_changes_value()
    {
        var service = CreateService();
        var patient = service.CreatePatient("tenant-a", "Jean", "Dupont", null, "staff-1");
        var contact = service.AddContact("tenant-a", patient.Id, PatientContactType.Phone, "+32100000", false, "staff-1");

        var updated = service.UpdateContact("tenant-a", patient.Id, contact.Id, "+32199999", true);

        Assert.Equal("+32199999", updated.Value);
        Assert.True(updated.IsPrimary);
    }

    [Fact]
    public void RemoveContact_deletes_it()
    {
        var service = CreateService();
        var patient = service.CreatePatient("tenant-a", "Jean", "Dupont", null, "staff-1");
        var contact = service.AddContact("tenant-a", patient.Id, PatientContactType.Phone, "+32100000", false, "staff-1");

        service.RemoveContact("tenant-a", patient.Id, contact.Id);

        Assert.Empty(service.ListContacts("tenant-a", patient.Id));
    }

    [Fact]
    public void UpdateContact_throws_not_found_when_contact_belongs_to_another_patient()
    {
        var service = CreateService();
        var patientA = service.CreatePatient("tenant-a", "Jean", "Dupont", null, "staff-1");
        var patientB = service.CreatePatient("tenant-a", "Marie", "Curie", null, "staff-1");
        var contactOfB = service.AddContact("tenant-a", patientB.Id, PatientContactType.Phone, "+32100000", false, "staff-1");

        Assert.Throws<KeyNotFoundException>(() =>
            service.UpdateContact("tenant-a", patientA.Id, contactOfB.Id, "+32199999", true));
    }

    [Fact]
    public void RemoveContact_throws_not_found_when_contact_does_not_exist()
    {
        var service = CreateService();
        var patient = service.CreatePatient("tenant-a", "Jean", "Dupont", null, "staff-1");

        Assert.Throws<KeyNotFoundException>(() =>
            service.RemoveContact("tenant-a", patient.Id, Guid.NewGuid()));
    }

    [Fact]
    public void RevokeConsent_throws_not_found_when_consent_belongs_to_another_patient()
    {
        var service = CreateService();
        var patientA = service.CreatePatient("tenant-a", "Jean", "Dupont", null, "staff-1");
        var patientB = service.CreatePatient("tenant-a", "Marie", "Curie", null, "staff-1");
        var consentOfB = service.RecordConsent("tenant-a", patientB.Id, ConsentType.TraitementDonnees, true, "staff-1");

        Assert.Throws<KeyNotFoundException>(() =>
            service.RevokeConsent("tenant-a", patientA.Id, consentOfB.Id));
    }

    [Fact]
    public void RecordConsent_requires_existing_patient()
    {
        var service = CreateService();

        Assert.Throws<KeyNotFoundException>(() =>
            service.RecordConsent("tenant-a", Guid.NewGuid(), ConsentType.TraitementDonnees, true, "staff-1"));
    }

    [Fact]
    public void RecordConsent_then_RevokeConsent_keeps_history_instead_of_deleting()
    {
        var service = CreateService();
        var patient = service.CreatePatient("tenant-a", "Jean", "Dupont", null, "staff-1");

        var consent = service.RecordConsent("tenant-a", patient.Id, ConsentType.PartageDossier, true, "staff-1");
        Assert.Null(consent.RevokedAtUtc);

        var revoked = service.RevokeConsent("tenant-a", patient.Id, consent.Id);

        Assert.NotNull(revoked.RevokedAtUtc);
        Assert.True(revoked.Granted);

        var consents = service.ListConsents("tenant-a", patient.Id);
        Assert.Single(consents);
        Assert.Equal(consent.Id, consents[0].Id);
    }
}
