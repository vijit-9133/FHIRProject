using FhirProject.Api.DTOs;
using FhirProject.Api.Mapping;
using FhirProject.Api.Models.entities;
using FhirProject.Api.Models.enums;
using FhirProject.Api.Repositories.Interfaces;
using FhirProject.Api.Services.Interfaces;
using FhirProject.Api.Validation;
using System.Text.Json;

namespace FhirProject.Api.Services.Implementations
{
    public class FhirConversionService : IFhirConversionService
    {
        private readonly IConversionRequestRepository _conversionRequestRepository;
        private readonly IFhirResourceRepository _fhirResourceRepository;
        private readonly IEnumerable<IFhirResourceMapper> _mappers;
        private readonly IEnumerable<IFhirValidator> _validators;

        public FhirConversionService(
            IConversionRequestRepository conversionRequestRepository,
            IFhirResourceRepository fhirResourceRepository,
            IEnumerable<IFhirResourceMapper> mappers,
            IEnumerable<IFhirValidator> validators)
        {
            _conversionRequestRepository = conversionRequestRepository ?? throw new ArgumentNullException(nameof(conversionRequestRepository));
            _fhirResourceRepository = fhirResourceRepository ?? throw new ArgumentNullException(nameof(fhirResourceRepository));
            _mappers = mappers ?? throw new ArgumentNullException(nameof(mappers));
            _validators = validators ?? throw new ArgumentNullException(nameof(validators));
        }

        public async Task<ConvertToFhirResponseDto> ConvertToFhirAsync(ConvertToFhirRequestDto request)
        {
            if (request?.Data == null)
            {
                return new ConvertToFhirResponseDto
                {
                    Success = false,
                    Message = "Invalid request data"
                };
            }

            // Validate input
            var validationResult = ValidateInput(request);
            if (!validationResult.IsValid)
            {
                return new ConvertToFhirResponseDto
                {
                    Success = false,
                    Message = validationResult.ErrorMessage
                };
            }

            // Save conversion request with Pending status
            var inputJson = JsonSerializer.Serialize(request.Data);
            var conversionRequest = new ConversionRequestEntity
            {
                ResourceType = request.ResourceType.ToString(),
                InputDataJson = inputJson,
                CreatedAt = DateTime.UtcNow,
                Status = ConversionStatus.Pending,
                MappingVersion = "v1"
            };

            var savedRequest = await _conversionRequestRepository.CreateAsync(conversionRequest);

            try
            {
                // Get mapper and convert to FHIR
                var mapper = GetMapper(request.ResourceType);
                var fhirJson = mapper.MapToFhirJson(request.Data);

                // Validate FHIR
                var validator = GetValidator(request.ResourceType);
                validator.Validate(fhirJson, request.ResourceType);

                var fhirResource = JsonSerializer.Deserialize<object>(fhirJson);

                // Save FHIR resource
                var fhirEntity = new FhirResourceEntity
                {
                    ConversionRequestId = savedRequest.Id,
                    FhirJson = fhirJson,
                    CreatedAt = DateTime.UtcNow
                };

                await _fhirResourceRepository.SaveAsync(fhirEntity);

                // Update status to Success
                savedRequest.Status = ConversionStatus.Success;
                savedRequest.ErrorMessage = null;
                await _conversionRequestRepository.UpdateAsync(savedRequest);

                return new ConvertToFhirResponseDto
                {
                    Id = savedRequest.Id,
                    Success = true,
                    Message = "Conversion completed successfully",
                    FhirResource = fhirResource
                };
            }
            catch (FhirValidationException ex)
            {
                // Update status to Failed
                savedRequest.Status = ConversionStatus.Failed;
                savedRequest.ErrorMessage = ex.ErrorCode;
                await _conversionRequestRepository.UpdateAsync(savedRequest);

                return new ConvertToFhirResponseDto
                {
                    Id = savedRequest.Id,
                    Success = false,
                    Message = ex.ErrorCode,
                    FhirResource = new { errorCode = ex.ErrorCode, errors = ex.ValidationErrors }
                };
            }
            catch (Exception ex)
            {
                // Update status to Failed
                savedRequest.Status = ConversionStatus.Failed;
                savedRequest.ErrorMessage = ex.Message;
                await _conversionRequestRepository.UpdateAsync(savedRequest);

                return new ConvertToFhirResponseDto
                {
                    Id = savedRequest.Id,
                    Success = false,
                    Message = $"Conversion failed: {ex.Message}"
                };
            }
        }

        public async Task<FhirResourceEntity?> GetFhirResourceByConversionRequestIdAsync(int conversionRequestId)
        {
            return await _fhirResourceRepository.GetByConversionRequestIdAsync(conversionRequestId);
        }

        public async Task<ConversionRequestEntity?> GetConversionRequestByIdAsync(int id)
        {
            return await _conversionRequestRepository.GetByIdAsync(id);
        }

        public async Task<IEnumerable<ConversionRequestEntity>> GetConversionHistoryAsync()
        {
            return await _conversionRequestRepository.GetAllAsync();
        }

        private (bool IsValid, string ErrorMessage) ValidateInput(ConvertToFhirRequestDto request)
        {
            var patient = request.Data;
            if (string.IsNullOrWhiteSpace(patient.FirstName))
                return (false, "First name is required");

            if (string.IsNullOrWhiteSpace(patient.LastName))
                return (false, "Last name is required");

            if (patient.DateOfBirth == default)
                return (false, "Date of birth is required");

            return (true, string.Empty);
        }

        private IFhirResourceMapper GetMapper(FhirResourceType resourceType)
        {
            var mapper = _mappers.FirstOrDefault(m => m.SupportedResourceType == resourceType);
            if (mapper == null)
                throw new UnsupportedResourceTypeException(resourceType);
            return mapper;
        }

        private IFhirValidator GetValidator(FhirResourceType resourceType)
        {
            var validator = _validators.FirstOrDefault(v => v.SupportedResourceType == resourceType);
            if (validator == null)
                throw new UnsupportedResourceTypeException(resourceType);
            return validator;
        }
    }
}