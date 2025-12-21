namespace FhirProject.Api.DTOs
{
    public class ConvertToFhirResponseDto
{
    public int Id { get; set; }
    public bool Success { get; set; }
    public string Message { get; set; }
    public object FhirResource { get; set; }
}
}