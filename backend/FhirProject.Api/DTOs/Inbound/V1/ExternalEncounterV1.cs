using System.Text.Json.Serialization;

namespace FhirProject.Api.DTOs.Inbound.V1;

public class ExternalEncounterV1
{
    [JsonPropertyName("externalEncounterId")]
    public required string ExternalEncounterId { get; set; }

    [JsonPropertyName("encounterType")]
    public string? EncounterType { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("startDateTime")]
    public DateTime? StartDateTime { get; set; }

    [JsonPropertyName("endDateTime")]
    public DateTime? EndDateTime { get; set; }

    [JsonPropertyName("reasonCode")]
    public string? ReasonCode { get; set; }

    [JsonPropertyName("reasonDisplay")]
    public string? ReasonDisplay { get; set; }

    [JsonPropertyName("location")]
    public string? Location { get; set; }
}