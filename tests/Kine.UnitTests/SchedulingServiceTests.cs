using System;
using System.Collections.Generic;
using Kine.Modules.Scheduling.Application;
using Kine.Modules.Scheduling.Domain;
using Kine.Modules.Scheduling.Infrastructure;
using Xunit;

namespace Kine.UnitTests;

public class SchedulingServiceTests
{
    private static SchedulingService CreateService() => new(new InMemorySchedulingStore());

    private static readonly DateTime Start = new(2026, 8, 1, 9, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime End = new(2026, 8, 1, 9, 30, 0, DateTimeKind.Utc);

    [Fact]
    public void CreateSlot_persists_free_slot_scoped_to_tenant()
    {
        var service = CreateService();

        var slot = service.CreateSlot("tenant-a", "prat-1", Start, End, "staff-1");

        Assert.False(slot.IsBooked);
        Assert.Equal("tenant-a", slot.TenantId);
        Assert.Contains(slot, service.ListSlots("tenant-a"));
    }

    [Fact]
    public void CreateSlot_throws_when_end_before_start()
    {
        var service = CreateService();

        Assert.Throws<ArgumentException>(() =>
            service.CreateSlot("tenant-a", "prat-1", End, Start, "staff-1"));
    }

    [Fact]
    public void ListSlots_isolates_by_tenant()
    {
        var service = CreateService();
        service.CreateSlot("tenant-a", "prat-1", Start, End, "staff-1");
        service.CreateSlot("tenant-b", "prat-2", Start, End, "staff-2");

        Assert.Single(service.ListSlots("tenant-a"));
        Assert.Single(service.ListSlots("tenant-b"));
    }

    [Fact]
    public void BookAppointment_creates_scheduled_appointment_and_marks_slot_booked()
    {
        var service = CreateService();
        var slot = service.CreateSlot("tenant-a", "prat-1", Start, End, "staff-1");
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
        var slot = service.CreateSlot("tenant-a", "prat-1", Start, End, "staff-1");
        service.BookAppointment("tenant-a", slot.Id, Guid.NewGuid(), "staff-1");

        Assert.Throws<InvalidOperationException>(() =>
            service.BookAppointment("tenant-a", slot.Id, Guid.NewGuid(), "staff-1"));
    }

    [Fact]
    public void GetAppointment_returns_null_for_other_tenant()
    {
        var service = CreateService();
        var slot = service.CreateSlot("tenant-a", "prat-1", Start, End, "staff-1");
        var appointment = service.BookAppointment("tenant-a", slot.Id, Guid.NewGuid(), "staff-1");

        Assert.Null(service.GetAppointment("tenant-b", appointment.Id));
    }

    [Fact]
    public void ListAppointments_isolates_by_tenant()
    {
        var service = CreateService();
        var slotA = service.CreateSlot("tenant-a", "prat-1", Start, End, "staff-1");
        var slotB = service.CreateSlot("tenant-b", "prat-2", Start, End, "staff-2");
        service.BookAppointment("tenant-a", slotA.Id, Guid.NewGuid(), "staff-1");
        service.BookAppointment("tenant-b", slotB.Id, Guid.NewGuid(), "staff-2");

        Assert.Single(service.ListAppointments("tenant-a"));
        Assert.Single(service.ListAppointments("tenant-b"));
    }

    [Fact]
    public void CancelAppointment_sets_status_cancelled_and_frees_slot()
    {
        var service = CreateService();
        var slot = service.CreateSlot("tenant-a", "prat-1", Start, End, "staff-1");
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
        var slot = service.CreateSlot("tenant-a", "prat-1", Start, End, "staff-1");
        var appointment = service.BookAppointment("tenant-a", slot.Id, Guid.NewGuid(), "staff-1");
        service.CancelAppointment("tenant-a", appointment.Id);

        Assert.Throws<InvalidOperationException>(() =>
            service.CancelAppointment("tenant-a", appointment.Id));
    }

    [Fact]
    public void MarkNoShow_sets_status_and_keeps_slot_booked()
    {
        var service = CreateService();
        var slot = service.CreateSlot("tenant-a", "prat-1", Start, End, "staff-1");
        var appointment = service.BookAppointment("tenant-a", slot.Id, Guid.NewGuid(), "staff-1");

        var noShow = service.MarkNoShow("tenant-a", appointment.Id);

        Assert.Equal(AppointmentStatus.NoShow, noShow.Status);
        Assert.True(service.ListSlots("tenant-a")[0].IsBooked);
    }

    [Fact]
    public void MarkNoShow_throws_when_not_found()
    {
        var service = CreateService();

        Assert.Throws<KeyNotFoundException>(() =>
            service.MarkNoShow("tenant-a", Guid.NewGuid()));
    }
}
