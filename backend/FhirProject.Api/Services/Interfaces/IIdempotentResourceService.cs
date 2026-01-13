using FhirProject.Api.Models.enums;

namespace FhirProject.Api.Services.Interfaces;

public interface IIdempotentResourceService
{
    Task<string> GetOrCreateResourceIdAsync(string sourceSystem, string externalId, FhirResourceType resourceType, Func<Task<string>> createResourceFunc);
}