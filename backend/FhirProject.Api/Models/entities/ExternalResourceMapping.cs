using FhirProject.Api.Models.enums;

namespace FhirProject.Api.Models.entities;

public class ExternalResourceMapping
{
    public Guid Id { get; set; }
    public required string SourceSystem { get; set; }
    public required string ExternalId { get; set; }
    public FhirResourceType ResourceType { get; set; }
    public required string InternalResourceId { get; set; }
    public DateTime CreatedAt { get; set; }
}