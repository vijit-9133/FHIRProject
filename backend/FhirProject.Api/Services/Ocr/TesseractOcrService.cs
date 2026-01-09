using Tesseract;
using PdfiumViewer;

namespace FhirProject.Api.Services.Ocr;

public class TesseractOcrService : IOcrService
{
    private readonly ILogger<TesseractOcrService> _logger;
    private const string TesseractDataPath = @"./bin/Debug/net9.0/tessdata"; // Tesseract data files location

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
            
            string extractedText = fileExtension switch
            {
                ".pdf" => await ExtractTextFromPdfAsync(filePath),
                ".png" or ".jpg" or ".jpeg" => await ExtractTextFromImageAsync(filePath),
                _ => throw new NotSupportedException($"File type {fileExtension} not supported for OCR")
            };

            // Normalize whitespace
            extractedText = NormalizeWhitespace(extractedText);
            
            _logger.LogInformation("OCR extraction completed. Text length: {Length} characters", extractedText.Length);
            _logger.LogDebug("Extracted text: {Text}", extractedText);
            
            return extractedText;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "OCR extraction failed for file: {FilePath}", Path.GetFileName(filePath));
            throw new InvalidOperationException($"OCR extraction failed for {Path.GetFileName(filePath)}: {ex.Message}", ex);
        }
    }

    private async Task<string> ExtractTextFromImageAsync(string imagePath)
    {
        return await Task.Run(() =>
        {
            try
            {
                using var engine = new TesseractEngine(TesseractDataPath, "eng", EngineMode.Default);
                using var img = Pix.LoadFromFile(imagePath);
                using var page = engine.Process(img);
                
                var text = page.GetText();
                _logger.LogInformation("OCR extraction completed for image: {FileName}", Path.GetFileName(imagePath));
                
                return text ?? string.Empty;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to extract text from image {Path.GetFileName(imagePath)}: {ex.Message}", ex);
            }
        });
    }

    private async Task<string> ExtractTextFromPdfAsync(string pdfPath)
    {
        return await Task.Run(() =>
        {
            try
            {
                var combinedText = new List<string>();
                
                using var document = PdfDocument.Load(pdfPath);
                
                for (int pageIndex = 0; pageIndex < document.PageCount; pageIndex++)
                {
                    // Convert PDF page to image
                    using var pageImage = document.Render(pageIndex, 300, 300, PdfRenderFlags.CorrectFromDpi);
                    
                    // Convert to byte array for Tesseract
                    using var memoryStream = new MemoryStream();
                    pageImage.Save(memoryStream, System.Drawing.Imaging.ImageFormat.Png);
                    
                    using var pix = Pix.LoadFromMemory(memoryStream.ToArray());
                    using var engine = new TesseractEngine(TesseractDataPath, "eng", EngineMode.Default);
                    using var page = engine.Process(pix);
                    
                    var pageText = page.GetText();
                    if (!string.IsNullOrWhiteSpace(pageText))
                    {
                        combinedText.Add(pageText);
                    }
                    
                    _logger.LogDebug("Extracted text from PDF page {PageIndex}: {Length} characters", pageIndex + 1, pageText?.Length ?? 0);
                }
                
                var finalText = string.Join("\n\n", combinedText);
                _logger.LogInformation("PDF OCR extraction completed for file: {FileName}, {PageCount} pages processed", 
                    Path.GetFileName(pdfPath), document.PageCount);
                
                return finalText;
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException($"Failed to extract text from PDF {Path.GetFileName(pdfPath)}: {ex.Message}", ex);
            }
        });
    }
    
    private static string NormalizeWhitespace(string text)
    {
        if (string.IsNullOrEmpty(text))
            return string.Empty;
            
        // Replace multiple whitespace characters with single spaces
        // and trim leading/trailing whitespace
        return System.Text.RegularExpressions.Regex.Replace(text.Trim(), @"\s+", " ");
    }
}