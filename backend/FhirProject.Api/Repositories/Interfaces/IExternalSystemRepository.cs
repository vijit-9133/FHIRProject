using FhirProject.Api.Models.entities;

namespace FhirProject.Api.Repositories.Interfaces
{
    public interface IExternalSystemRepository
    {
        Task<ExternalSystem?> GetByClientIdAsync(string clientId);
        Task<ExternalSystem?> GetByIdAsync(int id);
        Task AddAsync(ExternalSystem system);
        Task UpdateAsync(ExternalSystem system);
        Task<List<ExternalSystem>> GetAllAsync();
        Task DeleteAsync(int id);
    }
}
