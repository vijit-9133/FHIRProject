using FhirProject.Api.Models.Normalized;
using FhirProject.Api.Services.Interfaces;

namespace FhirProject.Api.Services.Implementations;

public class HealthcareEventProcessingOrchestrator
{
    private readonly IFhirPatientUpsertService _patientUpsertService;
    private readonly IFhirPractitionerUpsertService _practitionerUpsertService;
    private readonly IFhirEncounterUpsertService _encounterUpsertService;

    public HealthcareEventProcessingOrchestrator(
        IFhirPatientUpsertService patientUpsertService,
        IFhirPractitionerUpsertService practitionerUpsertService,
        IFhirEncounterUpsertService encounterUpsertService)
    {
        _patientUpsertService = patientUpsertService;
        _practitionerUpsertService = practitionerUpsertService;
        _encounterUpsertService = encounterUpsertService;
    }

    public async Task<(string PatientId, string PractitionerId, string EncounterId)> ProcessHealthcareEventAsync(
        string sourceSystem, 
        NormalizedHealthcareEvent healthcareEvent)
    {
        // Step 1: Upsert Patient
        var patientId = await _patientUpsertService.UpsertPatientAsync(
            sourceSystem,
            healthcareEvent.Patient!.ExternalPatientId,
            healthcareEvent.Patient);

        // Step 2: Upsert Practitioner
        var practitionerId = await _practitionerUpsertService.UpsertPractitionerAsync(
            sourceSystem,
            healthcareEvent.Practitioner!.ExternalPractitionerId,
            healthcareEvent.Practitioner);

        // Step 3: Upsert Encounter (depends on Patient and Practitioner)
        var encounterId = await _encounterUpsertService.UpsertEncounterAsync(
            sourceSystem,
            healthcareEvent.Encounter!.ExternalEncounterId,
            healthcareEvent.Encounter,
            patientId,
            practitionerId);

        return (patientId, practitionerId, encounterId);
    }
}