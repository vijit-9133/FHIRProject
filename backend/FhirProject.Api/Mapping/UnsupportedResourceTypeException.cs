using FhirProject.Api.Models.enums;

namespace FhirProject.Api.Mapping
{
    public class UnsupportedResourceTypeException : Exception
    {
        public UnsupportedResourceTypeException(FhirResourceType resourceType)
            : base($"No mapper found for resource type: {resourceType}")
        {
        }
    }
}