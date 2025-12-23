using FhirProject.Api.Models.enums;

namespace FhirProject.Api.Models.entities
{
    public class UserEntity
    {
        public int Id { get; set; }
        public string Username { get; set; }
        public UserRole Role { get; set; }
    }
}