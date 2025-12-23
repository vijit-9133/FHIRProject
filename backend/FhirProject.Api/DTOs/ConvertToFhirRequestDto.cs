using FhirProject.Api.Models.enums;

namespace FhirProject.Api.DTOs
{
    public class ConvertToFhirRequestDto
    {
        public FhirResourceType ResourceType { get; set; }
        public dynamic Data { get; set; }
    }
}