using FhirProject.Api.DTOs;
using FhirProject.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace FhirProject.Api.Controllers
{
    /// <summary>
    /// Controller for FHIR data conversion and retrieval operations
    /// </summary>
    [ApiController]
    [Route("api/fhir")]
    [Authorize]
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

            try
            {
                // Extract user ID from JWT claims (required)
                var userId = GetCurrentUserId();
                
                var result = await _fhirConversionService.ConvertToFhirAsync(request, userId);
                Console.WriteLine($"Service result: Success={result.Success}, Message={result.Message}");
                
                if (result.Success)
                    return Ok(result);
                
                return BadRequest(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "Authentication required" });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, new { message = "An unexpected error occurred", error = ex.Message });
            }
        }

        /// <summary>
        /// Extracts user ID from JWT claims with strict enforcement
        /// </summary>
        /// <returns>User ID from authenticated user</returns>
        /// <exception cref="UnauthorizedAccessException">When user ID cannot be extracted</exception>
        private int GetCurrentUserId()
        {
            var userIdClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("User ID not found in token claims");
        }

        /// <summary>
        /// Checks if a conversion request exists regardless of ownership
        /// </summary>
        /// <param name="id">Conversion request ID</param>
        /// <returns>True if exists, false otherwise</returns>
        private async Task<bool> ConversionRequestExistsAsync(int id)
        {
            var request = await _fhirConversionService.GetConversionRequestByIdAsync(id);
            return request != null;
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
                var userId = GetCurrentUserId();
                var fhirResource = await _fhirConversionService.GetFhirResourceByConversionRequestIdAsync(conversionRequestId, userId);
                
                if (fhirResource != null)
                {
                    return Ok(fhirResource);
                }

                // Check if the conversion request exists to determine 403 vs 404
                if (await ConversionRequestExistsAsync(conversionRequestId))
                {
                    return Forbid(); // Exists but user doesn't own it
                }
                
                return NotFound($"FHIR resource not found for conversion request ID: {conversionRequestId}");
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "Authentication required" });
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
                var userId = GetCurrentUserId();
                var conversionRequest = await _fhirConversionService.GetConversionRequestByIdAsync(id, userId);
                
                if (conversionRequest != null)
                {
                    return Ok(conversionRequest);
                }

                // Check if the conversion request exists to determine 403 vs 404
                if (await ConversionRequestExistsAsync(id))
                {
                    return Forbid(); // Exists but user doesn't own it
                }
                
                return NotFound($"Conversion request not found with ID: {id}");
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "Authentication required" });
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
                var userId = GetCurrentUserId();
                var history = await _fhirConversionService.GetConversionHistoryAsync(userId);
                return Ok(history);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "Authentication required" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred", error = ex.Message });
            }
        }

        /// <summary>
        /// Re-runs an existing conversion request
        /// </summary>
        /// <param name="conversionRequestId">The conversion request ID to re-run</param>
        /// <returns>FHIR conversion result</returns>
        [HttpPost("rerun/{conversionRequestId}")]
        public async Task<IActionResult> RerunConversion(int conversionRequestId)
        {
            try
            {
                var userId = GetCurrentUserId();
                var result = await _fhirConversionService.RerunExistingConversionAsync(conversionRequestId, userId);
                
                if (result.Success)
                {
                    return Ok(result);
                }

                // Check if conversion not found vs access denied
                if (result.Message == "Conversion request not found")
                {
                    // Check if the conversion request exists to determine 403 vs 404
                    if (await ConversionRequestExistsAsync(conversionRequestId))
                    {
                        return Forbid(); // Exists but user doesn't own it
                    }
                    return NotFound(new { message = result.Message });
                }
                
                return BadRequest(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "Authentication required" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An unexpected error occurred", error = ex.Message });
            }
        }
    }
}