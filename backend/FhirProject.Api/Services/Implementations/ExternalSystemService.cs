using FhirProject.Api.DTOs;
using FhirProject.Api.Models.entities;
using FhirProject.Api.Models.enums;
using FhirProject.Api.Repositories.Interfaces;
using FhirProject.Api.Services.Interfaces;
using System.Security.Cryptography;
using System.Text;

namespace FhirProject.Api.Services.Implementations
{
    public class ExternalSystemService : IExternalSystemService
    {
        private readonly IExternalSystemRepository _repository;

        public ExternalSystemService(IExternalSystemRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        public async Task<ExternalSystemRegistrationDto> RegisterExternalSystemAsync(string systemName)
        {
            if (string.IsNullOrWhiteSpace(systemName))
                throw new ArgumentException("System name is required", nameof(systemName));

            var clientId = GenerateClientId();
            var clientSecret = GenerateClientSecret();
            var clientSecretHash = HashSecret(clientSecret);

            var system = new ExternalSystem
            {
                ClientId = clientId,
                ClientSecretHash = clientSecretHash,
                SystemName = systemName,
                Status = ExternalSystemStatus.PendingApproval,
                CreatedAt = DateTime.UtcNow
            };

            await _repository.AddAsync(system);

            return new ExternalSystemRegistrationDto
            {
                SystemId = system.Id,
                ClientId = clientId,
                ClientSecret = clientSecret,
                SystemName = systemName,
                Status = ExternalSystemStatus.PendingApproval.ToString()
            };
        }

        public async Task<bool> ApproveSystemAsync(int systemId, int approvedByUserId)
        {
            var system = await _repository.GetByIdAsync(systemId);
            if (system == null)
                return false;

            system.Status = ExternalSystemStatus.Active;
            system.ApprovedAt = DateTime.UtcNow;
            system.ApprovedByUserId = approvedByUserId;

            await _repository.UpdateAsync(system);
            return true;
        }

        public async Task<bool> SuspendSystemAsync(int systemId)
        {
            var system = await _repository.GetByIdAsync(systemId);
            if (system == null)
                return false;

            system.Status = ExternalSystemStatus.Suspended;
            await _repository.UpdateAsync(system);
            return true;
        }

        public async Task<bool> ActivateSystemAsync(int systemId)
        {
            var system = await _repository.GetByIdAsync(systemId);
            if (system == null)
                return false;

            system.Status = ExternalSystemStatus.Active;
            await _repository.UpdateAsync(system);
            return true;
        }

        public async Task<ExternalSystemDto?> GetSystemByClientIdAsync(string clientId)
        {
            var system = await _repository.GetByClientIdAsync(clientId);
            if (system == null)
                return null;

            return MapToDto(system);
        }

        public async Task<List<ExternalSystemDto>> GetAllSystemsAsync()
        {
            var systems = await _repository.GetAllAsync();
            return systems.Select(MapToDto).ToList();
        }

        public async Task<ExternalSystemDto?> GetSystemByIdAsync(int systemId)
        {
            var system = await _repository.GetByIdAsync(systemId);
            if (system == null)
                return null;

            return MapToDto(system);
        }

        public async Task<bool> DeleteSystemAsync(int systemId)
        {
            var system = await _repository.GetByIdAsync(systemId);
            if (system == null)
                return false;

            await _repository.DeleteAsync(systemId);
            return true;
        }

        private string GenerateClientId()
        {
            return $"ext-{Guid.NewGuid():N}";
        }

        private string GenerateClientSecret()
        {
            var bytes = new byte[32];
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(bytes);
            }
            return Convert.ToBase64String(bytes);
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

        private ExternalSystemDto MapToDto(ExternalSystem system)
        {
            return new ExternalSystemDto
            {
                Id = system.Id,
                ClientId = system.ClientId,
                SystemName = system.SystemName,
                Status = system.Status.ToString(),
                CreatedAt = system.CreatedAt,
                ApprovedAt = system.ApprovedAt,
                ApprovedByUsername = system.ApprovedByUser?.Username,
                LastAccessedAt = system.LastAccessedAt
            };
        }
    }
}
