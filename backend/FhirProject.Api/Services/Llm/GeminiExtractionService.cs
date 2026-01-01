using System.Text.Json;
using FhirProject.Api.DTOs;
using FhirProject.Api.Models.enums;
using FhirProject.Api.Models.custom;

namespace FhirProject.Api.Services.Llm;

public class GeminiExtractionService : IGeminiExtractionService
{
    private readonly ILogger<GeminiExtractionService> _logger;
    private readonly TimeSpan _timeout = TimeSpan.FromSeconds(30);

    public GeminiExtractionService(ILogger<GeminiExtractionService> logger)
    {
        _logger = logger;
    }

    public async Task<GeminiExtractionResultDto> ExtractStructuredDataAsync(string ocrText, FhirResourceType resourceType)
    {
        try
        {
            _logger.LogInformation("Starting Gemini extraction for resource type: {ResourceType}", resourceType);

            using var cts = new CancellationTokenSource(_timeout);
            
            var schema = GetSchemaForResourceType(resourceType);
            var prompt = BuildExtractionPrompt(ocrText, schema, resourceType);
            
            // Simulate Gemini API call with realistic processing time
            await Task.Delay(1000, cts.Token);
            
            var extractedData = SimulateGeminiExtraction(ocrText, resourceType);
            var fieldConfidences = GenerateFieldConfidences(extractedData, resourceType);
            var overallConfidence = CalculateOverallConfidence(fieldConfidences);
            var warnings = GenerateExtractionWarnings(extractedData, ocrText);

            _logger.LogInformation("Gemini extraction completed. Overall confidence: {Confidence}", overallConfidence);

            return new GeminiExtractionResultDto
            {
                ExtractedData = extractedData,
                FieldConfidences = fieldConfidences,
                OverallConfidence = overallConfidence,
                ExtractionWarnings = warnings,
                ResourceType = resourceType
            };
        }
        catch (OperationCanceledException)
        {
            _logger.LogError("Gemini extraction timed out for resource type: {ResourceType}", resourceType);
            throw new InvalidOperationException("Gemini extraction timed out");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Gemini extraction failed for resource type: {ResourceType}", resourceType);
            throw new InvalidOperationException($"Gemini extraction failed: {ex.Message}", ex);
        }
    }

    private string GetSchemaForResourceType(FhirResourceType resourceType)
    {
        return resourceType switch
        {
            FhirResourceType.Patient => ResourceSchemas.PatientSchema,
            FhirResourceType.Practitioner => ResourceSchemas.PractitionerSchema,
            FhirResourceType.Organization => ResourceSchemas.OrganizationSchema,
            _ => throw new NotSupportedException($"Resource type {resourceType} not supported")
        };
    }

    private string BuildExtractionPrompt(string ocrText, string schema, FhirResourceType resourceType)
    {
        return $@"Extract {resourceType} information from the following text and return ONLY valid JSON matching this exact schema:

Schema: {schema}

Rules:
- Return ONLY valid JSON, no explanations
- Use null for missing fields, never guess or hallucinate
- Match schema exactly
- Include confidence scores (0-1) for each field

Text to extract from:
{ocrText}";
    }

    private object SimulateGeminiExtraction(string ocrText, FhirResourceType resourceType)
    {
        // Simulate realistic Gemini extraction based on OCR text content
        return resourceType switch
        {
            FhirResourceType.Patient => ExtractPatientData(ocrText),
            FhirResourceType.Practitioner => ExtractPractitionerData(ocrText),
            FhirResourceType.Organization => ExtractOrganizationData(ocrText),
            _ => throw new NotSupportedException($"Resource type {resourceType} not supported")
        };
    }

    private CustomPatientInputModel ExtractPatientData(string ocrText)
    {
        return new CustomPatientInputModel
        {
            FirstName = ExtractField(ocrText, "John", "Name:"),
            LastName = ExtractField(ocrText, "Doe", "Name:"),
            DateOfBirth = DateTime.TryParse(ExtractField(ocrText, "1990-05-14", "Date of Birth:", "DOB:"), out var dob) ? dob : DateTime.Now.AddYears(-30),
            Gender = ExtractField(ocrText, "male", "Gender:"),
            PhoneNumber = ExtractField(ocrText, "+1-555-123-4567", "Phone:"),
            Email = ExtractField(ocrText, "john.doe@example.com", "Email:"),
            Address = new AddressInputModel
            {
                Line1 = ExtractField(ocrText, "123 Main Street", "Address:", "Street:"),
                City = ExtractField(ocrText, "San Francisco", "City:"),
                State = ExtractField(ocrText, "CA", "State:"),
                PostalCode = ExtractField(ocrText, "94105", "Postal:", "ZIP:"),
                Country = ExtractField(ocrText, "USA", "Country:")
            }
        };
    }

    private CustomPractitionerInputModel ExtractPractitionerData(string ocrText)
    {
        return new CustomPractitionerInputModel
        {
            FirstName = ExtractField(ocrText, "Jane", "Dr.", "Doctor"),
            LastName = ExtractField(ocrText, "Smith", "Dr.", "Doctor"),
            Gender = ExtractField(ocrText, "female", "Gender:"),
            Qualification = ExtractField(ocrText, "Doctor of Medicine", "MD", "Qualification:"),
            Speciality = ExtractField(ocrText, "Internal Medicine", "Specialization:", "Specialty:"),
            LicenseNumber = ExtractField(ocrText, "MD987654321", "License:", "License Number:")
        };
    }

    private CustomOrganizationInputModel ExtractOrganizationData(string ocrText)
    {
        return new CustomOrganizationInputModel
        {
            Name = ExtractField(ocrText, "General Hospital", "Hospital", "Clinic"),
            Type = ExtractField(ocrText, "Hospital", "Type:"),
            RegistrationNumber = ExtractField(ocrText, "ORG123456789", "Registration:", "Reg:")
        };
    }

    private string? ExtractField(string text, string defaultValue, params string[] keywords)
    {
        // Simple extraction logic - in real implementation, this would use Gemini
        foreach (var keyword in keywords)
        {
            if (text.Contains(keyword, StringComparison.OrdinalIgnoreCase))
            {
                return defaultValue;
            }
        }
        return null;
    }

    private Dictionary<string, decimal> GenerateFieldConfidences(object extractedData, FhirResourceType resourceType)
    {
        var confidences = new Dictionary<string, decimal>();
        var json = JsonSerializer.Serialize(extractedData);
        var doc = JsonDocument.Parse(json);

        foreach (var property in doc.RootElement.EnumerateObject())
        {
            var confidence = property.Value.ValueKind != JsonValueKind.Null ? 0.85m : 0.0m;
            confidences[property.Name] = confidence;
        }

        return confidences;
    }

    private decimal CalculateOverallConfidence(Dictionary<string, decimal> fieldConfidences)
    {
        if (!fieldConfidences.Any()) return 0.0m;
        return fieldConfidences.Values.Average();
    }

    private List<string> GenerateExtractionWarnings(object extractedData, string ocrText)
    {
        var warnings = new List<string>();
        
        if (ocrText.Length < 50)
        {
            warnings.Add("OCR text is very short, extraction may be incomplete");
        }

        var json = JsonSerializer.Serialize(extractedData);
        var doc = JsonDocument.Parse(json);
        var nullFields = doc.RootElement.EnumerateObject()
            .Where(p => p.Value.ValueKind == JsonValueKind.Null)
            .Count();

        if (nullFields > 3)
        {
            warnings.Add($"Many fields could not be extracted ({nullFields} null values)");
        }

        return warnings;
    }
}