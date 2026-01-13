using FhirProject.Api.Models.entities;
using FhirProject.Api.Models.enums;

namespace FhirProject.Api.Repositories.Interfaces;

public interface IExternalResourceMappingRepository
{
    Task<ExternalResourceMapping?> GetMappingAsync(string sourceSystem, string externalId);
    Task<ExternalResourceMapping?> GetMappingAsync(string sourceSystem, string externalId, FhirResourceType resourceType);
    Task<ExternalResourceMapping> CreateMappingAsync(ExternalResourceMapping mapping);
    Task UpdateMappingAsync(ExternalResourceMapping mapping);
}