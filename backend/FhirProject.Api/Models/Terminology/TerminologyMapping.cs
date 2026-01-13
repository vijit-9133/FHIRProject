namespace FhirProject.Api.Models.Terminology;

public class TerminologyMapping
{
    public required string OriginalText { get; set; }
    public required string MappedCode { get; set; }
    public required string CodeSystem { get; set; }
    public string? Display { get; set; }
}