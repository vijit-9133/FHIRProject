using FhirProject.Api.Mapping;
using FhirProject.Api.Validation;
using System.Net;
using System.Text.Json;

namespace FhirProject.Api.Middleware
{
    public class GlobalExceptionMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<GlobalExceptionMiddleware> _logger;

        public GlobalExceptionMiddleware(RequestDelegate next, ILogger<GlobalExceptionMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "An unhandled exception occurred");
                await HandleExceptionAsync(context, ex);
            }
        }

        private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
        {
            context.Response.ContentType = "application/json";

            object response;
            int statusCode;

            switch (exception)
            {
                case FhirValidationException fhirEx:
                    statusCode = (int)HttpStatusCode.UnprocessableEntity;
                    response = new
                    {
                        success = false,
                        errorCode = fhirEx.ErrorCode,
                        errors = fhirEx.ValidationErrors
                    };
                    break;

                case UnsupportedResourceTypeException:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    response = new
                    {
                        success = false,
                        errorCode = "UNSUPPORTED_RESOURCE_TYPE",
                        message = "FHIR resource type is not supported"
                    };
                    break;

                case ArgumentException:
                case InvalidOperationException:
                    statusCode = (int)HttpStatusCode.BadRequest;
                    response = new
                    {
                        success = false,
                        errorCode = "INVALID_REQUEST",
                        message = exception.Message
                    };
                    break;

                default:
                    statusCode = (int)HttpStatusCode.InternalServerError;
                    response = new
                    {
                        success = false,
                        errorCode = "INTERNAL_SERVER_ERROR",
                        message = "An unexpected error occurred"
                    };
                    break;
            }

            context.Response.StatusCode = statusCode;
            var jsonResponse = JsonSerializer.Serialize(response, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
            await context.Response.WriteAsync(jsonResponse);
        }
    }
}