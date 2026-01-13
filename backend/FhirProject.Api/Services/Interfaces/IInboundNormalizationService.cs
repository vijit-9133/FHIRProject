using FhirProject.Api.DTOs.Inbound.V1;
using FhirProject.Api.Models.Normalized;

namespace FhirProject.Api.Services.Interfaces;

public interface IInboundNormalizationService
{
    NormalizedHealthcareEvent Normalize(ExternalHealthcareEventV1 externalEvent);
}