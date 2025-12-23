namespace FhirProject.Api.Models.custom
{
    public class CustomPractitionerInputModel
    {
        public string FirstName { get; set; }
        public string LastName { get; set; }
        public string Gender { get; set; }
        public string Qualification { get; set; }
        public string Speciality { get; set; }
        public string LicenseNumber { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? OrganizationName { get; set; }
    }
}