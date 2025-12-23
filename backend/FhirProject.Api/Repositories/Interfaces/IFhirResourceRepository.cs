using FhirProject.Api.Models.entities;

namespace FhirProject.Api.Repositories.Interfaces
{
    public interface IFhirResourceRepository
    {
        Task<FhirResourceEntity> SaveAsync(FhirResourceEntity entity);
        Task<FhirResourceEntity?> GetByConversionRequestIdAsync(int conversionRequestId);
        Task<FhirResourceEntity?> GetByIdAsync(int id);
        Task<IEnumerable<FhirResourceEntity>> GetAllAsync();
        Task DeleteAsync(int id);
    }
}