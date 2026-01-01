namespace FhirProject.Api.Services.Ocr;

public interface IOcrService
{
    Task<string> ExtractTextAsync(string filePath);
}