using FhirProject.Api.DTOs;

namespace FhirProject.Api.Services.Interfaces
{
    public interface IExternalSystemTokenService
    {
        Task<ExternalSystemTokenResponseDto?> AuthenticateAndGenerateTokenAsync(string clientId, string clientSecret);
    }
}
