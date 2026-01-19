using FhirProject.Api.DTOs;

namespace FhirProject.Api.Services.Interfaces
{
    public interface IExternalSystemService
    {
        Task<ExternalSystemRegistrationDto> RegisterExternalSystemAsync(string systemName);
        Task<bool> ApproveSystemAsync(int systemId, int approvedByUserId);
        Task<bool> SuspendSystemAsync(int systemId);
        Task<bool> ActivateSystemAsync(int systemId);
        Task<ExternalSystemDto?> GetSystemByClientIdAsync(string clientId);
        Task<ExternalSystemDto?> GetSystemByIdAsync(int systemId);
        Task<List<ExternalSystemDto>> GetAllSystemsAsync();
        Task<bool> DeleteSystemAsync(int systemId);
    }
}
