using FhirProject.Api.Models.custom;
using FhirProject.Api.Models.enums;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;

namespace FhirProject.Api.Mapping
{
    public class PatientFhirMapper : IFhirResourceMapper
    {
        public FhirResourceType SupportedResourceType => FhirResourceType.Patient;

        public string MapToFhirJson(object input)
        {
            var patientInput = input as CustomPatientInputModel 
                ?? throw new ArgumentException("Expected CustomPatientInputModel", nameof(input));
            var fhirPatient = MapToFhirPatient(patientInput);
            var serializer = new FhirJsonSerializer();
            return serializer.SerializeToString(fhirPatient);
        }

        private Hl7.Fhir.Model.Patient MapToFhirPatient(CustomPatientInputModel input)
        {
            var patient = new Hl7.Fhir.Model.Patient();

            patient.Name.Add(new Hl7.Fhir.Model.HumanName
            {
                Use = Hl7.Fhir.Model.HumanName.NameUse.Official,
                Family = input.LastName,
                Given = new[] { input.FirstName }
            });

            if (!string.IsNullOrWhiteSpace(input.Gender))
            {
                patient.Gender = input.Gender.ToLower() switch
                {
                    "male" => Hl7.Fhir.Model.AdministrativeGender.Male,
                    "female" => Hl7.Fhir.Model.AdministrativeGender.Female,
                    "other" => Hl7.Fhir.Model.AdministrativeGender.Other,
                    _ => Hl7.Fhir.Model.AdministrativeGender.Unknown
                };
            }

            patient.BirthDate = input.DateOfBirth.ToString("yyyy-MM-dd");

            if (!string.IsNullOrWhiteSpace(input.PhoneNumber))
            {
                patient.Telecom.Add(new Hl7.Fhir.Model.ContactPoint
                {
                    System = Hl7.Fhir.Model.ContactPoint.ContactPointSystem.Phone,
                    Value = input.PhoneNumber,
                    Use = Hl7.Fhir.Model.ContactPoint.ContactPointUse.Home
                });
            }

            if (!string.IsNullOrWhiteSpace(input.Email))
            {
                patient.Telecom.Add(new Hl7.Fhir.Model.ContactPoint
                {
                    System = Hl7.Fhir.Model.ContactPoint.ContactPointSystem.Email,
                    Value = input.Email,
                    Use = Hl7.Fhir.Model.ContactPoint.ContactPointUse.Home
                });
            }

            if (input.Address != null)
            {
                patient.Address.Add(new Hl7.Fhir.Model.Address
                {
                    Line = new[] { input.Address.Line1 },
                    City = input.Address.City,
                    State = input.Address.State,
                    PostalCode = input.Address.PostalCode,
                    Country = input.Address.Country
                });
            }

            return patient;
        }
    }
}