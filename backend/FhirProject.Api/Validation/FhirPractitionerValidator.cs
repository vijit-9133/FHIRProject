using FhirProject.Api.Models.enums;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;

namespace FhirProject.Api.Validation
{
    public class FhirPractitionerValidator : IFhirValidator
    {
        public FhirResourceType SupportedResourceType => FhirResourceType.Practitioner;

        public void Validate(string fhirJson, FhirResourceType resourceType)
        {
            var errors = new List<string>();

            var deserializer = new FhirJsonDeserializer();
            var practitioner = deserializer.Deserialize<Practitioner>(fhirJson);

            if (practitioner.TypeName != "Practitioner")
                errors.Add("ResourceType must be Practitioner");

            if (practitioner.Name == null || !practitioner.Name.Any())
                errors.Add("Practitioner.name is required");

            if (practitioner.Gender == null)
                errors.Add("Practitioner.gender is required");

            if (practitioner.Identifier == null || !practitioner.Identifier.Any())
                errors.Add("Practitioner.identifier (license number) is required");

            if (errors.Any())
                throw new FhirValidationException(errors);
        }
    }
}