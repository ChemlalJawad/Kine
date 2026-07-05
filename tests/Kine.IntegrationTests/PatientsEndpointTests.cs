using System;
using System.Net;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Kine.Api.Modules;
using Kine.Modules.Patients.Domain;
using Microsoft.AspNetCore.Mvc.Testing;
using Xunit;

namespace Kine.IntegrationTests;

public class PatientsEndpointTests
{
    private static WebApplicationFactory<Program> CreateFactory() => new();

    private static HttpClient CreateTenantClient(WebApplicationFactory<Program> factory, string tenantId = "tenant-001")
    {
        var client = factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Tenant-Id", tenantId);
        return client;
    }

    [Fact]
    public async Task Patients_endpoints_reject_requests_without_tenant()
    {
        await using var factory = CreateFactory();
        using var client = factory.CreateClient();

        using var response = await client.GetAsync("/api/patients");

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Create_then_get_patient_roundtrips()
    {
        await using var factory = CreateFactory();
        using var client = CreateTenantClient(factory);

        var createResponse = await client.PostAsJsonAsync("/api/patients", new CreatePatientRequest("Jean", "Dupont", new DateOnly(1990, 1, 1)));
        Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

        var created = await createResponse.Content.ReadFromJsonAsync<Patient>();
        Assert.NotNull(created);

        var getResponse = await client.GetAsync($"/api/patients/{created!.Id}");
        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
    }

    [Fact]
    public async Task Patients_are_isolated_between_tenants()
    {
        await using var factory = CreateFactory();
        using var clientA = CreateTenantClient(factory, "tenant-a");
        using var clientB = CreateTenantClient(factory, "tenant-b");

        var createResponse = await clientA.PostAsJsonAsync("/api/patients", new CreatePatientRequest("Jean", "Dupont", null));
        var created = await createResponse.Content.ReadFromJsonAsync<Patient>();

        var crossTenantGet = await clientB.GetAsync($"/api/patients/{created!.Id}");

        Assert.Equal(HttpStatusCode.NotFound, crossTenantGet.StatusCode);
    }

    [Fact]
    public async Task Delete_patient_archives_instead_of_removing()
    {
        await using var factory = CreateFactory();
        using var client = CreateTenantClient(factory);

        var createResponse = await client.PostAsJsonAsync("/api/patients", new CreatePatientRequest("Jean", "Dupont", null));
        var created = await createResponse.Content.ReadFromJsonAsync<Patient>();

        var deleteResponse = await client.DeleteAsync($"/api/patients/{created!.Id}");
        Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

        var getResponse = await client.GetAsync($"/api/patients/{created.Id}");
        var patient = await getResponse.Content.ReadFromJsonAsync<Patient>();

        Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
        Assert.Equal(PatientStatus.Archived, patient!.Status);
    }

    [Fact]
    public async Task Contact_and_consent_crud_roundtrips_under_patient()
    {
        await using var factory = CreateFactory();
        using var client = CreateTenantClient(factory);

        var createResponse = await client.PostAsJsonAsync("/api/patients", new CreatePatientRequest("Jean", "Dupont", null));
        var patient = await createResponse.Content.ReadFromJsonAsync<Patient>();

        var contactResponse = await client.PostAsJsonAsync($"/api/patients/{patient!.Id}/contacts",
            new CreatePatientContactRequest(PatientContactType.Email, "jean@example.com", true));
        Assert.Equal(HttpStatusCode.Created, contactResponse.StatusCode);

        var contactsResponse = await client.GetAsync($"/api/patients/{patient.Id}/contacts");
        Assert.Equal(HttpStatusCode.OK, contactsResponse.StatusCode);

        var consentResponse = await client.PostAsJsonAsync($"/api/patients/{patient.Id}/consents",
            new CreatePatientConsentRequest(ConsentType.TraitementDonnees, true));
        Assert.Equal(HttpStatusCode.Created, consentResponse.StatusCode);

        var consent = await consentResponse.Content.ReadFromJsonAsync<PatientConsent>();
        var revokeResponse = await client.PostAsync($"/api/patients/{patient.Id}/consents/{consent!.Id}/revoke", content: null);
        Assert.Equal(HttpStatusCode.OK, revokeResponse.StatusCode);
    }

    [Fact]
    public async Task Create_patient_with_empty_first_name_returns_400_not_500()
    {
        // Regression P0-016: ArgumentException from the service used to escape
        // the endpoint (only InvalidOperationException was caught) -> 500.
        await using var factory = CreateFactory();
        using var client = CreateTenantClient(factory);

        var response = await client.PostAsJsonAsync("/api/patients", new CreatePatientRequest("", "Dupont", null));

        Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
    }

    [Fact]
    public async Task Update_contact_of_another_patient_returns_404()
    {
        // Regression P0-017: the patient id in the route was ignored, so a
        // contact belonging to another patient (same tenant) was modifiable
        // and the audit trail recorded a wrong patient reference.
        await using var factory = CreateFactory();
        using var client = CreateTenantClient(factory);

        var patientAResponse = await client.PostAsJsonAsync("/api/patients", new CreatePatientRequest("Jean", "Dupont", null));
        var patientA = await patientAResponse.Content.ReadFromJsonAsync<Patient>();
        var patientBResponse = await client.PostAsJsonAsync("/api/patients", new CreatePatientRequest("Marie", "Curie", null));
        var patientB = await patientBResponse.Content.ReadFromJsonAsync<Patient>();

        var contactResponse = await client.PostAsJsonAsync($"/api/patients/{patientB!.Id}/contacts",
            new CreatePatientContactRequest(PatientContactType.Phone, "+32100000", true));
        var contactOfB = await contactResponse.Content.ReadFromJsonAsync<PatientContact>();

        var crossPatientUpdate = await client.PutAsJsonAsync($"/api/patients/{patientA!.Id}/contacts/{contactOfB!.Id}",
            new UpdatePatientContactRequest("+32199999", false));

        Assert.Equal(HttpStatusCode.NotFound, crossPatientUpdate.StatusCode);
    }

    [Fact]
    public async Task Delete_missing_contact_returns_404_without_phantom_audit_event()
    {
        // Regression P0-017: deleting a nonexistent contact used to return 204
        // and still write an audit event.
        await using var factory = CreateFactory();
        using var client = CreateTenantClient(factory);

        var createResponse = await client.PostAsJsonAsync("/api/patients", new CreatePatientRequest("Jean", "Dupont", null));
        var patient = await createResponse.Content.ReadFromJsonAsync<Patient>();

        var deleteResponse = await client.DeleteAsync($"/api/patients/{patient!.Id}/contacts/{Guid.NewGuid()}");

        Assert.Equal(HttpStatusCode.NotFound, deleteResponse.StatusCode);
    }
}
