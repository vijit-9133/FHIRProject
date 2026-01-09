namespace FhirProject.Api.DTOs
{
    public class LoginResponseDto
    {
        public bool Success { get; set; }
        public int UserId { get; set; }
        public string Role { get; set; }
        public string Token { get; set; }
    }
}