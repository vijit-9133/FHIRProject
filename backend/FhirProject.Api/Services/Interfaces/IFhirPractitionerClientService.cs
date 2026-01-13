using Hl7.Fhir.Model;

namespace FhirProject.Api.Services.Interfaces;

public interface IFhirPractitionerClientService
{
    System.Threading.Tasks.Task<Practitioner> CreatePractitionerAsync(Practitioner practitioner);
    System.Threading.Tasks.Task<Practitioner?> GetPractitionerAsync(string practitionerId);
    System.Threading.Tasks.Task UpdatePractitionerAsync(Practitioner practitioner);
}