namespace FhirProject.Api.DTOs
{
    public class ExternalSystemRegistrationDto
    {
        public int SystemId { get; set; }
        public string ClientId { get; set; }
        public string ClientSecret { get; set; }
        public string SystemName { get; set; }
        public string Status { get; set; }
    }
}
