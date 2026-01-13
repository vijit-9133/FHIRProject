using FhirProject.Api.Models.enums;

namespace FhirProject.Api.Models.Results;

public class HealthcareEventResult
{
    public int ConversionRequestId { get; set; }
    public string? InternalPatientFhirId { get; set; }
    public string? InternalPractitionerFhirId { get; set; }
    public string? InternalEncounterFhirId { get; set; }
    public ConversionStatus Status { get; set; }
    public bool WasProcessed { get; set; }
    public string? Message { get; set; }
}