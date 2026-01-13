using FhirProject.Api.Models.entities;
using FhirProject.Api.Models.enums;
using FhirProject.Api.Repositories.Interfaces;
using FhirProject.Api.Services.Interfaces;

namespace FhirProject.Api.Services.Implementations;

public class ConversionLifecycleService : IConversionLifecycleService
{
    private readonly IConversionRequestRepository _conversionRepository;

    public ConversionLifecycleService(IConversionRequestRepository conversionRepository)
    {
        _conversionRepository = conversionRepository;
    }

    public async Task UpdateStatusAsync(int conversionRequestId, ConversionStatus status, string? failureReason = null)
    {
        var conversion = await _conversionRepository.GetByIdAsync(conversionRequestId);
        if (conversion == null)
            throw new ArgumentException($"Conversion request {conversionRequestId} not found");

        var now = DateTime.UtcNow;
        
        conversion.Status = status;

        // Update lifecycle timestamps
        switch (status)
        {
            case ConversionStatus.Received:
                break; // CreatedAt already set
            case ConversionStatus.Normalized:
                conversion.NormalizedAt = now;
                break;
            case ConversionStatus.TerminologyMapped:
                conversion.TerminologyMappedAt = now;
                break;
            case ConversionStatus.FhirCreated:
                conversion.FhirCreatedAt = now;
                break;
            case ConversionStatus.FhirValidated:
                conversion.FhirValidatedAt = now;
                break;
            case ConversionStatus.Stored:
                conversion.StoredAt = now;
                break;
            case ConversionStatus.Failed:
                conversion.FailureReason = failureReason;
                conversion.FailureStage = GetCurrentStage(conversion);
                break;
        }

        await _conversionRepository.UpdateAsync(conversion);
    }

    public async Task<ConversionRequestEntity?> GetConversionWithLifecycleAsync(int conversionRequestId)
    {
        return await _conversionRepository.GetByIdAsync(conversionRequestId);
    }

    private string GetCurrentStage(ConversionRequestEntity conversion)
    {
        if (conversion.StoredAt.HasValue) return "STORED";
        if (conversion.FhirValidatedAt.HasValue) return "FHIR_VALIDATED";
        if (conversion.FhirCreatedAt.HasValue) return "FHIR_CREATED";
        if (conversion.TerminologyMappedAt.HasValue) return "TERMINOLOGY_MAPPED";
        if (conversion.NormalizedAt.HasValue) return "NORMALIZED";
        return "RECEIVED";
    }
}