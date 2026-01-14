using FhirProject.Api.Services.Interfaces;
using Hl7.Fhir.Rest;
using Hl7.Fhir.Serialization;
using Microsoft.AspNetCore.Mvc;

namespace FhirProject.Api.Controllers;

[ApiController]
[Route("api/fhir")]
public class FhirResourceController : ControllerBase
{
    private readonly IFhirPatientClientService _patientClient;
    private readonly IFhirPractitionerClientService _practitionerClient;
    private readonly IFhirEncounterClientService _encounterClient;
    private readonly FhirJsonSerializer _serializer;

    public FhirResourceController(
        IFhirPatientClientService patientClient,
        IFhirPractitionerClientService practitionerClient,
        IFhirEncounterClientService encounterClient)
    {
        _patientClient = patientClient;
        _practitionerClient = practitionerClient;
        _encounterClient = encounterClient;
        _serializer = new FhirJsonSerializer();
    }

    [HttpGet("patient/{id}")]
    public async Task<IActionResult> GetPatient(string id)
    {
        try
        {
            var patient = await _patientClient.GetPatientAsync(id);
            if (patient == null)
            {
                return NotFound(new { message = $"FHIR Patient with id {id} not found" });
            }

            var json = _serializer.SerializeToString(patient);
            return Content(json, "application/fhir+json");
        }
        catch (FhirOperationException ex) when (ex.Status == System.Net.HttpStatusCode.NotFound)
        {
            return NotFound(new { message = $"FHIR Patient with id {id} not found" });
        }
    }

    [HttpGet("practitioner/{id}")]
    public async Task<IActionResult> GetPractitioner(string id)
    {
        try
        {
            var practitioner = await _practitionerClient.GetPractitionerAsync(id);
            if (practitioner == null)
            {
                return NotFound(new { message = $"FHIR Practitioner with id {id} not found" });
            }

            var json = _serializer.SerializeToString(practitioner);
            return Content(json, "application/fhir+json");
        }
        catch (FhirOperationException ex) when (ex.Status == System.Net.HttpStatusCode.NotFound)
        {
            return NotFound(new { message = $"FHIR Practitioner with id {id} not found" });
        }
    }

    [HttpGet("encounter/{id}")]
    public async Task<IActionResult> GetEncounter(string id)
    {
        try
        {
            var encounter = await _encounterClient.GetEncounterAsync(id);
            if (encounter == null)
            {
                return NotFound(new { message = $"FHIR Encounter with id {id} not found" });
            }

            var json = _serializer.SerializeToString(encounter);
            return Content(json, "application/fhir+json");
        }
        catch (FhirOperationException ex) when (ex.Status == System.Net.HttpStatusCode.NotFound)
        {
            return NotFound(new { message = $"FHIR Encounter with id {id} not found" });
        }
    }
}
