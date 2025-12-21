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
}
}