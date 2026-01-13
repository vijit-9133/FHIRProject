using System.Text.Json.Serialization;

namespace FhirProject.Api.DTOs.Inbound.V1;

public class ExternalPractitionerV1
{
    [JsonPropertyName("externalPractitionerId")]
    public required string ExternalPractitionerId { get; set; }

    [JsonPropertyName("firstName")]
    public string? FirstName { get; set; }

    [JsonPropertyName("lastName")]
    public string? LastName { get; set; }

    [JsonPropertyName("qualification")]
    public string? Qualification { get; set; }

    [JsonPropertyName("specialty")]
    public string? Specialty { get; set; }

    [JsonPropertyName("phoneNumber")]
    public string? PhoneNumber { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("address")]
    public ExternalAddressV1? Address { get; set; }
}