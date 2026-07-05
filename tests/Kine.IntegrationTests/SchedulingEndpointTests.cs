using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Kine.Api.Modules;
using Kine.Modules.Scheduling.Domain;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Kine.IntegrationTests;

public class SchedulingEndpointTests
{
    private static readonly DateTime Start = new(2026, 8, 1, 9, 0, 0, DateTimeKind.Utc);
    private static readonly DateTime End = new(2026, 8, 1, 9, 30, 0, DateTimeKind.Utc);

    private static WebApplicationFactory<Program> CreateFactory() => new();

    private static HttpClient CreateTenantClient(WebApplicationFactory<Program> factory, string tenantId = "tenant-001")
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId);
        return client;
    }

    /// <summary>F-B2: les slots exigent un praticien enregistre; cree via l'API.</summary>
    private static async Task<string> CreatePractitionerId(HttpClient client)
    {
        var response = await client.PostAsJsonAsync("/api/scheduling/practitioners",
            new CreatePractitionerRequest("Julie", "Peeters", null));
        var practitioner = await response.Content.ReadFromJsonAsync<Practitioner>();
        return practitioner!.Id.ToString();
    }

    [Fact]
    public async Task Scheduling_endpoints_reject_requests_without_tenant()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/api/scheduling/slots");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_slot_then_book_appointment_roundtrips()
    {
        await using var factory = CreateFactory();
        using var client = CreateTenantClient(factory);

        var slotResponse = await client.PostAsJsonAsync("/api/scheduling/slots", new CreateSlotRequest(await CreatePractitionerId(client), Start, End));
        Assert.Equal(HttpStatusCode.Created, slotResponse.StatusCode);
        var slot = await slotResponse.Content.ReadFromJsonAsync<PractitionerSlot>();

        var patientId = Guid.NewGuid();
        var bookResponse = await client.PostAsJsonAsync("/api/scheduling/appointments", new BookAppointmentRequest(slot!.Id, patientId));
        Assert.Equal(HttpStatusCode.Created, bookResponse.StatusCode);

        var appointment = await bookResponse.Content.ReadFromJsonAsync<Appointment>();
        Assert.Equal(AppointmentStatus.Scheduled, appointment!.Status);

        var getResponse = await client.GetAsync($"/api/scheduling/appointments/{appointment.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
    }

    [Fact]
    public async Task Booking_already_booked_slot_returns_conflict()
    {
        await using var factory = CreateFactory();
        using var client = CreateTenantClient(factory);

        var slotResponse = await client.PostAsJsonAsync("/api/scheduling/slots", new CreateSlotRequest(await CreatePractitionerId(client), Start, End));
        var slot = await slotResponse.Content.ReadFromJsonAsync<PractitionerSlot>();

        await client.PostAsJsonAsync("/api/scheduling/appointments", new BookAppointmentRequest(slot!.Id, Guid.NewGuid()));
        var secondBooking = await client.PostAsJsonAsync("/api/scheduling/appointments", new BookAppointmentRequest(slot.Id, Guid.NewGuid()));

        Assert.Equal(HttpStatusCode.Conflict, secondBooking.StatusCode);
    }

    [Fact]
    public async Task Cancel_appointment_frees_slot_for_rebooking()
    {
        await using var factory = CreateFactory();
        using var client = CreateTenantClient(factory);

        var slotResponse = await client.PostAsJsonAsync("/api/scheduling/slots", new CreateSlotRequest(await CreatePractitionerId(client), Start, End));
        var slot = await slotResponse.Content.ReadFromJsonAsync<PractitionerSlot>();

        var bookResponse = await client.PostAsJsonAsync("/api/scheduling/appointments", new BookAppointmentRequest(slot!.Id, Guid.NewGuid()));
        var appointment = await bookResponse.Content.ReadFromJsonAsync<Appointment>();

        var cancelResponse = await client.PostAsync($"/api/scheduling/appointments/{appointment!.Id}/cancel", content: null);
        Assert.Equal(HttpStatusCode.OK, cancelResponse.StatusCode);

        var rebookResponse = await client.PostAsJsonAsync("/api/scheduling/appointments", new BookAppointmentRequest(slot.Id, Guid.NewGuid()));
        Assert.Equal(HttpStatusCode.Created, rebookResponse.StatusCode);
    }

    [Fact]
    public async Task Mark_no_show_sets_status()
    {
        await using var factory = CreateFactory();
        using var client = CreateTenantClient(factory);

        var slotResponse = await client.PostAsJsonAsync("/api/scheduling/slots", new CreateSlotRequest(await CreatePractitionerId(client), Start, End));
        var slot = await slotResponse.Content.ReadFromJsonAsync<PractitionerSlot>();

        var bookResponse = await client.PostAsJsonAsync("/api/scheduling/appointments", new BookAppointmentRequest(slot!.Id, Guid.NewGuid()));
        var appointment = await bookResponse.Content.ReadFromJsonAsync<Appointment>();

        var noShowResponse = await client.PostAsync($"/api/scheduling/appointments/{appointment!.Id}/no-show", content: null);
        Assert.Equal(HttpStatusCode.OK, noShowResponse.StatusCode);

        var updated = await noShowResponse.Content.ReadFromJsonAsync<Appointment>();
        Assert.Equal(AppointmentStatus.NoShow, updated!.Status);
    }

    [Fact]
    public async Task Appointments_are_isolated_between_tenants()
    {
        await using var factory = CreateFactory();
        using var clientA = CreateTenantClient(factory, "tenant-a");
        using var clientB = CreateTenantClient(factory, "tenant-b");

        var slotResponse = await clientA.PostAsJsonAsync("/api/scheduling/slots", new CreateSlotRequest(await CreatePractitionerId(clientA), Start, End));
        var slot = await slotResponse.Content.ReadFromJsonAsync<PractitionerSlot>();

        var bookResponse = await clientA.PostAsJsonAsync("/api/scheduling/appointments", new BookAppointmentRequest(slot!.Id, Guid.NewGuid()));
        var appointment = await bookResponse.Content.ReadFromJsonAsync<Appointment>();

        var crossTenantGet = await clientB.GetAsync($"/api/scheduling/appointments/{appointment!.Id}");

        Assert.Equal(HttpStatusCode.NotFound, crossTenantGet.StatusCode);
    }
}
