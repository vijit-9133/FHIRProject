using FhirProject.Api.Models.enums;

namespace FhirProject.Api.DTOs;

public class GeminiExtractionResultDto
{
    public object? ExtractedData { get; set; }
    public Dictionary<string, decimal> FieldConfidences { get; set; } = new();
    public decimal OverallConfidence { get; set; }
    public List<string> ExtractionWarnings { get; set; } = new();
    public FhirResourceType ResourceType { get; set; }
}