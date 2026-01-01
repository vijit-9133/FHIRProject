using FhirProject.Api.Models.enums;

namespace FhirProject.Api.DTOs;

public class EnhancedDocumentIngestionResponseDto
{
    public string Message { get; set; } = null!;
    public FhirResourceType ResourceType { get; set; }
    public string ExtractedText { get; set; } = null!;
    public GeminiExtractionResultDto? GeminiExtraction { get; set; }
}