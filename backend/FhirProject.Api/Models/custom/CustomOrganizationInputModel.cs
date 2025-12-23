namespace FhirProject.Api.Models.custom
{
    public class CustomOrganizationInputModel
    {
        public string Name { get; set; }
        public string Type { get; set; }
        public string RegistrationNumber { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? AddressLine { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? PostalCode { get; set; }
        public string? Country { get; set; }
    }
}