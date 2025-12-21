using FhirProject.Api.Models.enums;

namespace FhirProject.Api.Mapping
{
    public interface IFhirResourceMapper
    {
        FhirResourceType SupportedResourceType { get; }
        string MapToFhirJson(object input);
    }
}