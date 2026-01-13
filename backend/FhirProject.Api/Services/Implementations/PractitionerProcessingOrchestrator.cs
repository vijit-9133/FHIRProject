using FhirProject.Api.Models.Normalized;
using FhirProject.Api.Services.Interfaces;

namespace FhirProject.Api.Services.Implementations;

public class PractitionerProcessingOrchestrator
{
    private readonly IFhirPractitionerUpsertService _practitionerUpsertService;

    public PractitionerProcessingOrchestrator(IFhirPractitionerUpsertService practitionerUpsertService)
    {
        _practitionerUpsertService = practitionerUpsertService;
    }

    public async Task<string> ProcessPractitionerAsync(string sourceSystem, NormalizedPractitioner normalizedPractitioner)
    {
        // Use the idempotent upsert service
        var internalPractitionerId = await _practitionerUpsertService.UpsertPractitionerAsync(
            sourceSystem,
            normalizedPractitioner.ExternalPractitionerId,
            normalizedPractitioner);

        return internalPractitionerId;
    }
}