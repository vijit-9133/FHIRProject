using FhirProject.Api.DTOs;
using FhirProject.Api.DTOs.Inbound.V1;
using FhirProject.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FhirProject.Api.Controllers;

[ApiController]
[Route("api/integration")]
public class ExternalIntegrationController : ControllerBase
{
    private readonly IClientCredentialsService _clientCredentialsService;
    private readonly IInboundNormalizationService _normalizationService;
    private readonly IHealthcareEventIdempotencyService _eventIdempotencyService;

    public ExternalIntegrationController(
        IClientCredentialsService clientCredentialsService,
        IInboundNormalizationService normalizationService,
        IHealthcareEventIdempotencyService eventIdempotencyService)
    {
        _clientCredentialsService = clientCredentialsService;
        _normalizationService = normalizationService;
        _eventIdempotencyService = eventIdempotencyService;
    }

    [HttpPost("events")]
    public async Task<ActionResult<ExternalIntegrationResponseDto>> ProcessExternalEvent(
        [FromBody] ExternalHealthcareEventV1 externalEvent,
        [FromHeader(Name = "X-Client-Id")] string clientId,
        [FromHeader(Name = "X-Client-Secret")] string clientSecret)
    {
        // Validate client credentials
        if (string.IsNullOrEmpty(clientId) || string.IsNullOrEmpty(clientSecret))
        {
            return Unauthorized("Client credentials required");
        }

        if (!_clientCredentialsService.ValidateCredentials(clientId, clientSecret))
        {
            return Unauthorized("Invalid client credentials");
        }

        try
        {
            // Normalize the external event
            var normalizedEvent = _normalizationService.Normalize(externalEvent);

            // Process with TRUE event-level idempotency
            var result = await _eventIdempotencyService.ProcessHealthcareEventAsync(normalizedEvent);

            // Map result to response DTO
            var response = new ExternalIntegrationResponseDto
            {
                ConversionRequestId = result.ConversionRequestId,
                InternalPatientFhirId = result.InternalPatientFhirId,
                InternalPractitionerFhirId = result.InternalPractitionerFhirId,
                InternalEncounterFhirId = result.InternalEncounterFhirId,
                Status = result.Status,
                Message = result.Message
            };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(500, new ExternalIntegrationResponseDto
            {
                ConversionRequestId = 0,
                Status = Models.enums.ConversionStatus.Failed,
                Message = "Failed to process external healthcare event"
            });
        }
    }
}