using FhirProject.Api.DTOs;
using FhirProject.Api.Models.entities;
using FhirProject.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FhirProject.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ConversionLifecycleController : ControllerBase
{
    private readonly IConversionLifecycleService _lifecycleService;

    public ConversionLifecycleController(IConversionLifecycleService lifecycleService)
    {
        _lifecycleService = lifecycleService;
    }

    [HttpGet("{id}/lifecycle")]
    public async Task<ActionResult<ConversionLifecycleDto>> GetLifecycle(int id)
    {
        var conversion = await _lifecycleService.GetConversionWithLifecycleAsync(id);
        if (conversion == null)
            return NotFound();

        var lifecycleDto = MapToLifecycleDto(conversion);
        return Ok(lifecycleDto);
    }

    private ConversionLifecycleDto MapToLifecycleDto(ConversionRequestEntity conversion)
    {
        var dto = new ConversionLifecycleDto
        {
            Id = conversion.Id,
            Status = conversion.Status,
            CreatedAt = conversion.CreatedAt,
            NormalizedAt = conversion.NormalizedAt,
            TerminologyMappedAt = conversion.TerminologyMappedAt,
            FhirCreatedAt = conversion.FhirCreatedAt,
            FhirValidatedAt = conversion.FhirValidatedAt,
            StoredAt = conversion.StoredAt,
            FailureReason = conversion.FailureReason,
            FailureStage = conversion.FailureStage,
            ErrorMessage = conversion.ErrorMessage
        };

        // Build lifecycle steps
        var steps = new List<LifecycleStepDto>
        {
            new() { Stage = "RECEIVED", CompletedAt = conversion.CreatedAt },
            new() { Stage = "NORMALIZED", CompletedAt = conversion.NormalizedAt },
            new() { Stage = "TERMINOLOGY_MAPPED", CompletedAt = conversion.TerminologyMappedAt },
            new() { Stage = "FHIR_CREATED", CompletedAt = conversion.FhirCreatedAt },
            new() { Stage = "FHIR_VALIDATED", CompletedAt = conversion.FhirValidatedAt },
            new() { Stage = "STORED", CompletedAt = conversion.StoredAt }
        };

        // Calculate durations
        DateTime? previousTime = conversion.CreatedAt;
        foreach (var step in steps)
        {
            if (step.CompletedAt.HasValue && previousTime.HasValue)
            {
                step.Duration = step.CompletedAt.Value.Subtract(previousTime.Value);
                previousTime = step.CompletedAt;
            }
        }

        dto.Steps = steps;
        return dto;
    }
}