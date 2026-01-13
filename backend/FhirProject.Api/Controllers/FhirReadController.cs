using FhirProject.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace FhirProject.Api.Controllers;

[ApiController]
[Route("api/fhir")]
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

    [HttpGet("Patient/{id}")]
    public async Task<IActionResult> GetPatient(
        string id,
        [FromHeader(Name = "X-Client-Id")] string clientId,
        [FromHeader(Name = "X-Client-Secret")] string clientSecret)
    {
        // Validate client credentials
        if (!ValidateClient(clientId, clientSecret))
            return Unauthorized("Invalid client credentials");

        // Check read permission
        if (!_clientCredentialsService.HasPermission(clientId, "read:Patient"))
            return Forbid("Insufficient permissions to read Patient resources");

        var patientJson = await _fhirResourceService.GetPatientByIdAsync(id);
        if (patientJson == null)
            return NotFound();

        return Content(patientJson, "application/fhir+json");
    }

    [HttpGet("Patient")]
    public async Task<IActionResult> SearchPatients(
        [FromQuery] string? identifier,
        [FromHeader(Name = "X-Client-Id")] string clientId,
        [FromHeader(Name = "X-Client-Secret")] string clientSecret)
    {
        // Validate client credentials
        if (!ValidateClient(clientId, clientSecret))
            return Unauthorized("Invalid client credentials");

        // Check search permission
        if (!_clientCredentialsService.HasPermission(clientId, "search:Patient"))
            return Forbid("Insufficient permissions to search Patient resources");

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

    [HttpGet("Encounter")]
    public async Task<IActionResult> SearchEncounters(
        [FromQuery] string? patient,
        [FromHeader(Name = "X-Client-Id")] string clientId,
        [FromHeader(Name = "X-Client-Secret")] string clientSecret)
    {
        // Validate client credentials
        if (!ValidateClient(clientId, clientSecret))
            return Unauthorized("Invalid client credentials");

        // Check search permission
        if (!_clientCredentialsService.HasPermission(clientId, "search:Encounter"))
            return Forbid("Insufficient permissions to search Encounter resources");

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

    private bool ValidateClient(string clientId, string clientSecret)
    {
        return !string.IsNullOrEmpty(clientId) && 
               !string.IsNullOrEmpty(clientSecret) && 
               _clientCredentialsService.ValidateCredentials(clientId, clientSecret);
    }
}