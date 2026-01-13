using FhirProject.Api.Models.enums;

namespace FhirProject.Api.Models.entities
{
    public class ConversionRequestEntity
{
    public int Id { get; set; }

    public string ResourceType { get; set; }

    // Raw non-FHIR input JSON
    public string InputDataJson { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public ConversionStatus Status { get; set; } = ConversionStatus.Pending;

    public string? ErrorMessage { get; set; }

    public string MappingVersion { get; set; } = "v1";

    public InputSourceType InputSourceType { get; set; } = InputSourceType.Form;

    public decimal? ExtractionConfidence { get; set; }

    public string? ExtractionWarnings { get; set; }

    // Nullable ownership field for future user-specific data access
    public int? UserId { get; set; }

    // Lifecycle tracking fields
    public DateTime? NormalizedAt { get; set; }
    public DateTime? TerminologyMappedAt { get; set; }
    public DateTime? FhirCreatedAt { get; set; }
    public DateTime? FhirValidatedAt { get; set; }
    public DateTime? StoredAt { get; set; }
    public string? FailureReason { get; set; }
    public string? FailureStage { get; set; }
}
}