using FhirProject.Api.Models.enums;

namespace FhirProject.Api.Validation
{
    public interface IFhirValidator
    {
        FhirResourceType SupportedResourceType { get; }
        void Validate(string fhirJson, FhirResourceType resourceType);
    }
}