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

public class ReportingEndpointTests
{
    private static WebApplicationFactory<Program> CreateFactory() => new();

    private static HttpClient CreateTenantClient(WebApplicationFactory<Program> factory, string tenantId = "tenant-001")
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId);
        return client;
    }

    [Fact]
    public async Task Summary_aggregates_created_data_by_month()
    {
        await using var factory = CreateFactory();
        using var client = CreateTenantClient(factory);

        // One patient + one invoice this month.
        var patientResponse = await client.PostAsJsonAsync(
            "/api/patients", new CreatePatientRequest("Jean", "Test", null));
        Assert.Equal(HttpStatusCode.Created, patientResponse.StatusCode);

        var invoiceResponse = await client.PostAsJsonAsync(
            "/api/billing/invoices", new CreateInvoiceRequest(Guid.NewGuid(), "558014", null));
        Assert.Equal(HttpStatusCode.Created, invoiceResponse.StatusCode);

        var summary = await client.GetFromJsonAsync<JsonElement>("/api/reporting/summary");

        Assert.Equal(1, summary.GetProperty("patientsActive").GetInt32());
        var months = summary.GetProperty("months");
        Assert.Equal(1, months.GetArrayLength());

        var currentMonth = months[0];
        Assert.Equal(DateTime.UtcNow.ToString("yyyy-MM"), currentMonth.GetProperty("month").GetString());
        Assert.Equal(1, currentMonth.GetProperty("pendingInvoices").GetInt32());
        Assert.Equal(23.45m, currentMonth.GetProperty("invoicedAmount").GetDecimal());
    }

    [Fact]
    public async Task Csv_export_returns_csv_content()
    {
        await using var factory = CreateFactory();
        using var client = CreateTenantClient(factory);

        await client.PostAsJsonAsync("/api/billing/invoices", new CreateInvoiceRequest(Guid.NewGuid(), "558014", null));

        var response = await client.GetAsync("/api/reporting/export.csv");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("text/csv", response.Content.Headers.ContentType?.MediaType);

        var body = await response.Content.ReadAsStringAsync();
        Assert.StartsWith("mois;", body);
        Assert.Contains("23.45", body);
    }
}
