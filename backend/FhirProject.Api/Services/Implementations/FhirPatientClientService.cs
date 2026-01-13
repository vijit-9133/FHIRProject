using FhirProject.Api.Services.Interfaces;
using Hl7.Fhir.Model;
using Hl7.Fhir.Rest;
using Microsoft.Extensions.Configuration;
using System.Threading.Tasks;

namespace FhirProject.Api.Services.Implementations;

public class FhirPatientClientService : IFhirPatientClientService
{
    private readonly FhirClient _fhirClient;

    public FhirPatientClientService(IConfiguration configuration)
    {
        var baseUrl = configuration["FhirServer:BaseUrl"];
        _fhirClient = new FhirClient(baseUrl);
        _fhirClient.Settings.PreferredFormat = ResourceFormat.Json;
    }

    public async System.Threading.Tasks.Task<Patient> CreatePatientAsync(Patient patient)
    {
        var createdPatient = await _fhirClient.CreateAsync(patient);
        return createdPatient;
    }

    public async System.Threading.Tasks.Task<Patient?> GetPatientAsync(string patientId)
    {
        return await _fhirClient.ReadAsync<Patient>($"Patient/{patientId}");
    }

    public async System.Threading.Tasks.Task<Patient> UpdatePatientAsync(string patientId, Patient patient)
    {
        patient.Id = patientId;
        var updatedPatient = await _fhirClient.UpdateAsync(patient);
        return updatedPatient;
    }
}