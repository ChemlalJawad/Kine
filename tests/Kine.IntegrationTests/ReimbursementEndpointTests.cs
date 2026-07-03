using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Kine.Api.Modules;
using Kine.Modules.Reimbursement.Domain;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Kine.IntegrationTests;

public class ReimbursementEndpointTests
{
    private static WebApplicationFactory<Program> CreateFactory() => new();

    private static HttpClient CreateTenantClient(WebApplicationFactory<Program> factory, string tenantId = "tenant-001")
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId);
        return client;
    }

    [Fact]
    public async Task Reimbursement_endpoints_reject_requests_without_tenant()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();

        var response = await client.GetAsync("/api/reimbursement/cases");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_case_then_submit_roundtrips_with_mock_reference()
    {
        await using var factory = CreateFactory();
        using var client = CreateTenantClient(factory);

        var createResponse = await client.PostAsJsonAsync(
            "/api/reimbursement/cases", new CreateReimbursementCaseRequest(new[] { Guid.NewGuid() }));
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<ReimbursementCase>();
        Assert.Equal(ReimbursementCaseStatus.Draft, created!.Status);

        var submitResponse = await client.PostAsJsonAsync(
            $"/api/reimbursement/cases/{created.Id}/status",
            new TransitionReimbursementCaseRequest(ReimbursementCaseStatus.Submitted));
        Assert.Equal(HttpStatusCode.OK, submitResponse.StatusCode);

        var submitted = await submitResponse.Content.ReadFromJsonAsync<ReimbursementCase>();
        Assert.Equal(ReimbursementCaseStatus.Submitted, submitted!.Status);
        Assert.StartsWith("EFACT-", submitted.SubmissionRef);
    }

    [Fact]
    public async Task Invalid_transition_returns_conflict()
    {
        await using var factory = CreateFactory();
        using var client = CreateTenantClient(factory);

        var createResponse = await client.PostAsJsonAsync(
            "/api/reimbursement/cases", new CreateReimbursementCaseRequest(new[] { Guid.NewGuid() }));
        var created = await createResponse.Content.ReadFromJsonAsync<ReimbursementCase>();

        var response = await client.PostAsJsonAsync(
            $"/api/reimbursement/cases/{created!.Id}/status",
            new TransitionReimbursementCaseRequest(ReimbursementCaseStatus.Approved));

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }

    [Fact]
    public async Task Empty_invoice_list_returns_bad_request()
    {
        await using var factory = CreateFactory();
        using var client = CreateTenantClient(factory);

        var response = await client.PostAsJsonAsync(
            "/api/reimbursement/cases", new CreateReimbursementCaseRequest(Array.Empty<Guid>()));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Cases_are_isolated_between_tenants()
    {
        await using var factory = CreateFactory();
        using var clientA = CreateTenantClient(factory, "tenant-a");
        using var clientB = CreateTenantClient(factory, "tenant-b");

        var createResponse = await clientA.PostAsJsonAsync(
            "/api/reimbursement/cases", new CreateReimbursementCaseRequest(new[] { Guid.NewGuid() }));
        var created = await createResponse.Content.ReadFromJsonAsync<ReimbursementCase>();

        var crossTenantGet = await clientB.GetAsync($"/api/reimbursement/cases/{created!.Id}");

        Assert.Equal(HttpStatusCode.NotFound, crossTenantGet.StatusCode);
    }
}
