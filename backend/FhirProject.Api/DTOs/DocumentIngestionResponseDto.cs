using FhirProject.Api.Models.enums;

namespace FhirProject.Api.DTOs;

public class DocumentIngestionResponseDto
{
    public string Message { get; set; } = null!;
    public FhirResourceType ResourceType { get; set; }
}