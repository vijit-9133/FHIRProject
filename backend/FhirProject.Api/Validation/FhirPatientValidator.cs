using FhirProject.Api.Models.enums;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;

namespace FhirProject.Api.Validation
{
    public class FhirPatientValidator : IFhirValidator
    {
        public FhirResourceType SupportedResourceType => FhirResourceType.Patient;

        public void Validate(string fhirJson, FhirResourceType resourceType)
        {
            var errors = new List<string>();

            var deserializer = new FhirJsonDeserializer();
            var patient = deserializer.Deserialize<Patient>(fhirJson);

            if (patient.TypeName != "Patient")
                errors.Add("ResourceType must be Patient");

            if (patient.Name == null || !patient.Name.Any())
                errors.Add("Patient.name is required");

            if (patient.Gender == null)
                errors.Add("Patient.gender is required");

            if (string.IsNullOrWhiteSpace(patient.BirthDate))
                errors.Add("Patient.birthDate is required");

            if (errors.Any())
                throw new FhirValidationException(errors);
        }
    }
}