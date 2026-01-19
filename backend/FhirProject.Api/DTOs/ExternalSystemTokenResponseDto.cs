namespace FhirProject.Api.DTOs
{
    public class ExternalSystemTokenResponseDto
    {
        public string AccessToken { get; set; }
        public int ExpiresIn { get; set; }
    }
}
