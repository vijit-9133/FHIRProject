using FhirProject.Api.DTOs;
using FhirProject.Api.Models.enums;
using FhirProject.Api.Repositories.Interfaces;
using FhirProject.Api.Services.Interfaces;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace FhirProject.Api.Services.Implementations
{
    public class ExternalSystemTokenService : IExternalSystemTokenService
    {
        private readonly IExternalSystemRepository _repository;
        private readonly IConfiguration _configuration;

        public ExternalSystemTokenService(
            IExternalSystemRepository repository,
            IConfiguration configuration)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
            _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
        }

        public async Task<ExternalSystemTokenResponseDto?> AuthenticateAndGenerateTokenAsync(string clientId, string clientSecret)
        {
            if (string.IsNullOrWhiteSpace(clientId) || string.IsNullOrWhiteSpace(clientSecret))
                return null;

            var system = await _repository.GetByClientIdAsync(clientId);
            if (system == null)
                return null;

            if (system.Status != ExternalSystemStatus.Active)
                return null;

            var providedSecretHash = HashSecret(clientSecret);
            if (providedSecretHash != system.ClientSecretHash)
                return null;

            system.LastAccessedAt = DateTime.UtcNow;
            await _repository.UpdateAsync(system);

            var token = GenerateToken(system);
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"]);

            return new ExternalSystemTokenResponseDto
            {
                AccessToken = token,
                ExpiresIn = expirationMinutes * 60
            };
        }

        private string GenerateToken(Models.entities.ExternalSystem system)
        {
            var jwtSettings = _configuration.GetSection("JwtSettings");
            var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]);
            var issuer = jwtSettings["Issuer"];
            var audience = jwtSettings["Audience"];
            var expirationMinutes = int.Parse(jwtSettings["ExpirationMinutes"]);

            var claims = new[]
            {
                new Claim("ClientType", "External"),
                new Claim("ClientId", system.ClientId),
                new Claim("SystemId", system.Id.ToString()),
                new Claim("SourceSystem", system.SystemName),
                new Claim(ClaimTypes.Role, "ExternalSystem")
            };

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddMinutes(expirationMinutes),
                Issuer = issuer,
                Audience = audience,
                SigningCredentials = new SigningCredentials(new SymmetricSecurityKey(secretKey), SecurityAlgorithms.HmacSha256)
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(token);
        }

        private string HashSecret(string secret)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.UTF8.GetBytes(secret);
                var hash = sha256.ComputeHash(bytes);
                return Convert.ToBase64String(hash);
            }
        }
    }
}
