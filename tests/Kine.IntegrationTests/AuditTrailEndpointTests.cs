using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Text.Json;
using System.Threading.Tasks;
using Kine.Api.Modules;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Kine.IntegrationTests;

/// <summary>
/// P0-008: les actions sensibles produisent des evenements dans le journal
/// append-only, et la chaine de hash reste verifiable via /api/audit/verify.
/// </summary>
public class AuditTrailEndpointTests
{
    private static WebApplicationFactory<Program> CreateFactory() => new();

    private static HttpClient CreateTenantClient(WebApplicationFactory<Program> factory, string tenantId = "tenant-001")
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId);
        client.DefaultRequestHeaders.Add("X-Actor-Id", "staff-test");
        return client;
    }

    [Fact]
    public async Task Sensitive_actions_produce_audit_events_and_chain_stays_valid()
    {
        await using var factory = CreateFactory();
        using var client = CreateTenantClient(factory);

        // patient_created
        var patientResponse = await client.PostAsJsonAsync(
            "/api/patients", new CreatePatientRequest("Jean", "Test", null));
        Assert.Equal(HttpStatusCode.Created, patientResponse.StatusCode);

        // invoice_created
        var invoiceResponse = await client.PostAsJsonAsync(
            "/api/billing/invoices", new CreateInvoiceRequest(Guid.NewGuid(), "558014", null));
        Assert.Equal(HttpStatusCode.Created, invoiceResponse.StatusCode);

        var events = await client.GetFromJsonAsync<JsonElement[]>("/api/audit/events");
        Assert.NotNull(events);
        Assert.Equal(2, events!.Length);
        Assert.Equal("patient_created", events[0].GetProperty("action").GetString());
        Assert.Equal("invoice_created", events[1].GetProperty("action").GetString());
        Assert.Equal("staff-test", events[0].GetProperty("actorId").GetString());

        var verify = await client.GetFromJsonAsync<JsonElement>("/api/audit/verify");
        Assert.True(verify.GetProperty("isValid").GetBoolean());
        Assert.Equal(2, verify.GetProperty("count").GetInt32());
    }

    [Fact]
    public async Task Audit_journal_is_isolated_between_tenants()
    {
        await using var factory = CreateFactory();
        using var clientA = CreateTenantClient(factory, "tenant-a");
        using var clientB = CreateTenantClient(factory, "tenant-b");

        await clientA.PostAsJsonAsync("/api/patients", new CreatePatientRequest("Jean", "Test", null));

        var eventsB = await clientB.GetFromJsonAsync<JsonElement[]>("/api/audit/events");
        Assert.NotNull(eventsB);
        Assert.Empty(eventsB!);
    }
}
