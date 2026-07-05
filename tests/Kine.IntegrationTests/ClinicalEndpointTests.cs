using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Kine.Api.Modules;
using Kine.Modules.Clinical.Application;
using Kine.Modules.Clinical.Domain;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Kine.IntegrationTests;

public class ClinicalEndpointTests
{
    private static WebApplicationFactory<Program> CreateFactory() => new();

    private static HttpClient CreateTenantClient(WebApplicationFactory<Program> factory, string tenantId = "tenant-001")
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId);
        return client;
    }

    [Fact]
    public async Task Clinical_endpoints_reject_requests_without_tenant()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync($"/api/clinical/patients/{Guid.NewGuid()}/seances");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_seance_then_list_roundtrips()
    {
        await using var factory = CreateFactory();
        using var client = CreateTenantClient(factory);
        var patientId = Guid.NewGuid();

        var createResponse = await client.PostAsJsonAsync(
            $"/api/clinical/patients/{patientId}/seances",
            new CreateSeanceRequest(DateTime.UtcNow, "Seance test"));
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var seances = await client.GetFromJsonAsync<SeanceClinique[]>($"/api/clinical/patients/{patientId}/seances");
        Assert.NotNull(seances);
        Assert.Single(seances!);
        Assert.Equal("Seance test", seances![0].Note);
    }

    [Fact]
    public async Task Seances_are_isolated_between_tenants()
    {
        await using var factory = CreateFactory();
        using var clientA = CreateTenantClient(factory, "tenant-a");
        using var clientB = CreateTenantClient(factory, "tenant-b");
        var patientId = Guid.NewGuid();

        await clientA.PostAsJsonAsync(
            $"/api/clinical/patients/{patientId}/seances",
            new CreateSeanceRequest(DateTime.UtcNow, null));

        var seancesB = await clientB.GetFromJsonAsync<SeanceClinique[]>($"/api/clinical/patients/{patientId}/seances");

        Assert.NotNull(seancesB);
        Assert.Empty(seancesB!);
    }

    // ----- Prescriptions (F-A4) -----

    [Fact]
    public async Task Prescription_roundtrip_and_usage()
    {
        await using var factory = CreateFactory();
        using var client = CreateTenantClient(factory);
        var patientId = Guid.NewGuid();

        var createResponse = await client.PostAsJsonAsync($"/api/clinical/patients/{patientId}/prescriptions",
            new CreatePrescriptionRequest("Dr Anne Willems", DateTime.UtcNow.AddDays(-3), 2));
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);
        var prescription = await createResponse.Content.ReadFromJsonAsync<Prescription>();

        var seanceResponse = await client.PostAsJsonAsync($"/api/clinical/patients/{patientId}/seances",
            new CreateSeanceRequest(DateTime.UtcNow, "Seance 1", null, prescription!.Id));
        Assert.Equal(HttpStatusCode.Created, seanceResponse.StatusCode);

        var listResponse = await client.GetAsync($"/api/clinical/patients/{patientId}/prescriptions");
        var usages = await listResponse.Content.ReadFromJsonAsync<PrescriptionUsage[]>();
        Assert.Single(usages!);
        Assert.Equal(1, usages![0].SeancesUsed);
        Assert.Equal(1, usages[0].SeancesRemaining);
    }

    [Fact]
    public async Task Seance_beyond_prescription_quota_returns_409()
    {
        await using var factory = CreateFactory();
        using var client = CreateTenantClient(factory);
        var patientId = Guid.NewGuid();

        var createResponse = await client.PostAsJsonAsync($"/api/clinical/patients/{patientId}/prescriptions",
            new CreatePrescriptionRequest("Dr Anne Willems", DateTime.UtcNow.AddDays(-3), 1));
        var prescription = await createResponse.Content.ReadFromJsonAsync<Prescription>();

        await client.PostAsJsonAsync($"/api/clinical/patients/{patientId}/seances",
            new CreateSeanceRequest(DateTime.UtcNow, null, null, prescription!.Id));
        var overQuota = await client.PostAsJsonAsync($"/api/clinical/patients/{patientId}/seances",
            new CreateSeanceRequest(DateTime.UtcNow, null, null, prescription.Id));

        Assert.Equal(HttpStatusCode.Conflict, overQuota.StatusCode);
    }

    [Fact]
    public async Task Prescriptions_are_isolated_between_tenants()
    {
        await using var factory = CreateFactory();
        using var clientA = CreateTenantClient(factory, "tenant-a");
        using var clientB = CreateTenantClient(factory, "tenant-b");
        var patientId = Guid.NewGuid();

        await clientA.PostAsJsonAsync($"/api/clinical/patients/{patientId}/prescriptions",
            new CreatePrescriptionRequest("Dr Anne Willems", DateTime.UtcNow, 9));

        var crossTenantList = await clientB.GetAsync($"/api/clinical/patients/{patientId}/prescriptions");
        var usages = await crossTenantList.Content.ReadFromJsonAsync<PrescriptionUsage[]>();
        Assert.Empty(usages!);
    }
}
