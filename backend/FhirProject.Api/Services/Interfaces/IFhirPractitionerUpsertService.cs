using FhirProject.Api.Models.Normalized;

namespace FhirProject.Api.Services.Interfaces;

public interface IFhirPractitionerUpsertService
{
    Task<string> UpsertPractitionerAsync(string sourceSystem, string externalPractitionerId, NormalizedPractitioner normalizedPractitioner);
}