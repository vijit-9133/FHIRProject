using FhirProject.Api.Models.entities;
using FhirProject.Api.Models.enums;
using FhirProject.Api.Models.Normalized;
using FhirProject.Api.Models.Results;
using FhirProject.Api.Repositories.Interfaces;
using FhirProject.Api.Services.Interfaces;
using System.Collections.Concurrent;
using System.Text.Json;

namespace FhirProject.Api.Services.Implementations;

public class HealthcareEventIdempotencyService : IHealthcareEventIdempotencyService
{
    private readonly IConversionRequestRepository _conversionRepository;
    private readonly IFhirPatientUpsertService _patientUpsertService;
    private readonly IFhirPractitionerUpsertService _practitionerUpsertService;
    private readonly IFhirEncounterUpsertService _encounterUpsertService;
    private readonly ConcurrentDictionary<string, SemaphoreSlim> _semaphores = new();

    public HealthcareEventIdempotencyService(
        IConversionRequestRepository conversionRepository,
        IFhirPatientUpsertService patientUpsertService,
        IFhirPractitionerUpsertService practitionerUpsertService,
        IFhirEncounterUpsertService encounterUpsertService)
    {
        _conversionRepository = conversionRepository;
        _patientUpsertService = patientUpsertService;
        _practitionerUpsertService = practitionerUpsertService;
        _encounterUpsertService = encounterUpsertService;
    }

    public async Task<HealthcareEventResult> ProcessHealthcareEventAsync(NormalizedHealthcareEvent healthcareEvent)
    {
        var eventType = "ExternalHealthcareEvent";
        var lockKey = $"{healthcareEvent.SourceSystem}:{healthcareEvent.ExternalReferenceId}:{eventType}";
        var semaphore = _semaphores.GetOrAdd(lockKey, _ => new SemaphoreSlim(1, 1));

        await semaphore.WaitAsync();
        try
        {
            // Query ConversionRequests for existing event
            var existingRequest = await _conversionRepository.GetByExternalReferenceAsync(
                healthcareEvent.SourceSystem,
                healthcareEvent.ExternalReferenceId,
                eventType);

            if (existingRequest != null)
            {
                // Handle existing request based on status
                switch (existingRequest.Status)
                {
                    case ConversionStatus.Completed:
                        // Return stored FHIR IDs without reprocessing
                        return ExtractResultFromCompletedRequest(existingRequest);

                    case ConversionStatus.InProgress:
                        // Block duplicate processing - return current state
                        return new HealthcareEventResult
                        {
                            ConversionRequestId = existingRequest.Id,
                            Status = ConversionStatus.InProgress,
                            WasProcessed = false,
                            Message = "Event is currently being processed"
                        };

                    case ConversionStatus.Failed:
                        // Allow retry by continuing to processing
                        await UpdateRequestStatus(existingRequest.Id, ConversionStatus.InProgress);
                        return await ProcessNewEvent(healthcareEvent, existingRequest.Id);

                    default:
                        // For any other status, continue processing
                        await UpdateRequestStatus(existingRequest.Id, ConversionStatus.InProgress);
                        return await ProcessNewEvent(healthcareEvent, existingRequest.Id);
                }
            }

            // No existing record - create new ConversionRequest
            var newRequest = new ConversionRequestEntity
            {
                ResourceType = eventType,
                InputDataJson = JsonSerializer.Serialize(healthcareEvent),
                Status = ConversionStatus.InProgress,
                CreatedAt = DateTime.UtcNow
            };

            var savedRequest = await _conversionRepository.CreateAsync(newRequest);
            return await ProcessNewEvent(healthcareEvent, savedRequest.Id);
        }
        finally
        {
            semaphore.Release();
            
            // Clean up semaphore if no other threads are waiting
            if (semaphore.CurrentCount == 1)
            {
                _semaphores.TryRemove(lockKey, out _);
                semaphore.Dispose();
            }
        }
    }

    private async Task<HealthcareEventResult> ProcessNewEvent(NormalizedHealthcareEvent healthcareEvent, int requestId)
    {
        try
        {
            var result = new HealthcareEventResult
            {
                ConversionRequestId = requestId,
                WasProcessed = true
            };

            // Step 1: Upsert Patient
            if (healthcareEvent.Patient != null)
            {
                result.InternalPatientFhirId = await _patientUpsertService.UpsertPatientAsync(
                    healthcareEvent.SourceSystem,
                    healthcareEvent.Patient.ExternalPatientId,
                    healthcareEvent.Patient);
            }

            // Step 2: Upsert Practitioner
            if (healthcareEvent.Practitioner != null)
            {
                result.InternalPractitionerFhirId = await _practitionerUpsertService.UpsertPractitionerAsync(
                    healthcareEvent.SourceSystem,
                    healthcareEvent.Practitioner.ExternalPractitionerId,
                    healthcareEvent.Practitioner);
            }

            // Step 3: Upsert Encounter
            if (healthcareEvent.Encounter != null && result.InternalPatientFhirId != null && result.InternalPractitionerFhirId != null)
            {
                result.InternalEncounterFhirId = await _encounterUpsertService.UpsertEncounterAsync(
                    healthcareEvent.SourceSystem,
                    healthcareEvent.Encounter.ExternalEncounterId,
                    healthcareEvent.Encounter,
                    result.InternalPatientFhirId,
                    result.InternalPractitionerFhirId);
            }

            // Store FHIR IDs and mark as completed
            await UpdateRequestWithResults(requestId, result, ConversionStatus.Completed);
            result.Status = ConversionStatus.Completed;
            result.Message = "Healthcare event processed successfully";

            return result;
        }
        catch (Exception ex)
        {
            // Mark as failed and store error details
            await UpdateRequestStatus(requestId, ConversionStatus.Failed, ex.Message, "FHIR_PROCESSING");
            
            return new HealthcareEventResult
            {
                ConversionRequestId = requestId,
                Status = ConversionStatus.Failed,
                WasProcessed = false,
                Message = "Failed to process healthcare event"
            };
        }
    }

    private HealthcareEventResult ExtractResultFromCompletedRequest(ConversionRequestEntity request)
    {
        // Parse stored FHIR IDs from InputDataJson or use a separate storage mechanism
        // For now, return basic completed result - in production, store FHIR IDs separately
        return new HealthcareEventResult
        {
            ConversionRequestId = request.Id,
            Status = ConversionStatus.Completed,
            WasProcessed = false,
            Message = "Event already processed - returning cached result"
        };
    }

    private async Task UpdateRequestStatus(int requestId, ConversionStatus status, string? errorMessage = null, string? failureStage = null)
    {
        var request = await _conversionRepository.GetByIdAsync(requestId);
        if (request != null)
        {
            request.Status = status;
            if (!string.IsNullOrEmpty(errorMessage))
            {
                request.FailureReason = errorMessage;
            }
            if (!string.IsNullOrEmpty(failureStage))
            {
                request.FailureStage = failureStage;
            }
            await _conversionRepository.UpdateAsync(request);
        }
    }

    private async Task UpdateRequestWithResults(int requestId, HealthcareEventResult result, ConversionStatus status)
    {
        var request = await _conversionRepository.GetByIdAsync(requestId);
        if (request != null)
        {
            request.Status = status;
            // Store FHIR IDs in a structured way within InputDataJson or use separate storage
            // For now, just update status - in production, extend entity or use separate table
            await _conversionRepository.UpdateAsync(request);
        }
    }
}