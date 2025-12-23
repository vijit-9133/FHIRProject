using FhirProject.Api.Models.custom;
using FhirProject.Api.Models.enums;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using System.Text.Json;

namespace FhirProject.Api.Mapping
{
    public class PractitionerFhirMapper : IFhirResourceMapper
    {
        public FhirResourceType SupportedResourceType => FhirResourceType.Practitioner;

        public string MapToFhirJson(object input)
        {
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var practitionerInput = JsonSerializer.Deserialize<CustomPractitionerInputModel>(input.ToString(), options);
            var fhirPractitioner = MapToFhirPractitioner(practitionerInput);
            var serializer = new FhirJsonSerializer();
            return serializer.SerializeToString(fhirPractitioner);
        }

        private Hl7.Fhir.Model.Practitioner MapToFhirPractitioner(CustomPractitionerInputModel input)
        {
            var practitioner = new Hl7.Fhir.Model.Practitioner();

            practitioner.Name.Add(new Hl7.Fhir.Model.HumanName
            {
                Use = Hl7.Fhir.Model.HumanName.NameUse.Official,
                Family = input.LastName,
                Given = new[] { input.FirstName }
            });

            if (!string.IsNullOrWhiteSpace(input.Gender))
            {
                practitioner.Gender = input.Gender.ToLower() switch
                {
                    "male" => Hl7.Fhir.Model.AdministrativeGender.Male,
                    "female" => Hl7.Fhir.Model.AdministrativeGender.Female,
                    "other" => Hl7.Fhir.Model.AdministrativeGender.Other,
                    _ => Hl7.Fhir.Model.AdministrativeGender.Unknown
                };
            }

            if (!string.IsNullOrWhiteSpace(input.LicenseNumber))
            {
                practitioner.Identifier.Add(new Hl7.Fhir.Model.Identifier
                {
                    Use = Hl7.Fhir.Model.Identifier.IdentifierUse.Official,
                    System = "http://hl7.org/fhir/sid/us-npi",
                    Value = input.LicenseNumber
                });
            }

            if (!string.IsNullOrWhiteSpace(input.PhoneNumber))
            {
                practitioner.Telecom.Add(new Hl7.Fhir.Model.ContactPoint
                {
                    System = Hl7.Fhir.Model.ContactPoint.ContactPointSystem.Phone,
                    Value = input.PhoneNumber,
                    Use = Hl7.Fhir.Model.ContactPoint.ContactPointUse.Work
                });
            }

            if (!string.IsNullOrWhiteSpace(input.Email))
            {
                practitioner.Telecom.Add(new Hl7.Fhir.Model.ContactPoint
                {
                    System = Hl7.Fhir.Model.ContactPoint.ContactPointSystem.Email,
                    Value = input.Email,
                    Use = Hl7.Fhir.Model.ContactPoint.ContactPointUse.Work
                });
            }

            if (!string.IsNullOrWhiteSpace(input.Qualification))
            {
                practitioner.Qualification.Add(new Hl7.Fhir.Model.Practitioner.QualificationComponent
                {
                    Code = new Hl7.Fhir.Model.CodeableConcept
                    {
                        Text = input.Qualification
                    }
                });
            }

            return practitioner;
        }
    }
}