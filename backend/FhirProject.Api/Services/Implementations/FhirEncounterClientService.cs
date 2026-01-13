using FhirProject.Api.Services.Interfaces;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace FhirProject.Api.Services.Implementations;

public class FhirEncounterClientService : IFhirEncounterClientService
{
    private readonly FhirClient _fhirClient;

    public FhirEncounterClientService(IConfiguration configuration)
    {
        var baseUrl = configuration["FhirServer:BaseUrl"];
        _fhirClient = new FhirClient(baseUrl);
        _fhirClient.Settings.PreferredFormat = ResourceFormat.Json;
    }

    public async System.Threading.Tasks.Task<Encounter> CreateEncounterAsync(Encounter encounter)
    {
        var createdEncounter = await _fhirClient.CreateAsync(encounter);
        return createdEncounter;
    }

    public async System.Threading.Tasks.Task<Encounter?> GetEncounterAsync(string encounterId)
    {
        return await _fhirClient.ReadAsync<Encounter>($"Encounter/{encounterId}");
    }

    public async System.Threading.Tasks.Task UpdateEncounterAsync(Encounter encounter)
    {
        await _fhirClient.UpdateAsync(encounter);
    }
}