using FhirProject.Api.Models.entities;

namespace FhirProject.Api.Services.Interfaces;

public interface IFhirResourceService
{
    Task<string?> GetPatientByIdAsync(string fhirId);
    Task<List<string>> SearchPatientsByIdentifierAsync(string identifier);
    Task<List<string>> SearchEncountersByPatientAsync(string patientId);
}