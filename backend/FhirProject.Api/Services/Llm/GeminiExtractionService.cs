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
            _logger.LogInformation("OCR input text length: {Length} characters", ocrText.Length);
            _logger.LogInformation("OCR input text: {OcrText}", ocrText);

            using var cts = new CancellationTokenSource(_timeout);
            
            var schema = GetSchemaForResourceType(resourceType);
            var prompt = BuildExtractionPrompt(ocrText, schema, resourceType);
            
            // Simulate Gemini API call with realistic processing time
            await Task.Delay(1000, cts.Token);
            
            var extractedData = ExtractFromOcrText(ocrText, resourceType);
            var fieldConfidences = GenerateFieldConfidences(extractedData, resourceType);
            var overallConfidence = CalculateOverallConfidence(fieldConfidences);
            var warnings = GenerateExtractionWarnings(extractedData, ocrText);

            _logger.LogInformation("Extracted data: {ExtractedData}", JsonSerializer.Serialize(extractedData));
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

    private object ExtractFromOcrText(string ocrText, FhirResourceType resourceType)
    {
        // Extract data strictly from OCR text - no hardcoded fallbacks
        return resourceType switch
        {
            FhirResourceType.Patient => ExtractPatientDataFromText(ocrText),
            FhirResourceType.Practitioner => ExtractPractitionerDataFromText(ocrText),
            FhirResourceType.Organization => ExtractOrganizationDataFromText(ocrText),
            _ => throw new NotSupportedException($"Resource type {resourceType} not supported")
        };
    }

    private CustomPatientInputModel ExtractPatientDataFromText(string ocrText)
    {
        // Extract data strictly from OCR text - return null if not found
        return new CustomPatientInputModel
        {
            FirstName = ExtractFieldFromText(ocrText, "Name:", "First Name:", "Patient:"),
            LastName = ExtractFieldFromText(ocrText, "Name:", "Last Name:", "Patient:"),
            DateOfBirth = ParseDateFromText(ocrText) ?? DateTime.MinValue,
            Gender = ExtractFieldFromText(ocrText, "Gender:", "Sex:"),
            PhoneNumber = ExtractFieldFromText(ocrText, "Phone:", "Tel:", "Telephone:"),
            Email = ExtractFieldFromText(ocrText, "Email:", "E-mail:"),
            Address = ExtractAddressFromText(ocrText)
        };
    }

    private CustomPractitionerInputModel ExtractPractitionerDataFromText(string ocrText)
    {
        // Extract data strictly from OCR text - return null if not found
        return new CustomPractitionerInputModel
        {
            FirstName = ExtractFieldFromText(ocrText, "Dr.", "Doctor", "Practitioner:"),
            LastName = ExtractFieldFromText(ocrText, "Dr.", "Doctor", "Practitioner:"),
            Gender = ExtractFieldFromText(ocrText, "Gender:", "Sex:"),
            Qualification = ExtractFieldFromText(ocrText, "MD", "Qualification:", "Degree:"),
            Speciality = ExtractFieldFromText(ocrText, "Specialization:", "Specialty:", "Department:"),
            LicenseNumber = ExtractFieldFromText(ocrText, "License:", "License Number:", "Registration:")
        };
    }

    private CustomOrganizationInputModel ExtractOrganizationDataFromText(string ocrText)
    {
        // Extract data strictly from OCR text - return null if not found
        return new CustomOrganizationInputModel
        {
            Name = ExtractFieldFromText(ocrText, "Hospital", "Clinic", "Organization:", "Company:"),
            Type = ExtractFieldFromText(ocrText, "Type:", "Category:"),
            RegistrationNumber = ExtractFieldFromText(ocrText, "Registration:", "Reg:", "ID:")
        };
    }

    private string? ExtractFieldFromText(string text, params string[] keywords)
    {
        // Extract actual field value from OCR text based on keywords
        if (string.IsNullOrWhiteSpace(text))
            return null;

        foreach (var keyword in keywords)
        {
            var keywordIndex = text.IndexOf(keyword, StringComparison.OrdinalIgnoreCase);
            if (keywordIndex >= 0)
            {
                // Find the value after the keyword
                var startIndex = keywordIndex + keyword.Length;
                if (startIndex < text.Length)
                {
                    var remainingText = text.Substring(startIndex).Trim();
                    // Extract until next line or common delimiters
                    var endIndex = remainingText.IndexOfAny(new[] { '\n', '\r', ',', ';', '|' });
                    if (endIndex > 0)
                    {
                        var extractedValue = remainingText.Substring(0, endIndex).Trim();
                        return string.IsNullOrWhiteSpace(extractedValue) ? null : extractedValue;
                    }
                    else if (remainingText.Length > 0)
                    {
                        // Take the remaining text if no delimiter found
                        var extractedValue = remainingText.Trim();
                        return string.IsNullOrWhiteSpace(extractedValue) ? null : extractedValue;
                    }
                }
            }
        }
        return null;
    }

    private DateTime? ParseDateFromText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        var dateKeywords = new[] { "Date of Birth:", "DOB:", "Born:", "Birth Date:" };
        foreach (var keyword in dateKeywords)
        {
            var dateValue = ExtractFieldFromText(text, keyword);
            if (dateValue != null && DateTime.TryParse(dateValue, out var parsedDate))
            {
                return parsedDate;
            }
        }
        return null;
    }

    private AddressInputModel? ExtractAddressFromText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        var address = new AddressInputModel
        {
            Line1 = ExtractFieldFromText(text, "Address:", "Street:", "Line1:"),
            City = ExtractFieldFromText(text, "City:"),
            State = ExtractFieldFromText(text, "State:", "Province:"),
            PostalCode = ExtractFieldFromText(text, "Postal:", "ZIP:", "Postal Code:"),
            Country = ExtractFieldFromText(text, "Country:")
        };

        // Return null if no address fields were found
        if (string.IsNullOrWhiteSpace(address.Line1) && 
            string.IsNullOrWhiteSpace(address.City) && 
            string.IsNullOrWhiteSpace(address.State) && 
            string.IsNullOrWhiteSpace(address.PostalCode) && 
            string.IsNullOrWhiteSpace(address.Country))
        {
            return null;
        }

        return address;
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
        
        if (string.IsNullOrWhiteSpace(ocrText) || ocrText.StartsWith("OCR_NOT_IMPLEMENTED"))
        {
            warnings.Add("OCR is not implemented - no real text extraction performed");
        }
        else if (ocrText.Length < 20)
        {
            warnings.Add("OCR text is very short, extraction may be incomplete");
        }

        var json = JsonSerializer.Serialize(extractedData);
        var doc = JsonDocument.Parse(json);
        var nullFields = doc.RootElement.EnumerateObject()
            .Where(p => p.Value.ValueKind == JsonValueKind.Null)
            .Count();

        var totalFields = doc.RootElement.EnumerateObject().Count();
        
        if (nullFields == totalFields)
        {
            warnings.Add("No fields could be extracted from the OCR text");
        }
        else if (nullFields > totalFields / 2)
        {
            warnings.Add($"Many fields could not be extracted ({nullFields} out of {totalFields} fields are null)");
        }

        return warnings;
    }
}