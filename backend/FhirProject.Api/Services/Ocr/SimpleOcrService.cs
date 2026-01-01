namespace FhirProject.Api.Services.Ocr;

public class SimpleOcrService : IOcrService
{
    private readonly ILogger<SimpleOcrService> _logger;

    public SimpleOcrService(ILogger<SimpleOcrService> logger)
    {
        _logger = logger;
    }

    public async Task<string> ExtractTextAsync(string filePath)
    {
        try
        {
            _logger.LogInformation("Starting OCR extraction for file: {FilePath}", Path.GetFileName(filePath));

            var fileExtension = Path.GetExtension(filePath).ToLowerInvariant();
            
            // Simulate OCR processing time
            await Task.Delay(500);

            var extractedText = fileExtension switch
            {
                ".pdf" => await ExtractTextFromPdfAsync(filePath),
                ".png" or ".jpg" or ".jpeg" => await ExtractTextFromImageAsync(filePath),
                _ => throw new NotSupportedException($"File type {fileExtension} not supported for OCR")
            };

            _logger.LogInformation("OCR extraction completed. Text length: {Length} characters", extractedText.Length);
            return extractedText;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OCR extraction failed for file: {FilePath}", Path.GetFileName(filePath));
            throw new InvalidOperationException($"OCR extraction failed: {ex.Message}", ex);
        }
    }

    private async Task<string> ExtractTextFromImageAsync(string imagePath)
    {
        await Task.Delay(300);
        
        // Simulate extracted text from medical document image
        return @"PATIENT INFORMATION
Name: John Doe
Date of Birth: 05/14/1990
Gender: Male
Phone: +1-555-123-4567
Email: john.doe@example.com

ADDRESS:
123 Main Street
San Francisco, CA 94105
USA

MEDICAL RECORD NUMBER: MR123456789
VISIT DATE: 12/30/2024";
    }

    private async Task<string> ExtractTextFromPdfAsync(string pdfPath)
    {
        await Task.Delay(400);
        
        // Simulate extracted text from medical PDF
        return @"MEDICAL REPORT

PRACTITIONER INFORMATION
Dr. Jane Smith, MD
Specialization: Internal Medicine
License Number: MD987654321
Qualification: Doctor of Medicine

PATIENT: John Doe
DOB: 1990-05-14
VISIT: 2024-12-30

DIAGNOSIS: Routine checkup
NOTES: Patient in good health";
    }
}