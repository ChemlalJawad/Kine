using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Kine.Api.Modules;
using Kine.Modules.Billing.Domain;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Kine.IntegrationTests;

public class BillingEndpointTests
{
    private const string KnownCode = "558014";

    private static WebApplicationFactory<Program> CreateFactory() => new();

    private static HttpClient CreateTenantClient(WebApplicationFactory<Program> factory, string tenantId = "tenant-001")
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId);
        return client;
    }

    [Fact]
    public async Task Billing_endpoints_reject_requests_without_tenant()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/api/billing/invoices");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Actes_catalog_is_available()
    {
        await using var factory = CreateFactory();
        using var client = CreateTenantClient(factory);

        var actes = await client.GetFromJsonAsync<ActeInami[]>("/api/billing/actes");

        Assert.NotNull(actes);
        Assert.NotEmpty(actes!);
    }

    [Fact]
    public async Task Create_invoice_then_mark_reimbursed_roundtrips()
    {
        await using var factory = CreateFactory();
        using var client = CreateTenantClient(factory);

        var createResponse = await client.PostAsJsonAsync(
            "/api/billing/invoices", new CreateInvoiceRequest(Guid.NewGuid(), KnownCode, "Solidaris"));
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var invoice = await createResponse.Content.ReadFromJsonAsync<Invoice>();
        Assert.Equal(InvoiceStatus.Pending, invoice!.Status);

        var reimburseResponse = await client.PostAsync($"/api/billing/invoices/{invoice.Id}/mark-reimbursed", content: null);
        Assert.Equal(HttpStatusCode.OK, reimburseResponse.StatusCode);

        var updated = await reimburseResponse.Content.ReadFromJsonAsync<Invoice>();
        Assert.Equal(InvoiceStatus.Reimbursed, updated!.Status);
    }

    [Fact]
    public async Task Create_invoice_with_unknown_code_returns_bad_request()
    {
        await using var factory = CreateFactory();
        using var client = CreateTenantClient(factory);

        var response = await client.PostAsJsonAsync(
            "/api/billing/invoices", new CreateInvoiceRequest(Guid.NewGuid(), "000000", null));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Double_transition_returns_conflict()
    {
        await using var factory = CreateFactory();
        using var client = CreateTenantClient(factory);

        var createResponse = await client.PostAsJsonAsync(
            "/api/billing/invoices", new CreateInvoiceRequest(Guid.NewGuid(), KnownCode, null));
        var invoice = await createResponse.Content.ReadFromJsonAsync<Invoice>();

        await client.PostAsync($"/api/billing/invoices/{invoice!.Id}/mark-reimbursed", content: null);
        var second = await client.PostAsync($"/api/billing/invoices/{invoice.Id}/mark-rejected", content: null);

        Assert.Equal(HttpStatusCode.Conflict, second.StatusCode);
    }

    [Fact]
    public async Task Invoices_are_isolated_between_tenants()
    {
        await using var factory = CreateFactory();
        using var clientA = CreateTenantClient(factory, "tenant-a");
        using var clientB = CreateTenantClient(factory, "tenant-b");

        var createResponse = await clientA.PostAsJsonAsync(
            "/api/billing/invoices", new CreateInvoiceRequest(Guid.NewGuid(), KnownCode, null));
        var invoice = await createResponse.Content.ReadFromJsonAsync<Invoice>();

        var crossTenantGet = await clientB.GetAsync($"/api/billing/invoices/{invoice!.Id}");

        Assert.Equal(HttpStatusCode.NotFound, crossTenantGet.StatusCode);
    }
}
