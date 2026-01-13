using FhirProject.Api.Services.Interfaces;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace FhirProject.Api.Services.Implementations;

public class FhirPractitionerClientService : IFhirPractitionerClientService
{
    private readonly FhirClient _fhirClient;

    public FhirPractitionerClientService(IConfiguration configuration)
    {
        var baseUrl = configuration["FhirServer:BaseUrl"];
        _fhirClient = new FhirClient(baseUrl);
        _fhirClient.Settings.PreferredFormat = ResourceFormat.Json;
    }

    public async System.Threading.Tasks.Task<Practitioner> CreatePractitionerAsync(Practitioner practitioner)
    {
        var createdPractitioner = await _fhirClient.CreateAsync(practitioner);
        return createdPractitioner;
    }

    public async System.Threading.Tasks.Task<Practitioner?> GetPractitionerAsync(string practitionerId)
    {
        return await _fhirClient.ReadAsync<Practitioner>($"Practitioner/{practitionerId}");
    }

    public async System.Threading.Tasks.Task UpdatePractitionerAsync(Practitioner practitioner)
    {
        await _fhirClient.UpdateAsync(practitioner);
    }
}