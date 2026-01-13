using FhirProject.Api.Models.Terminology;
using FhirProject.Api.Services.Interfaces;

namespace FhirProject.Api.Services.Implementations.Terminology;

public class TerminologyMappingService : ITerminologyMappingService
{
    private readonly Dictionary<string, (string Code, string Display)> _diagnosisToIcd10 = new(StringComparer.OrdinalIgnoreCase)
    {
        { "diabetes", ("E11.9", "Type 2 diabetes mellitus without complications") },
        { "hypertension", ("I10", "Essential hypertension") },
        { "pneumonia", ("J18.9", "Pneumonia, unspecified organism") },
        { "asthma", ("J45.9", "Asthma, unspecified") },
        { "depression", ("F32.9", "Major depressive disorder, single episode, unspecified") },
        { "anxiety", ("F41.9", "Anxiety disorder, unspecified") },
        { "chest pain", ("R06.02", "Shortness of breath") },
        { "headache", ("R51", "Headache") },
        { "fever", ("R50.9", "Fever, unspecified") },
        { "covid", ("U07.1", "COVID-19") }
    };

    private readonly Dictionary<string, (string Code, string Display)> _encounterTypeToFhir = new(StringComparer.OrdinalIgnoreCase)
    {
        { "inpatient", ("IMP", "inpatient encounter") },
        { "outpatient", ("AMB", "ambulatory") },
        { "emergency", ("EMER", "emergency") },
        { "urgent care", ("EMER", "emergency") },
        { "home health", ("HH", "home health") },
        { "virtual", ("VR", "virtual") },
        { "telehealth", ("VR", "virtual") },
        { "observation", ("OBSENC", "observation encounter") },
        { "day surgery", ("SS", "short stay") },
        { "wellness", ("AMB", "ambulatory") }
    };

    private readonly Dictionary<string, (string Code, string Display)> _specialtyToSnomed = new(StringComparer.OrdinalIgnoreCase)
    {
        { "cardiology", ("17561000", "Cardiologist") },
        { "dermatology", ("18803008", "Dermatologist") },
        { "emergency medicine", ("309343006", "Physician") },
        { "family medicine", ("62247001", "Family medicine specialist") },
        { "internal medicine", ("39677007", "Internal medicine specialist") },
        { "neurology", ("56397003", "Neurologist") },
        { "oncology", ("66862007", "Radiologist") },
        { "orthopedics", ("22731001", "Orthopedic surgeon") },
        { "pediatrics", ("82296001", "Pediatrician") },
        { "psychiatry", ("80584001", "Psychiatrist") },
        { "radiology", ("66862007", "Radiologist") },
        { "surgery", ("304292004", "Surgeon") },
        { "urology", ("24590004", "Urologist") },
        { "gynecology", ("83685006", "Gynecologist") },
        { "anesthesiology", ("88189002", "Anesthesiologist") }
    };

    public TerminologyMapping? MapDiagnosisToIcd10(string diagnosisText)
    {
        if (string.IsNullOrWhiteSpace(diagnosisText))
            return null;

        var normalizedText = diagnosisText.Trim();
        
        foreach (var kvp in _diagnosisToIcd10)
        {
            if (normalizedText.Contains(kvp.Key, StringComparison.OrdinalIgnoreCase))
            {
                return new TerminologyMapping
                {
                    OriginalText = diagnosisText,
                    MappedCode = kvp.Value.Code,
                    CodeSystem = "http://hl7.org/fhir/sid/icd-10-cm",
                    Display = kvp.Value.Display
                };
            }
        }

        return null;
    }

    public TerminologyMapping? MapEncounterTypeToFhir(string encounterType)
    {
        if (string.IsNullOrWhiteSpace(encounterType))
            return null;

        if (_encounterTypeToFhir.TryGetValue(encounterType.Trim(), out var mapping))
        {
            return new TerminologyMapping
            {
                OriginalText = encounterType,
                MappedCode = mapping.Code,
                CodeSystem = "http://terminology.hl7.org/CodeSystem/v3-ActCode",
                Display = mapping.Display
            };
        }

        return null;
    }

    public TerminologyMapping? MapPractitionerSpecialtyToSnomed(string specialty)
    {
        if (string.IsNullOrWhiteSpace(specialty))
            return null;

        if (_specialtyToSnomed.TryGetValue(specialty.Trim(), out var mapping))
        {
            return new TerminologyMapping
            {
                OriginalText = specialty,
                MappedCode = mapping.Code,
                CodeSystem = "http://snomed.info/sct",
                Display = mapping.Display
            };
        }

        return null;
    }
}