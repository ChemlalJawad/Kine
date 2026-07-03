using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Kine.Api.Modules;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Kine.IntegrationTests;

/// <summary>
/// P0-006: matrice de permissions RBAC appliquee des que des roles sont fournis
/// (header X-Roles en dev/demo, claims OIDC en prod). Sans information de role,
/// les requetes passent (fallback dev, aligne sur le gate MFA).
/// </summary>
public class RbacEndpointTests
{
    private static WebApplicationFactory<Program> CreateFactory() => new();

    private static HttpClient CreateClient(WebApplicationFactory<Program> factory, string? roles)
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Tenant-Id", "tenant-001");
        if (roles is not null)
        {
            client.DefaultRequestHeaders.Add("X-Roles", roles);
        }

        return client;
    }

    [Fact]
    public async Task Requests_without_role_information_pass_through()
    {
        await using var factory = CreateFactory();
        using var client = CreateClient(factory, roles: null);

        var response = await client.GetAsync("/api/billing/invoices");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Assistant_cannot_read_billing()
    {
        await using var factory = CreateFactory();
        using var client = CreateClient(factory, "Assistant");

        var response = await client.GetAsync("/api/billing/invoices");

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Billing_role_can_create_invoice_but_not_write_patients()
    {
        await using var factory = CreateFactory();
        using var client = CreateClient(factory, "Billing");

        var invoiceResponse = await client.PostAsJsonAsync(
            "/api/billing/invoices", new CreateInvoiceRequest(Guid.NewGuid(), "558014", null));
        Assert.Equal(HttpStatusCode.Created, invoiceResponse.StatusCode);

        var patientResponse = await client.PostAsJsonAsync(
            "/api/patients", new CreatePatientRequest("Jean", "Test", null));
        Assert.Equal(HttpStatusCode.Forbidden, patientResponse.StatusCode);
    }

    [Fact]
    public async Task Kine_can_write_patients_and_clinical_but_not_reporting()
    {
        await using var factory = CreateFactory();
        using var client = CreateClient(factory, "Kine");

        var patientResponse = await client.PostAsJsonAsync(
            "/api/patients", new CreatePatientRequest("Jean", "Test", null));
        Assert.Equal(HttpStatusCode.Created, patientResponse.StatusCode);

        var reportingResponse = await client.GetAsync("/api/reporting/summary");
        Assert.Equal(HttpStatusCode.Forbidden, reportingResponse.StatusCode);
    }

    [Fact]
    public async Task Admin_cabinet_has_access_everywhere()
    {
        await using var factory = CreateFactory();
        using var client = CreateClient(factory, "AdminCabinet");

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/patients")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/billing/invoices")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/reimbursement/cases")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/reporting/summary")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/audit/events")).StatusCode);
    }

    [Fact]
    public async Task Roles_are_combined_when_multiple_are_sent()
    {
        await using var factory = CreateFactory();
        using var client = CreateClient(factory, "Assistant,Billing");

        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/billing/invoices")).StatusCode);
        Assert.Equal(HttpStatusCode.OK, (await client.GetAsync("/api/patients")).StatusCode);
    }
}
