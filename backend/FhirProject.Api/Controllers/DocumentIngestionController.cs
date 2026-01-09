using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using FhirProject.Api.DTOs;
using FhirProject.Api.Models.enums;
using FhirProject.Api.Services.Ocr;
using FhirProject.Api.Services.Llm;

namespace FhirProject.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class DocumentIngestionController : ControllerBase
{
    private const long MaxFileSizeBytes = 10 * 1024 * 1024; // 10MB
    private readonly string[] AllowedExtensions = { ".pdf", ".png", ".jpg", ".jpeg" };
    private readonly ILogger<DocumentIngestionController> _logger;
    private readonly IOcrService _ocrService;
    private readonly IGeminiExtractionService _geminiService;

    public DocumentIngestionController(
        ILogger<DocumentIngestionController> logger, 
        IOcrService ocrService,
        IGeminiExtractionService geminiService)
    {
        _logger = logger;
        _ocrService = ocrService;
        _geminiService = geminiService;
    }

    [HttpPost("document")]
    public async Task<ActionResult<EnhancedDocumentIngestionResponseDto>> IngestDocument([FromForm] DocumentIngestionRequestDto request)
    {
        // Validate file is provided
        if (request.File == null || request.File.Length == 0)
        {
            return BadRequest("File is required");
        }

        // Validate file size
        if (request.File.Length > MaxFileSizeBytes)
        {
            return BadRequest($"File size exceeds maximum limit of {MaxFileSizeBytes / (1024 * 1024)}MB");
        }

        // Validate file extension
        var fileExtension = Path.GetExtension(request.File.FileName)?.ToLowerInvariant();
        if (string.IsNullOrEmpty(fileExtension) || !AllowedExtensions.Contains(fileExtension))
        {
            return BadRequest($"File type not supported. Allowed types: {string.Join(", ", AllowedExtensions)}");
        }

        // Validate resource type
        if (!Enum.IsDefined(typeof(FhirResourceType), request.ResourceType))
        {
            return BadRequest("Invalid resource type");
        }

        string? tempFilePath = null;
        try
        {
            // Save file to temporary location
            tempFilePath = await SaveFileTemporarilyAsync(request.File);
            
            // Extract text using OCR
            string extractedText;
            try
            {
                extractedText = await _ocrService.ExtractTextAsync(tempFilePath);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "OCR extraction failed for file: {FileName}", request.File.FileName);
                return BadRequest($"OCR extraction failed: {ex.Message}");
            }
            
            // Extract structured data using Gemini
            GeminiExtractionResultDto? geminiResult = null;
            try
            {
                geminiResult = await _geminiService.ExtractStructuredDataAsync(extractedText, request.ResourceType);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Gemini extraction failed for file: {FileName}", request.File.FileName);
                return BadRequest($"Gemini extraction failed: {ex.Message}");
            }
            
            // Process the file with extracted text and Gemini results
            await ProcessFileAsync(tempFilePath, request.ResourceType, extractedText, geminiResult);

            var response = new EnhancedDocumentIngestionResponseDto
            {
                Message = $"Document '{request.File.FileName}' processed successfully for {request.ResourceType}. Extracted {extractedText.Length} characters with {geminiResult.OverallConfidence:P1} confidence.",
                ResourceType = request.ResourceType,
                ExtractedText = extractedText,
                GeminiExtraction = geminiResult
            };

            _logger.LogInformation(
                "Document ingestion response payload: {@Response}",
                response
            );
            _logger.LogInformation("Returning document ingestion response");
            return Ok(response);
        }
        finally
        {
            // Ensure cleanup happens regardless of success or failure
            if (!string.IsNullOrEmpty(tempFilePath))
            {
                CleanupTempFile(tempFilePath);
            }
        }
    }

    private async Task<string> SaveFileTemporarilyAsync(IFormFile file)
    {
        // Generate unique filename to avoid collisions
        var uniqueFileName = $"{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
        var tempFilePath = Path.Combine(Path.GetTempPath(), uniqueFileName);

        // Save file using async stream
        using var fileStream = new FileStream(tempFilePath, FileMode.Create);
        await file.CopyToAsync(fileStream);

        return tempFilePath;
    }

    private async Task ProcessFileAsync(string filePath, FhirResourceType resourceType, string extractedText, GeminiExtractionResultDto geminiResult)
    {
        // Process the extracted text and Gemini structured data
        _logger.LogInformation("Processing file {FilePath} for resource type {ResourceType}. Text: {TextLength} chars, Gemini confidence: {Confidence:P1}", 
            Path.GetFileName(filePath), resourceType, extractedText.Length, geminiResult.OverallConfidence);
        
        // Log extraction warnings if any
        if (geminiResult.ExtractionWarnings.Any())
        {
            _logger.LogWarning("Gemini extraction warnings: {Warnings}", string.Join("; ", geminiResult.ExtractionWarnings));
        }
        
        // Future: This is where structured data would be prepared for human review
        // before potential FHIR conversion
    }

    private void CleanupTempFile(string filePath)
    {
        try
        {
            if (System.IO.File.Exists(filePath))
            {
                System.IO.File.Delete(filePath);
                _logger.LogDebug("Temporary file cleaned up: {FilePath}", filePath);
            }
        }
        catch (Exception ex)
        {
            // Log cleanup failure but don't throw - this shouldn't break the response
            _logger.LogWarning(ex, "Failed to cleanup temporary file: {FilePath}", filePath);
        }
    }
}