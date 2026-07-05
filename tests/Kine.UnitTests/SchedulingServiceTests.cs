using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Kine.Modules.Scheduling.Application;
using Kine.Modules.Scheduling.Domain;
using Kine.Modules.Scheduling.Infrastructure;
using Xunit;

namespace Kine.UnitTests;

public class SchedulingServiceTests
{
    private static SchedulingService CreateService() => new(new InMemorySchedulingStore());

    /// <summary>F-B2: les slots exigent un praticien enregistre; helper de test.</summary>
    private static string NewPractitioner(SchedulingService service, string tenantId, string staff = "staff-1")
        => service.CreatePractitioner(tenantId, "Julie", "Peeters", null, staff).Id.ToString();

    private static readonly DateTime Start = new(2026, 8, 1, 9, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime End = new(2026, 8, 1, 9, 30, 0, DateTimeKind.Utc);

    [Fact]
    public void CreateSlot_persists_free_slot_scoped_to_tenant()
    {
        var service = CreateService();

        var slot = service.CreateSlot("tenant-a", NewPractitioner(service, "tenant-a"), Start, End, "staff-1");

        Assert.False(slot.IsBooked);
        Assert.Equal("tenant-a", slot.TenantId);
        Assert.Contains(slot, service.ListSlots("tenant-a"));
    }

    [Fact]
    public void CreateSlot_throws_when_practitioner_is_unknown()
    {
        var service = CreateService();

        Assert.Throws<KeyNotFoundException>(() =>
            service.CreateSlot("tenant-a", Guid.NewGuid().ToString(), Start, End, "staff-1"));
    }

    [Fact]
    public void CreatePractitioner_then_ListPractitioners_is_tenant_scoped()
    {
        var service = CreateService();
        service.CreatePractitioner("tenant-a", "Julie", "Peeters", "5-12345-67-890", "staff-1");
        service.CreatePractitioner("tenant-b", "Thomas", "Dujardin", null, "staff-2");

        Assert.Single(service.ListPractitioners("tenant-a"));
        Assert.Single(service.ListPractitioners("tenant-b"));
        Assert.Equal("Peeters", service.ListPractitioners("tenant-a")[0].LastName);
    }

    [Fact]
    public void CreateSlot_throws_when_end_before_start()
    {
        var service = CreateService();

        Assert.Throws<ArgumentException>(() =>
            service.CreateSlot("tenant-a", NewPractitioner(service, "tenant-a"), End, Start, "staff-1"));
    }

    [Fact]
    public void ListSlots_isolates_by_tenant()
    {
        var service = CreateService();
        service.CreateSlot("tenant-a", NewPractitioner(service, "tenant-a"), Start, End, "staff-1");
        service.CreateSlot("tenant-b", NewPractitioner(service, "tenant-b", "staff-2"), Start, End, "staff-2");

        Assert.Single(service.ListSlots("tenant-a"));
        Assert.Single(service.ListSlots("tenant-b"));
    }

    [Fact]
    public void BookAppointment_creates_scheduled_appointment_and_marks_slot_booked()
    {
        var service = CreateService();
        var slot = service.CreateSlot("tenant-a", NewPractitioner(service, "tenant-a"), Start, End, "staff-1");
        var patientId = Guid.NewGuid();

        var appointment = service.BookAppointment("tenant-a", slot.Id, patientId, "staff-1");

        Assert.Equal(AppointmentStatus.Scheduled, appointment.Status);
        Assert.Equal(patientId, appointment.PatientId);
        Assert.Equal(slot.Id, appointment.SlotId);

        var slots = service.ListSlots("tenant-a");
        Assert.True(slots[0].IsBooked);
    }

    [Fact]
    public void BookAppointment_throws_when_slot_not_found()
    {
        var service = CreateService();

        Assert.Throws<KeyNotFoundException>(() =>
            service.BookAppointment("tenant-a", Guid.NewGuid(), Guid.NewGuid(), "staff-1"));
    }

    [Fact]
    public void BookAppointment_throws_when_slot_already_booked()
    {
        var service = CreateService();
        var slot = service.CreateSlot("tenant-a", NewPractitioner(service, "tenant-a"), Start, End, "staff-1");
        service.BookAppointment("tenant-a", slot.Id, Guid.NewGuid(), "staff-1");

        Assert.Throws<InvalidOperationException>(() =>
            service.BookAppointment("tenant-a", slot.Id, Guid.NewGuid(), "staff-1"));
    }

    [Fact]
    public void BookAppointment_allows_exactly_one_winner_under_concurrent_booking_of_same_slot()
    {
        // Regression P0-015: GetSlot / check IsBooked / UpdateSlot were three
        // separate store calls, so two concurrent bookings could both succeed.
        var service = CreateService();
        var slot = service.CreateSlot("tenant-a", NewPractitioner(service, "tenant-a"), Start, End, "staff-1");
        const int contenders = 20;

        var successes = 0;
        var conflicts = 0;

        Parallel.For(0, contenders, _ =>
        {
            try
            {
                service.BookAppointment("tenant-a", slot.Id, Guid.NewGuid(), "staff-1");
                Interlocked.Increment(ref successes);
            }
            catch (InvalidOperationException)
            {
                Interlocked.Increment(ref conflicts);
            }
        });

        Assert.Equal(1, successes);
        Assert.Equal(contenders - 1, conflicts);
        Assert.Single(service.ListAppointments("tenant-a"));
    }

    [Fact]
    public void GetAppointment_returns_null_for_other_tenant()
    {
        var service = CreateService();
        var slot = service.CreateSlot("tenant-a", NewPractitioner(service, "tenant-a"), Start, End, "staff-1");
        var appointment = service.BookAppointment("tenant-a", slot.Id, Guid.NewGuid(), "staff-1");

        Assert.Null(service.GetAppointment("tenant-b", appointment.Id));
    }

    [Fact]
    public void ListAppointments_isolates_by_tenant()
    {
        var service = CreateService();
        var slotA = service.CreateSlot("tenant-a", NewPractitioner(service, "tenant-a"), Start, End, "staff-1");
        var slotB = service.CreateSlot("tenant-b", NewPractitioner(service, "tenant-b", "staff-2"), Start, End, "staff-2");
        service.BookAppointment("tenant-a", slotA.Id, Guid.NewGuid(), "staff-1");
        service.BookAppointment("tenant-b", slotB.Id, Guid.NewGuid(), "staff-2");

        Assert.Single(service.ListAppointments("tenant-a"));
        Assert.Single(service.ListAppointments("tenant-b"));
    }

    [Fact]
    public void CancelAppointment_sets_status_cancelled_and_frees_slot()
    {
        var service = CreateService();
        var slot = service.CreateSlot("tenant-a", NewPractitioner(service, "tenant-a"), Start, End, "staff-1");
        var appointment = service.BookAppointment("tenant-a", slot.Id, Guid.NewGuid(), "staff-1");

        var cancelled = service.CancelAppointment("tenant-a", appointment.Id);

        Assert.Equal(AppointmentStatus.Cancelled, cancelled.Status);
        Assert.False(service.ListSlots("tenant-a")[0].IsBooked);
    }

    [Fact]
    public void CancelAppointment_throws_when_not_found()
    {
        var service = CreateService();

        Assert.Throws<KeyNotFoundException>(() =>
            service.CancelAppointment("tenant-a", Guid.NewGuid()));
    }

    [Fact]
    public void CancelAppointment_throws_when_already_cancelled()
    {
        var service = CreateService();
        var slot = service.CreateSl