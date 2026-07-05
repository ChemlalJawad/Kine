using System;
using System.Collections.Generic;
using System.Linq;
using Kine.Modules.Clinical.Application;
using Kine.Modules.Clinical.Infrastructure;
using Xunit;

namespace Kine.UnitTests;

public class ClinicalServiceTests
{
    private const string TenantId = "tenant-001";
    private const string Actor = "staff-1";

    private static ClinicalService CreateService() => new(new InMemorySeanceStore(), new InMemoryPrescriptionStore());

    [Fact]
    public void Create_seance_persists_and_counts()
    {
        var service = CreateService();
        var patientId = Guid.NewGuid();

        var seance = service.CreateSeance(TenantId, patientId, DateTime.UtcNow, "Suivi lombalgie", Actor);

        Assert.Equal(patientId, seance.PatientId);
        Assert.Equal("Suivi lombalgie", seance.Note);
        Assert.Equal(1, service.CountSeances(TenantId, patientId));
    }

    [Fact]
    public void Blank_note_is_stored_as_null()
    {
        var service = CreateService();

        var seance = service.CreateSeance(TenantId, Guid.NewGuid(), DateTime.UtcNow, "   ", Actor);

        Assert.Null(seance.Note);
    }

    [Fact]
    public void List_seances_returns_most_recent_first()
    {
        var service = CreateService();
        var patientId = Guid.NewGuid();
        var now = DateTime.UtcNow;

        service.CreateSeance(TenantId, patientId, now.AddDays(-14), "ancienne", Actor);
        service.CreateSeance(TenantId, patientId, now.AddDays(-1), "recente", Actor);

        var seances = service.ListSeances(TenantId, patientId);

        Assert.Equal(2, seances.Count);
        Assert.Equal("recente", seances[0].Note);
    }

    [Fact]
    public void Create_seance_rejects_empty_patient()
    {
        var service = CreateService();

        Assert.Throws<ArgumentException>(() =>
            service.CreateSeance(TenantId, Guid.Empty, DateTime.UtcNow, null, Actor));
    }

    [Fact]
    public void Create_seance_rejects_default_date()
    {
        var service = CreateService();

        Assert.Throws<ArgumentException>(() =>
            service.CreateSeance(TenantId, Guid.NewGuid(), default, null, Actor));
    }

    [Fact]
    public void Create_seance_rejects_missing_tenant()
    {
        var service = CreateService();

        Assert.Throws<ArgumentException>(() =>
            service.CreateSeance(" ", Guid.NewGuid(), DateTime.UtcNow, null, Actor));
    }

    [Fact]
    public void Seances_are_isolated_between_tenants()
    {
        var service = CreateService();
        var patientId = Guid.NewGuid();

        service.CreateSeance(TenantId, patientId, DateTime.UtcNow, null, Actor);

        Assert.Empty(service.ListSeances("tenant-other", patientId));
        Assert.Equal(0, service.CountSeances("tenant-other", patientId));
    }

    [Fact]
    public void Optional_appointment_link_is_preserved()
    {
        var service = CreateService();
        var appointmentId = Guid.NewGuid();

        var seance = service.CreateSeance(TenantId, Guid.NewGuid(), DateTime.UtcNow, null, Actor, appointmentId);

        Assert.Equal(appointmentId, seance.AppointmentId);
    }

    // ----- Prescriptions (F-A4) -----

    [Fact]
    public void CreatePrescription_computes_two_month_validity()
    {
        var service = CreateService();
        var patientId = Guid.NewGuid();
        var prescribedAt = new DateTime(2026, 7, 1, 0, 0, 0, DateTimeKind.Utc);

        var prescription = service.CreatePrescription(TenantId, patientId, "Dr Anne Willems", null, prescribedAt, "Lombalgie", 9, Actor);

        Assert.Equal(prescribedAt.AddMonths(2), prescription.ValidUntilUtc);
        Assert.Equal(9, prescription.SessionsPrescribed);
    }

    [Fact]
    public void CreatePrescription_rejects_non_positive_sessions()
    {
        var service = CreateService();

        Assert.Throws<ArgumentException>(() =>
            service.CreatePrescription(TenantId, Guid.NewGuid(), "Dr Anne Willems", null, DateTime.UtcNow, null, 0, Actor));
    }

    [Fact]
    public void ListPrescriptions_reports_usage_and_remaining()
    {
        var service = CreateService();
        var patientId = Guid.NewGuid();
        var prescription = service.CreatePrescription(TenantId, patientId, "Dr Anne Willems", null, DateTime.UtcNow.AddDays(-10), null, 3, Actor);

        service.CreateSeance(TenantId, patientId, DateTime.UtcNow, null, Actor, prescriptionId: prescription.Id);

        var usage = Assert.Single(service.ListPrescriptions(TenantId, patientId));
        Assert.Equal(1, usage.SeancesUsed);
        Assert.Equal(2, usage.SeancesRemaining);
        Assert.False(usage.IsExpired);
    }

    [Fact]
    public void CreateSeance_rejects_seance_beyond_prescription_quota()
    {
        var service = CreateService();
        var patientId = Guid.NewGuid();
        var prescription = service.CreatePrescription(TenantId, patientId, "Dr Anne Willems", null, DateTime.UtcNow.AddDays(-5), null, 1, Actor);

        service.CreateSeance(TenantId, patientId, DateTime.UtcNow, null, Actor, prescriptionId: prescription.Id);

        Assert.Throws<InvalidOperationException>(() =>
            service.CreateSeance(TenantId, patientId, DateTime.UtcNow, null, Actor, prescriptionId: prescription.Id));
    }

    [Fact]
    public void CreateSeance_rejects_seance_after_prescription_expiry()
    {
        var service = CreateService();
        var patientId = Guid.NewGuid();
        var prescription = service.CreatePrescription(TenantId, patientId, "Dr Anne Willems", null, DateTime.UtcNow.AddMonths(-3), null, 9, Actor);

        Assert.Throws<InvalidOperationException>(() =>
            service.CreateSeance(TenantId, patientId, DateTime.UtcNow, null, Actor, prescriptionId: prescription.Id));
    }

    [Fact]
    public void CreateSeance_rejects_prescription_of_another_patient()
    {
        var service = CreateService();
        var prescription = service.CreatePrescription(TenantId, Guid.NewGuid(), "Dr Anne Willems", null, DateTime.UtcNow, null, 9, Actor);

        Assert.Throws<KeyNotFoundException>(() =>
            service.CreateSeance(TenantId, Guid.NewGuid(), DateTime.UtcNow, null, Actor, prescriptionId: prescription.Id));
    }
}
