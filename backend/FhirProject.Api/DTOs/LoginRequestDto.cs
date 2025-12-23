using FhirProject.Api.Models.enums;

namespace FhirProject.Api.DTOs
{
    public class LoginRequestDto
    {
        public string Username { get; set; }
        public UserRole Role { get; set; }
    }
}