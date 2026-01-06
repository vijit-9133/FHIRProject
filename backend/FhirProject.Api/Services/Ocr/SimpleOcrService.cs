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
        
        var fileName = Path.GetFileName(imagePath);
        _logger.LogInformation("OCR extraction for image file: {FileName}", fileName);
        
        // Return file-specific placeholder since real OCR is not implemented
        return $"OCR_NOT_IMPLEMENTED_FOR_{fileName}";
    }

    private async Task<string> ExtractTextFromPdfAsync(string pdfPath)
    {
        await Task.Delay(400);
        
        var fileName = Path.GetFileName(pdfPath);
        _logger.LogInformation("OCR extraction for PDF file: {FileName}", fileName);
        
        // Return file-specific placeholder since real OCR is not implemented
        return $"OCR_NOT_IMPLEMENTED_FOR_{fileName}";
    }
}