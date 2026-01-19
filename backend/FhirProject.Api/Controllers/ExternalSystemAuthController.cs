using FhirProject.Api.DTOs;
using FhirProject.Api.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FhirProject.Api.Controllers
{
    [ApiController]
    [Route("api/auth/external")]
    public class ExternalSystemAuthController : ControllerBase
    {
        private readonly IExternalSystemTokenService _tokenService;

        public ExternalSystemAuthController(IExternalSystemTokenService tokenService)
        {
            _tokenService = tokenService ?? throw new ArgumentNullException(nameof(tokenService));
        }

        [HttpPost("token")]
        public async Task<IActionResult> GetToken([FromBody] ExternalSystemTokenRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request?.ClientId) || string.IsNullOrWhiteSpace(request?.ClientSecret))
                return BadRequest(new { message = "ClientId and ClientSecret are required" });

            var response = await _tokenService.AuthenticateAndGenerateTokenAsync(request.ClientId, request.ClientSecret);

            if (response == null)
                return Unauthorized(new { message = "Invalid credentials or system not active" });

            return Ok(response);
        }
    }
}
