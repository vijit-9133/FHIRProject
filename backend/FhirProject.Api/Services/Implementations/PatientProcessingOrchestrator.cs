using FhirProject.Api.Models.Normalized;
using FhirProject.Api.Services.Interfaces;

namespace FhirProject.Api.Services.Implementations;

/// <summary>
/// Example service showing how to use FhirPatientUpsertService from orchestration layer
/// </summary>
public class PatientProcessingOrchestrator
{
    private readonly IFhirPatientUpsertService _fhirPatientUpsertService;

    public PatientProcessingOrchestrator(IFhirPatientUpsertService fhirPatientUpsertService)
    {
        _fhirPatientUpsertService = fhirPatientUpsertService;
    }

    /// <summary>
    /// Example: Process a normalized patient with true idempotent behavior
    /// </summary>
    public async Task<string> ProcessPatientAsync(string sourceSystem, NormalizedPatient normalizedPatient)
    {
        // This call ensures:
        // 1. If Patient exists (by SourceSystem + ExternalPatientId + ResourceType.Patient), UPDATE it
        // 2. If Patient doesn't exist, CREATE it and save mapping
        // 3. Always return the same InternalResourceId for the same external Patient
        var patientFhirId = await _fhirPatientUpsertService.UpsertPatientAsync(
            sourceSystem,
            normalizedPatient.ExternalPatientId,
            normalizedPatient
        );

        return patientFhirId;
    }

    /// <summary>
    /// Example: Batch processing multiple patients with idempotent behavior
    /// </summary>
    public async Task<Dictionary<string, string>> ProcessPatientsAsync(
        string sourceSystem, 
        List<NormalizedPatient> patients)
    {
        var results = new Dictionary<string, string>();

        foreach (var patient in patients)
        {
            var fhirId = await _fhirPatientUpsertService.UpsertPatientAsync(
                sourceSystem,
                patient.ExternalPatientId,
                patient
            );
            
            results[patient.ExternalPatientId] = fhirId;
        }

        return results;
    }
}