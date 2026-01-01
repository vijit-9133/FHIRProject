using FhirProject.Api.DTOs;
using FhirProject.Api.Models.enums;

namespace FhirProject.Api.Services.Llm;

public interface IGeminiExtractionService
{
    Task<GeminiExtractionResultDto> ExtractStructuredDataAsync(string ocrText, FhirResourceType resourceType);
}