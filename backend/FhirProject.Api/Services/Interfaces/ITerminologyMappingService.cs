using FhirProject.Api.Models.Terminology;

namespace FhirProject.Api.Services.Interfaces;

public interface ITerminologyMappingService
{
    TerminologyMapping? MapDiagnosisToIcd10(string diagnosisText);
    TerminologyMapping? MapEncounterTypeToFhir(string encounterType);
    TerminologyMapping? MapPractitionerSpecialtyToSnomed(string specialty);
}