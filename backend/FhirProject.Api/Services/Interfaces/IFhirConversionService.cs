using FhirProject.Api.DTOs;
using FhirProject.Api.Models.entities;

namespace FhirProject.Api.Services.Interfaces
{
    public interface IFhirConversionService
    {
        Task<ConvertToFhirResponseDto> ConvertToFhirAsync(ConvertToFhirRequestDto request);
        Task<FhirResourceEntity?> GetFhirResourceByConversionRequestIdAsync(int conversionRequestId);
        Task<ConversionRequestEntity?> GetConversionRequestByIdAsync(int id);
        Task<IEnumerable<ConversionRequestEntity>> GetConversionHistoryAsync();
        Task<ConvertToFhirResponseDto> RerunExistingConversionAsync(int conversionRequestId);
    }
}