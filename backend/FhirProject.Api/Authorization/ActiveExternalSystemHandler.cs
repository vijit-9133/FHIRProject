using FhirProject.Api.Models.enums;
using FhirProject.Api.Repositories.Interfaces;
using Microsoft.AspNetCore.Authorization;

namespace FhirProject.Api.Authorization
{
    public class ActiveExternalSystemRequirement : IAuthorizationRequirement
    {
    }

    public class ActiveExternalSystemHandler : AuthorizationHandler<ActiveExternalSystemRequirement>
    {
        private readonly IExternalSystemRepository _repository;

        public ActiveExternalSystemHandler(IExternalSystemRepository repository)
        {
            _repository = repository ?? throw new ArgumentNullException(nameof(repository));
        }

        protected override async Task HandleRequirementAsync(
            AuthorizationHandlerContext context,
            ActiveExternalSystemRequirement requirement)
        {
            var clientTypeClaim = context.User.FindFirst("ClientType");
            if (clientTypeClaim?.Value != "External")
            {
                context.Fail();
                return;
            }

            var systemIdClaim = context.User.FindFirst("SystemId");
            if (systemIdClaim == null || !int.TryParse(systemIdClaim.Value, out var systemId))
            {
                context.Fail();
                return;
            }

            var system = await _repository.GetByIdAsync(systemId);
            if (system == null || system.Status != ExternalSystemStatus.Active)
            {
                context.Fail();
                return;
            }

            context.Succeed(requirement);
        }
    }
}
