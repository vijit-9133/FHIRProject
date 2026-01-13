namespace FhirProject.Api.Services.Interfaces;

public interface IClientCredentialsService
{
    bool ValidateCredentials(string clientId, string clientSecret);
    bool HasPermission(string clientId, string permission);
}