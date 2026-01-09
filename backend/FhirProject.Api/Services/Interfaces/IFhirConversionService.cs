using FhirProject.Api.DTOs;
using FhirProject.Api.Models.entities;

namespace FhirProject.Api.Services.Interfaces
{
    public interface IFhirConversionService
    {
        Task<ConvertToFhirResponseDto> ConvertToFhirAsync(ConvertToFhirRequestDto request);
        Task<ConvertToFhirResponseDto> ConvertToFhirAsync(ConvertToFhirRequestDto request, int? userId);
        Task<FhirResourceEntity?> GetFhirResourceByConversionRequestIdAsync(int conversionRequestId);
        Task<FhirResourceEntity?> GetFhirResourceByConversionRequestIdAsync(int conversionRequestId, int? userId);
        Task<ConversionRequestEntity?> GetConversionRequestByIdAsync(int id);
        Task<ConversionRequestEntity?> GetConversionRequestByIdAsync(int id, int? userId);
        Task<IEnumerable<ConversionRequestEntity>> GetConversionHistoryAsync();
        Task<IEnumerable<ConversionRequestEntity>> GetConversionHistoryAsync(int? userId);
        Task<ConvertToFhirResponseDto> RerunExistingConversionAsync(int conversionRequestId);
        Task<ConvertToFhirResponseDto> RerunExistingConversionAsync(int conversionRequestId, int? userId);
    }
}