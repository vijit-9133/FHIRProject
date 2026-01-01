using FhirProject.Api.Models.enums;

namespace FhirProject.Api.DTOs;

public class DocumentIngestionRequestDto
{
    public IFormFile File { get; set; } = null!;
    public FhirResourceType ResourceType { get; set; }
}