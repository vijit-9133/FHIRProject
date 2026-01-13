using FhirProject.Api.Services.Interfaces;

namespace FhirProject.Api.Services.Implementations;

public class ClientCredentialsService : IClientCredentialsService
{
    private readonly IConfiguration _configuration;

    public ClientCredentialsService(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public bool ValidateCredentials(string clientId, string clientSecret)
    {
        var validClients = _configuration.GetSection("ClientCredentials:ValidClients").Get<Dictionary<string, string>>();
        
        return validClients != null && 
               validClients.TryGetValue(clientId, out var expectedSecret) && 
               expectedSecret == clientSecret;
    }

    public bool HasPermission(string clientId, string permission)
    {
        var clientPermissions = _configuration.GetSection("ClientCredentials:ClientPermissions").Get<Dictionary<string, string[]>>();
        
        return clientPermissions != null &&
               clientPermissions.TryGetValue(clientId, out var permissions) &&
               permissions.Contains(permission);
    }
}