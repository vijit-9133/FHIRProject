using FhirProject.Api.Models.Normalized;

namespace FhirProject.Api.Services.Interfaces;

public interface IFhirPatientUpsertService
{
    Task<string> UpsertPatientAsync(string sourceSystem, string externalPatientId, NormalizedPatient normalizedPatient);
}