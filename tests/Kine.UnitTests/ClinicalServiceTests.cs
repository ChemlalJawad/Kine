using System;
using System.Linq;
using Kine.Modules.Clinical.Application;
using Kine.Modules.Clinical.Infrastructure;
using Xunit;

namespace Kine.UnitTests;

public class ClinicalServiceTests
{
    private const string TenantId = "tenant-001";
    private const string Actor = "staff-1";

    private static ClinicalService CreateService() => new(new InMemorySeanceStore());

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
}
