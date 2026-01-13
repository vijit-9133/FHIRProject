using System.Text.Json.Serialization;

namespace FhirProject.Api.DTOs.Inbound.V1;

public class ExternalHealthcareEventV1
{
    [JsonPropertyName("sourceSystem")]
    public required string SourceSystem { get; set; }

    [JsonPropertyName("sourceSystemVersion")]
    public required string SourceSystemVersion { get; set; }

    [JsonPropertyName("externalReferenceId")]
    public required string ExternalReferenceId { get; set; }

    [JsonPropertyName("eventTimestamp")]
    public required DateTime EventTimestamp { get; set; }

    [JsonPropertyName("patient")]
    public ExternalPatientV1? Patient { get; set; }

    [JsonPropertyName("practitioner")]
    public ExternalPractitionerV1? Practitioner { get; set; }

    [JsonPropertyName("encounter")]
    public ExternalEncounterV1? Encounter { get; set; }
}