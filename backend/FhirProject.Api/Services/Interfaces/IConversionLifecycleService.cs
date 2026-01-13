using FhirProject.Api.Models.entities;
using FhirProject.Api.Models.enums;
using FhirProject.Api.Repositories.Interfaces;

namespace FhirProject.Api.Services.Interfaces;

public interface IConversionLifecycleService
{
    Task UpdateStatusAsync(int conversionRequestId, ConversionStatus status, string? failureReason = null);
    Task<ConversionRequestEntity?> GetConversionWithLifecycleAsync(int conversionRequestId);
}