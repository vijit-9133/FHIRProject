using FhirProject.Api.Models.Normalized;

namespace FhirProject.Api.Services.Interfaces;

public interface IFhirEncounterUpsertService
{
    Task<string> UpsertEncounterAsync(
        string sourceSystem, 
        string externalEncounterId, 
        NormalizedEncounter normalizedEncounter,
        string internalPatientFhirId,
        string internalPractitionerFhirId);
}