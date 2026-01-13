using FhirProject.Api.DTOs.Inbound.V1;
using FhirProject.Api.Models.entities;
using FhirProject.Api.Models.enums;
using FhirProject.Api.Services.Interfaces;
using FhirProject.Api.Repositories.Interfaces;

namespace FhirProject.Api.Services.Implementations;

public class InboundIntegrationOrchestrator
{
    private readonly IConversionLifecycleService _lifecycleService;
    private readonly IInboundNormalizationService _normalizationService;
    private readonly ITerminologyMappingService _terminologyService;
    private readonly IConversionRequestRepository _conversionRepository;

    public InboundIntegrationOrchestrator(
        IConversionLifecycleService lifecycleService,
        IInboundNormalizationService normalizationService,
        ITerminologyMappingService terminologyService,
        IConversionRequestRepository conversionRepository)
    {
        _lifecycleService = lifecycleService;
        _normalizationService = normalizationService;
        _terminologyService = terminologyService;
        _conversionRepository = conversionRepository;
    }

    public async Task<int> ProcessExternalHealthcareEventAsync(ExternalHealthcareEventV1 externalEvent, int? userId = null)
    {
        // Create initial conversion request
        var conversionRequest = new ConversionRequestEntity
        {
            ResourceType = "ExternalHealthcareEvent",
            InputDataJson = System.Text.Json.JsonSerializer.Serialize(externalEvent),
            Status = ConversionStatus.Received,
            UserId = userId,
            CreatedAt = DateTime.UtcNow
        };

        var savedRequest = await _conversionRepository.CreateAsync(conversionRequest);

        try
        {
            // Step 1: Normalize
            await _lifecycleService.UpdateStatusAsync(savedRequest.Id, ConversionStatus.Normalized);
            var normalizedEvent = _normalizationService.Normalize(externalEvent);

            // Step 2: Terminology Mapping
            await _lifecycleService.UpdateStatusAsync(savedRequest.Id, ConversionStatus.TerminologyMapped);
            await ApplyTerminologyMappings(normalizedEvent);

            // Step 3: FHIR Creation
            await _lifecycleService.UpdateStatusAsync(savedRequest.Id, ConversionStatus.FhirCreated);
            // TODO: Create FHIR resources using existing mappers

            // Step 4: FHIR Validation
            await _lifecycleService.UpdateStatusAsync(savedRequest.Id, ConversionStatus.FhirValidated);
            // TODO: Validate FHIR resources

            // Step 5: Storage
            await _lifecycleService.UpdateStatusAsync(savedRequest.Id, ConversionStatus.Stored);

            return savedRequest.Id;
        }
        catch (Exception ex)
        {
            await _lifecycleService.UpdateStatusAsync(savedRequest.Id, ConversionStatus.Failed, ex.Message);
            throw;
        }
    }

    private async Task ApplyTerminologyMappings(Models.Normalized.NormalizedHealthcareEvent normalizedEvent)
    {
        // Apply terminology mappings (placeholder implementation)
        if (normalizedEvent.Encounter?.EncounterType != null)
        {
            var encounterMapping = _terminologyService.MapEncounterTypeToFhir(normalizedEvent.Encounter.EncounterType);
            // Store mapping result in normalized event or separate tracking
        }

        if (normalizedEvent.Practitioner?.Specialty != null)
        {
            var specialtyMapping = _terminologyService.MapPractitionerSpecialtyToSnomed(normalizedEvent.Practitioner.Specialty);
            // Store mapping result
        }

        await Task.CompletedTask;
    }
}