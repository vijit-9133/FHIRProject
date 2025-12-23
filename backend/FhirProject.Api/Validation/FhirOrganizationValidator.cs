using FhirProject.Api.Models.enums;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;

namespace FhirProject.Api.Validation
{
    public class FhirOrganizationValidator : IFhirValidator
    {
        public FhirResourceType SupportedResourceType => FhirResourceType.Organization;

        public void Validate(string fhirJson, FhirResourceType resourceType)
        {
            var errors = new List<string>();

            var deserializer = new FhirJsonDeserializer();
            var organization = deserializer.Deserialize<Organization>(fhirJson);

            if (organization.TypeName != "Organization")
                errors.Add("ResourceType must be Organization");

            if (string.IsNullOrWhiteSpace(organization.Name))
                errors.Add("Organization.name is required");

            if (organization.Identifier == null || !organization.Identifier.Any())
                errors.Add("Organization.identifier (registration number) is required");

            if (errors.Any())
                throw new FhirValidationException(errors);
        }
    }
}