using Hl7.Fhir.Model;
using System.Threading.Tasks;

namespace FhirProject.Api.Services.Interfaces;

public interface IFhirPatientClientService
{
    System.Threading.Tasks.Task<Patient> CreatePatientAsync(Patient patient);
    System.Threading.Tasks.Task<Patient?> GetPatientAsync(string patientId);
    System.Threading.Tasks.Task<Patient> UpdatePatientAsync(string patientId, Patient patient);
}