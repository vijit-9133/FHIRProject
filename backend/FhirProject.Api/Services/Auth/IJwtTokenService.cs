using FhirProject.Api.Models.entities;

namespace FhirProject.Api.Services.Auth
{
    public interface IJwtTokenService
    {
        string GenerateToken(UserEntity user);
    }
}