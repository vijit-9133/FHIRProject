using Hl7.Fhir.Model;

namespace FhirProject.Api.Services.Interfaces;

public interface IFhirEncounterClientService
{
    System.Threading.Tasks.Task<Encounter> CreateEncounterAsync(Encounter encounter);
    System.Threading.Tasks.Task<Encounter?> GetEncounterAsync(string encounterId);
    System.Threading.Tasks.Task UpdateEncounterAsync(Encounter encounter);
}