using FhirProject.Api.Models.custom;
using FhirProject.Api.Models.enums;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using System.Text.Json;

namespace FhirProject.Api.Mapping
{
    public class OrganizationFhirMapper : IFhirResourceMapper
    {
        public FhirResourceType SupportedResourceType => FhirResourceType.Organization;

        public string MapToFhirJson(object input)
        {
            var options = new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };
            var organizationInput = JsonSerializer.Deserialize<CustomOrganizationInputModel>(input.ToString(), options);
            var fhirOrganization = MapToFhirOrganization(organizationInput);
            var serializer = new FhirJsonSerializer();
            return serializer.SerializeToString(fhirOrganization);
        }

        private Hl7.Fhir.Model.Organization MapToFhirOrganization(CustomOrganizationInputModel input)
        {
            var organization = new Hl7.Fhir.Model.Organization();

            organization.Name = input.Name;

            if (!string.IsNullOrWhiteSpace(input.Type))
            {
                organization.Type.Add(new Hl7.Fhir.Model.CodeableConcept
                {
                    Text = input.Type
                });
            }

            if (!string.IsNullOrWhiteSpace(input.RegistrationNumber))
            {
                organization.Identifier.Add(new Hl7.Fhir.Model.Identifier
                {
                    Use = Hl7.Fhir.Model.Identifier.IdentifierUse.Official,
                    Value = input.RegistrationNumber
                });
            }

            if (!string.IsNullOrWhiteSpace(input.PhoneNumber))
            {
                organization.Telecom.Add(new Hl7.Fhir.Model.ContactPoint
                {
                    System = Hl7.Fhir.Model.ContactPoint.ContactPointSystem.Phone,
                    Value = input.PhoneNumber,
                    Use = Hl7.Fhir.Model.ContactPoint.ContactPointUse.Work
                });
            }

            if (!string.IsNullOrWhiteSpace(input.Email))
            {
                organization.Telecom.Add(new Hl7.Fhir.Model.ContactPoint
                {
                    System = Hl7.Fhir.Model.ContactPoint.ContactPointSystem.Email,
                    Value = input.Email,
                    Use = Hl7.Fhir.Model.ContactPoint.ContactPointUse.Work
                });
            }

            if (!string.IsNullOrWhiteSpace(input.AddressLine))
            {
                organization.Address.Add(new Hl7.Fhir.Model.Address
                {
                    Line = new[] { input.AddressLine },
                    City = input.City,
                    State = input.State,
                    PostalCode = input.PostalCode,
                    Country = input.Country
                });
            }

            return organization;
        }
    }
}