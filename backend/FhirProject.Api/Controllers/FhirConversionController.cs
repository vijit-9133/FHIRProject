using FhirProject.Api.DTOs;
using FhirProject.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FhirProject.Api.Controllers
{
    /// <summary>
    /// Controller for FHIR data conversion and retrieval operations
    /// </summary>
    [ApiController]
    [Route("api/fhir")]
    public class FhirConversionController : ControllerBase
    {
        private readonly IFhirConversionService _fhirConversionService;

        public FhirConversionController(IFhirConversionService fhirConversionService)
        {
            _fhirConversionService = fhirConversionService ?? throw new ArgumentNullException(nameof(fhirConversionService));
        }

        /// <summary>
        /// Converts custom healthcare data to FHIR-compliant format
        /// </summary>
        /// <param name="request">The conversion request containing custom data</param>
        /// <returns>FHIR conversion result</returns>
        [HttpPost("convert")]
        public async Task<IActionResult> ConvertToFhir([FromBody] ConvertToFhirRequestDto request)
        {
            Console.WriteLine($"Received request: {System.Text.Json.JsonSerializer.Serialize(request)}");
            
            if (request == null)
            {
                Console.WriteLine("Request is null");
                return BadRequest("Request cannot be null");
            }

            if (request.Data == null)
            {
                Console.WriteLine("Request.Data is null");
                return BadRequest("Request data cannot be null");
            }

            try
            {
                var result = await _fhirConversionService.ConvertToFhirAsync(request);
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { message = "An unexpected error occurred", error = ex.Message });
            }
        }

        /// <summary>
        /// Gets FHIR resource by conversion request ID
        /// </summary>
        /// <param name="conversionRequestId">The conversion request ID</param>
        /// <returns>FHIR resource JSON</returns>
        [HttpGet("{conversionRequestId}")]
        public async Task<IActionResult> GetFhirResource(int conversionRequestId)
        {
            try
            {
                var fhirResource = await _fhirConversionService.GetFhirResourceByConversionRequestIdAsync(conversionRequestId);
                
                if (fhirResource == null)
                    return NotFound($"FHIR resource not found for conversion request ID: {conversionRequestId}");

                return Ok(fhirResource);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred", error = ex.Message });
            }
        }

        /// <summary>
        /// Gets conversion request metadata by ID
        /// </summary>
        /// <param name="id">The conversion request ID</param>
        /// <returns>Conversion request metadata</returns>
        [HttpGet("request/{id}")]
        public async Task<IActionResult> GetConversionRequest(int id)
        {
            try
            {
                var conversionRequest = await _fhirConversionService.GetConversionRequestByIdAsync(id);
                
                if (conversionRequest == null)
                    return NotFound($"Conversion request not found with ID: {id}");

                return Ok(conversionRequest);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred", error = ex.Message });
            }
        }

        /// <summary>
        /// Gets all conversion history
        /// </summary>
        /// <returns>List of all conversion requests</returns>
        [HttpGet("history")]
        public async Task<IActionResult> GetConversionHistory()
        {
            try
            {
                var history = await _fhirConversionService.GetConversionHistoryAsync();
                return Ok(history);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred", error = ex.Message });
            }
        }
    }
}