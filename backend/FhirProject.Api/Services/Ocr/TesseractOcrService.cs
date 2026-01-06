namespace FhirProject.Api.Services.Ocr;

public class TesseractOcrService : IOcrService
{
    private readonly ILogger<TesseractOcrService> _logger;

    public TesseractOcrService(ILogger<TesseractOcrService> logger)
    {
        _logger = logger;
    }

    public async Task<string> ExtractTextAsync(string filePath)
    {
        try
        {
            _logger.LogInformation("Starting OCR extraction for file: {FilePath}", Path.GetFileName(filePath));

            var fileExtension = Path.GetExtension(filePath).ToLowerInvariant();
            
            if (fileExtension == ".pdf")
            {
                return await ExtractTextFromPdfAsync(filePath);
            }
            else
            {
                return await ExtractTextFromImageAsync(filePath);
            }
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
        _logger.LogInformation("OCR extraction completed for image: {FileName}", fileName);
        return $"OCR_NOT_IMPLEMENTED_FOR_{fileName}";
    }

    private async Task<string> ExtractTextFromPdfAsync(string pdfPath)
    {
        await Task.Delay(400);
        var fileName = Path.GetFileName(pdfPath);
        _logger.LogWarning("PDF OCR not yet implemented for file: {FileName}", fileName);
        return $"OCR_NOT_IMPLEMENTED_FOR_{fileName}";
    }
}