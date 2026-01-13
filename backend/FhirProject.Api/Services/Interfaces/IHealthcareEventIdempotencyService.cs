using FhirProject.Api.Models.Normalized;
using FhirProject.Api.Models.Results;

namespace FhirProject.Api.Services.Interfaces;

public interface IHealthcareEventIdempotencyService
{
    Task<HealthcareEventResult> ProcessHealthcareEventAsync(NormalizedHealthcareEvent healthcareEvent);
}