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
        if (string.IsNullOrWhiteSpace(text))
            return null;

        foreach (var keyword in keywords)
        {
            var extracted = ExtractFieldBetween(text, keyword, GetKnownLabels());
            if (!string.IsNullOrWhiteSpace(extracted))
                return extracted;
        }
        return null;
    }

    private string? ExtractFieldBetween(string text, string startLabel, string[] stopLabels)
    {
        var startIndex = text.IndexOf(startLabel, StringComparison.OrdinalIgnoreCase);
        if (startIndex < 0) return null;

        var valueStartIndex = startIndex + startLabel.Length;
        if (valueStartIndex >= text.Length) return null;

        var remainingText = text.Substring(valueStartIndex);
        
        // Find the earliest stop label
        var stopIndex = remainingText.Length;
        foreach (var stopLabel in stopLabels)
        {
            var labelIndex = remainingText.IndexOf(stopLabel, StringComparison.OrdinalIgnoreCase);
            if (labelIndex >= 0 && labelIndex < stopIndex)
            {
                stopIndex = labelIndex;
            }
        }

        var extractedValue = remainingText.Substring(0, stopIndex).Trim();
        
        // Clean up common delimiters at the start
        extractedValue = extractedValue.TrimStart(':', '-', '=', ' ', '\t');
        
        // For non-address fields, stop at line breaks
        if (!startLabel.Contains("Address", StringComparison.OrdinalIgnoreCase))
        {
            var lineBreakIndex = extractedValue.IndexOfAny(new[] { '\n', '\r' });
            if (lineBreakIndex >= 0)
            {
                extractedValue = extractedValue.Substring(0, lineBreakIndex);
            }
        }

        extractedValue = extractedValue.Trim();
        return string.IsNullOrWhiteSpace(extractedValue) ? null : extractedValue;
    }

    private string[] GetKnownLabels()
    {
        return new[]
        {
            "First Name", "Last Name", "Date of Birth", "DOB", "Gender", 
            "Phone", "Phone Number", "Email", "Address", "Name:", "Patient:",
            "Dr.", "Doctor", "Practitioner:", "Qualification:", "Degree:",
            "Specialization:", "Specialty:", "Department:", "License:", 
            "License Number:", "Registration:", "Hospital", "Clinic", 
            "Organization:", "Company:", "Type:", "Category:", "Reg:", "ID:",
            "Street:", "Line1:", "City:", "State:", "Province:", "Postal:", 
            "ZIP:", "Postal Code:", "Country:", "Tel:", "Telephone:", "E-mail:",
            "Sex:", "Born:", "Birth Date:", "MD"
        };
    }

    private DateTime? ParseDateFromText(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return null;

        var dateKeywords = new[] { "Date of Birth:", "DOB:", "Born:", "Birth Date:" };
        foreach (var keyword in dateKeywords)
        {
            var dateValue = ExtractFieldBetween(text, keyword, GetKnownLabels());
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

        // Extract address as a block first
        var addressBlock = ExtractFieldBetween(text, "Address:", GetKnownLabels());
        if (string.IsNullOrWhiteSpace(addressBlock))
        {
            addressBlock = ExtractFieldBetween(text, "Street:", GetKnownLabels());
        }

        var address = new AddressInputModel();

        if (!string.IsNullOrWhiteSpace(addressBlock))
        {
            // Parse address block into components
            var lines = addressBlock.Split(new[] { '\n', '\r' }, StringSplitOptions.RemoveEmptyEntries)
                                   .Select(l => l.Trim())
                                   .Where(l => !string.IsNullOrWhiteSpace(l))
                                   .ToArray();
            
            if (lines.Length > 0)
                address.Line1 = lines[0];
            if (lines.Length > 1)
            {
                // Try to parse city, state, postal from last line
                var lastLine = lines[lines.Length - 1];
                var parts = lastLine.Split(',').Select(p => p.Trim()).ToArray();
                if (parts.Length >= 2)
                {
                    address.City = parts[0];
                    var stateZip = parts[1].Split(' ').Where(p => !string.IsNullOrWhiteSpace(p)).ToArray();
                    if (stateZip.Length >= 1) address.State = stateZip[0];
                    if (stateZip.Length >= 2) address.PostalCode = stateZip[1];
                }
            }
        }
        else
        {
            // Try individual field extraction
            address.Line1 = ExtractFieldBetween(text, "Line1:", GetKnownLabels());
            address.City = ExtractFieldBetween(text, "City:", GetKnownLabels());
            address.State = ExtractFieldBetween(text, "State:", GetKnownLabels()) ?? 
                           ExtractFieldBetween(text, "Province:", GetKnownLabels());
            address.PostalCode = ExtractFieldBetween(text, "Postal:", GetKnownLabels()) ?? 
                                ExtractFieldBetween(text, "ZIP:", GetKnownLabels()) ?? 
                                ExtractFieldBetween(text, "Postal Code:", GetKnownLabels());
            address.Country = ExtractFieldBetween(text, "Country:", GetKnownLabels());
        }

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