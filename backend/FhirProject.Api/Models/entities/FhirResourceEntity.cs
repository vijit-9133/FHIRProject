namespace FhirProject.Api.Models.entities
{
    public class FhirResourceEntity
{
    public int Id { get; set; }

    public int ConversionRequestId { get; set; }

    // FHIR-compliant JSON output
    public string FhirJson { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
}