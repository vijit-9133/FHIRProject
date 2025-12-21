using FhirProject.Api.Models.entities;

namespace FhirProject.Api.Repositories.Interfaces
{
    public interface IConversionRequestRepository
    {
        Task<ConversionRequestEntity> CreateAsync(ConversionRequestEntity entity);
        Task<ConversionRequestEntity?> GetByIdAsync(int id);
        Task<IEnumerable<ConversionRequestEntity>> GetAllAsync();
        Task<IEnumerable<ConversionRequestEntity>> GetByResourceTypeAsync(string resourceType);
        Task<ConversionRequestEntity> UpdateAsync(ConversionRequestEntity entity);
    }
}