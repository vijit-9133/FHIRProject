using FhirProject.Api.DTOs;
using FhirProject.Api.Mapping;
using FhirProject.Api.Models.custom;
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
        private static readonly JsonSerializerOptions PersistenceOptions = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

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
            if (request == null)
            {
                return new ConvertToFhirResponseDto
                {
                    Success = false,
                    Message = "Invalid request data"
                };
            }

            // Validate and deserialize input
            var validationResult = ValidateAndDeserializeInput(request.ResourceType, request.Data);
            var (isValid, errorMessage, deserializedModel) = ((bool, string, object?))validationResult;
            if (!isValid)
            {
                return new ConvertToFhirResponseDto
                {
                    Success = false,
                    Message = errorMessage
                };
            }

            // Save conversion request with Pending status
            var inputJson = JsonSerializer.Serialize(request.Data, PersistenceOptions);
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
                // Get mapper and convert to FHIR using deserialized model
                var mapper = GetMapper(request.ResourceType);
                var fhirJson = mapper.MapToFhirJson(deserializedModel);

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

        public async Task<ConvertToFhirResponseDto> RerunExistingConversionAsync(int conversionRequestId)
        {
            var existingRequest = await _conversionRequestRepository.GetByIdAsync(conversionRequestId);
            if (existingRequest == null)
            {
                return new ConvertToFhirResponseDto
                {
                    Success = false,
                    Message = "Conversion request not found"
                };
            }

            var resourceType = Enum.Parse<FhirResourceType>(existingRequest.ResourceType);
            var inputDataJson = existingRequest.InputDataJson;

            // Validate stored JSON data
            if (string.IsNullOrWhiteSpace(inputDataJson))
            {
                return new ConvertToFhirResponseDto
                {
                    Id = existingRequest.Id,
                    Success = false,
                    Message = "Re-run failed: No input data found in stored record"
                };
            }

            try
            {
                // Deserialize to the correct input model type based on resource type
                object inputData;
                
                switch (resourceType)
                {
                    case FhirResourceType.Patient:
                        inputData = JsonSerializer.Deserialize<CustomPatientInputModel>(inputDataJson, PersistenceOptions);
                        break;
                    case FhirResourceType.Practitioner:
                        inputData = JsonSerializer.Deserialize<CustomPractitionerInputModel>(inputDataJson, PersistenceOptions);
                        break;
                    case FhirResourceType.Organization:
                        inputData = JsonSerializer.Deserialize<CustomOrganizationInputModel>(inputDataJson, PersistenceOptions);
                        break;
                    default:
                        throw new UnsupportedResourceTypeException(resourceType);
                }

                // Reset status to Pending only after successful JSON parsing
                existingRequest.Status = ConversionStatus.Pending;
                existingRequest.ErrorMessage = null;
                await _conversionRequestRepository.UpdateAsync(existingRequest);

                var mapper = GetMapper(resourceType);
                var fhirJson = mapper.MapToFhirJson(inputData);

                // Skip validation for rerun since data was already validated originally
                var fhirResource = JsonSerializer.Deserialize<object>(fhirJson);

                // Delete existing FHIR resource if any
                var existingFhirResource = await _fhirResourceRepository.GetByConversionRequestIdAsync(conversionRequestId);
                if (existingFhirResource != null)
                {
                    await _fhirResourceRepository.DeleteAsync(existingFhirResource.Id);
                }

                // Save new FHIR resource
                var fhirEntity = new FhirResourceEntity
                {
                    ConversionRequestId = existingRequest.Id,
                    FhirJson = fhirJson,
                    CreatedAt = DateTime.UtcNow
                };

                await _fhirResourceRepository.SaveAsync(fhirEntity);

                // Update status to Success
                existingRequest.Status = ConversionStatus.Success;
                existingRequest.ErrorMessage = null;
                await _conversionRequestRepository.UpdateAsync(existingRequest);

                return new ConvertToFhirResponseDto
                {
                    Id = existingRequest.Id,
                    Success = true,
                    Message = "Re-run completed successfully",
                    FhirResource = fhirResource
                };
            }
            catch (JsonException)
            {
                return new ConvertToFhirResponseDto
                {
                    Id = existingRequest.Id,
                    Success = false,
                    Message = "This conversion was created before replay support was added and cannot be re-run."
                };
            }
            catch (Exception ex)
            {
                existingRequest.Status = ConversionStatus.Failed;
                existingRequest.ErrorMessage = ex.Message;
                await _conversionRequestRepository.UpdateAsync(existingRequest);

                return new ConvertToFhirResponseDto
                {
                    Id = existingRequest.Id,
                    Success = false,
                    Message = $"Re-run failed: {ex.Message}"
                };
            }
        }

        private (bool IsValid, string ErrorMessage, object? DeserializedModel) ValidateAndDeserializeInput(FhirResourceType resourceType, dynamic data)
        {
            try
            {
                // Serialize the dynamic data to JSON first
                var dataJson = JsonSerializer.Serialize(data, PersistenceOptions);
                if (string.IsNullOrWhiteSpace(dataJson) || dataJson == "{}")
                    return (false, "Request data cannot be empty", null);

                if (resourceType == FhirResourceType.Patient)
                {
                    var patient = JsonSerializer.Deserialize<CustomPatientInputModel>(dataJson, PersistenceOptions);
                    if (string.IsNullOrWhiteSpace(patient?.FirstName))
                        return (false, "First name is required", null);

                    if (string.IsNullOrWhiteSpace(patient?.LastName))
                        return (false, "Last name is required", null);

                    if (patient?.DateOfBirth == DateTime.MinValue)
                        return (false, "Date of birth is required", null);

                    return (true, string.Empty, patient);
                }
                else if (resourceType == FhirResourceType.Practitioner)
                {
                    var practitioner = JsonSerializer.Deserialize<CustomPractitionerInputModel>(dataJson, PersistenceOptions);
                    
                    if (string.IsNullOrWhiteSpace(practitioner?.FirstName))
                        return (false, "First name is required", null);

                    if (string.IsNullOrWhiteSpace(practitioner?.LastName))
                        return (false, "Last name is required", null);

                    if (string.IsNullOrWhiteSpace(practitioner?.LicenseNumber))
                        return (false, "License number is required", null);

                    return (true, string.Empty, practitioner);
                }
                else if (resourceType == FhirResourceType.Organization)
                {
                    var organization = JsonSerializer.Deserialize<CustomOrganizationInputModel>(dataJson, PersistenceOptions);
                    
                    if (string.IsNullOrWhiteSpace(organization?.Name))
                        return (false, "Organization name is required", null);

                    if (string.IsNullOrWhiteSpace(organization?.RegistrationNumber))
                        return (false, "Registration number is required", null);

                    return (true, string.Empty, organization);
                }

                return (false, "Unsupported resource type", null);
            }
            catch (Exception ex)
            {
                return (false, $"Invalid data format: {ex.Message}", null);
            }
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