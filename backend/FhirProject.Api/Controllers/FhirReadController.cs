using FhirProject.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace FhirProject.Api.Controllers;

[ApiController]
[Route("api/fhir")]
[Authorize(Policy = "RequireExternalSystem")]
public class FhirReadController : ControllerBase
{
    private readonly IClientCredentialsService _clientCredentialsService;
    private readonly IFhirResourceService _fhirResourceService;

    public FhirReadController(
        IClientCredentialsService clientCredentialsService,
        IFhirResourceService fhirResourceService)
    {
        _clientCredentialsService = clientCredentialsService;
        _fhirResourceService = fhirResourceService;
    }

    [HttpGet("Patients/{id}")]
    public async Task<IActionResult> GetPatient(string id)
    {
        var patientJson = await _fhirResourceService.GetPatientByIdAsync(id);
        if (patientJson == null)
            return NotFound();

        return Content(patientJson, "application/fhir+json");
    }

    [HttpGet("Patients")]
    public async Task<IActionResult> SearchPatients([FromQuery] string? identifier)
    {
        if (string.IsNullOrEmpty(identifier))
            return BadRequest("identifier parameter is required");

        var results = await _fhirResourceService.SearchPatientsByIdentifierAsync(identifier);
        if (!results.Any())
        {
            // Return empty Bundle
            var emptyBundle = @"{
  ""resourceType"": ""Bundle"",
  ""type"": ""searchset"",
  ""total"": 0,
  ""entry"": []
}";
            return Content(emptyBundle, "application/fhir+json");
        }

        return Content(results.First(), "application/fhir+json");
    }

    [HttpGet("Encounters")]
    public async Task<IActionResult> SearchEncounters([FromQuery] string? patient)
    {
        if (string.IsNullOrEmpty(patient))
            return BadRequest("patient parameter is required");

        var results = await _fhirResourceService.SearchEncountersByPatientAsync(patient);
        if (!results.Any())
        {
            // Return empty Bundle
            var emptyBundle = @"{
  ""resourceType"": ""Bundle"",
  ""type"": ""searchset"",
  ""total"": 0,
  ""entry"": []
}";
            return Content(emptyBundle, "application/fhir+json");
        }

        return Content(results.First(), "application/fhir+json");
    }
}