using System.Text.Json.Serialization;

namespace FhirProject.Api.DTOs.Inbound.V1;

public class ExternalPatientV1
{
    [JsonPropertyName("externalPatientId")]
    public required string ExternalPatientId { get; set; }

    [JsonPropertyName("firstName")]
    public string? FirstName { get; set; }

    [JsonPropertyName("lastName")]
    public string? LastName { get; set; }

    [JsonPropertyName("dateOfBirth")]
    public DateTime? DateOfBirth { get; set; }

    [JsonPropertyName("gender")]
    public string? Gender { get; set; }

    [JsonPropertyName("phoneNumber")]
    public string? PhoneNumber { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("address")]
    public ExternalAddressV1? Address { get; set; }
}