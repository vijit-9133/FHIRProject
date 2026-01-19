using FhirProject.Api.DTOs;
using FhirProject.Api.Repositories.Interfaces;
using FhirProject.Api.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace FhirProject.Api.Controllers
{
    [ApiController]
    [Route("api/admin/external-systems")]
    // [Authorize(Policy = "RequireAdmin")] // Temporarily disabled for testing
    public class ExternalSystemManagementController : ControllerBase
    {
        private readonly IExternalSystemService _externalSystemService;
        private readonly IFhirConversionService _fhirConversionService;

        public ExternalSystemManagementController(
            IExternalSystemService externalSystemService,
            IFhirConversionService fhirConversionService)
        {
            _externalSystemService = externalSystemService ?? throw new ArgumentNullException(nameof(externalSystemService));
            _fhirConversionService = fhirConversionService ?? throw new ArgumentNullException(nameof(fhirConversionService));
        }

        [HttpPost("register")]
        [AllowAnonymous]
        public async Task<IActionResult> RegisterExternalSystem([FromBody] RegisterExternalSystemRequest request)
        {
            if (string.IsNullOrWhiteSpace(request?.SystemName))
                return BadRequest("System name is required");

            var result = await _externalSystemService.RegisterExternalSystemAsync(request.SystemName);
            return Ok(result);
        }

        [HttpPost("{id}/approve")]
        public async Task<IActionResult> ApproveSystem(int id)
        {
            var adminUserId = GetCurrentUserId();
            var success = await _externalSystemService.ApproveSystemAsync(id, adminUserId);

            if (!success)
                return NotFound(new { message = "External system not found" });

            return Ok(new { message = "System approved successfully" });
        }

        [HttpPost("{id}/suspend")]
        public async Task<IActionResult> SuspendSystem(int id)
        {
            var success = await _externalSystemService.SuspendSystemAsync(id);

            if (!success)
                return NotFound(new { message = "External system not found" });

            return Ok(new { message = "System suspended successfully" });
        }

        [HttpPost("{id}/activate")]
        public async Task<IActionResult> ActivateSystem(int id)
        {
            var success = await _externalSystemService.ActivateSystemAsync(id);

            if (!success)
                return NotFound(new { message = "External system not found" });

            return Ok(new { message = "System activated successfully" });
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> RejectSystem(int id)
        {
            var system = await _externalSystemService.GetSystemByIdAsync(id);
            if (system == null)
                return NotFound(new { message = "External system not found" });

            await _externalSystemService.DeleteSystemAsync(id);
            return Ok(new { message = "System rejected and deleted" });
        }

        [HttpGet]
        public async Task<IActionResult> GetAllSystems()
        {
            var systems = await _externalSystemService.GetAllSystemsAsync();
            return Ok(systems);
        }

        [HttpGet("status/{clientId}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetSystemStatus(string clientId)
        {
            var system = await _externalSystemService.GetSystemByClientIdAsync(clientId);
            if (system == null)
                return NotFound(new { message = "External system not found" });

            return Ok(system);
        }

        [HttpGet("~/api/admin/conversion-requests")]
        public async Task<IActionResult> GetAllConversionRequests()
        {
            // Call repository directly to bypass userId filtering
            var repository = HttpContext.RequestServices.GetRequiredService<IConversionRequestRepository>();
            var requests = await repository.GetAllAsync();
            var orderedRequests = requests
                .OrderByDescending(r => r.CreatedAt)
                .Select(r => new
                {
                    r.Id,
                    r.ResourceType,
                    Status = r.Status.ToString(),
                    r.UserId,
                    r.CreatedAt,
                    r.ErrorMessage
                });
            return Ok(orderedRequests);
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(userIdClaim, out var userId))
            {
                return userId;
            }
            throw new UnauthorizedAccessException("User ID not found in token claims");
        }
    }

    public class RegisterExternalSystemRequest
    {
        public string SystemName { get; set; }
    }
}
