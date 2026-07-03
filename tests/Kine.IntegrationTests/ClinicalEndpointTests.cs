using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Kine.Api.Modules;
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
}
