using FhirProject.Api.DTOs.Inbound.V1;
using FhirProject.Api.Models.Normalized;
using FhirProject.Api.Services.Interfaces;
using System.Text.RegularExpressions;

namespace FhirProject.Api.Services.Implementations.Normalization;

public class InboundNormalizationService : IInboundNormalizationService
{
    public NormalizedHealthcareEvent Normalize(ExternalHealthcareEventV1 externalEvent)
    {
        return new NormalizedHealthcareEvent
        {
            SourceSystem = NormalizeString(externalEvent.SourceSystem),
            SourceSystemVersion = NormalizeString(externalEvent.SourceSystemVersion),
            ExternalReferenceId = NormalizeString(externalEvent.ExternalReferenceId),
            EventTimestamp = NormalizeDateTime(externalEvent.EventTimestamp),
            Patient = externalEvent.Patient != null ? NormalizePatient(externalEvent.Patient) : null,
            Practitioner = externalEvent.Practitioner != null ? NormalizePractitioner(externalEvent.Practitioner) : null,
            Encounter = externalEvent.Encounter != null ? NormalizeEncounter(externalEvent.Encounter) : null
        };
    }

    private NormalizedPatient NormalizePatient(ExternalPatientV1 patient)
    {
        return new NormalizedPatient
        {
            ExternalPatientId = NormalizeString(patient.ExternalPatientId),
            FirstName = NormalizeString(patient.FirstName),
            LastName = NormalizeString(patient.LastName),
            DateOfBirth = patient.DateOfBirth,
            Gender = NormalizeGender(patient.Gender),
            PhoneNumber = NormalizePhoneNumber(patient.PhoneNumber),
            Email = NormalizeString(patient.Email),
            Address = patient.Address != null ? NormalizeAddress(patient.Address) : null
        };
    }

    private NormalizedPractitioner NormalizePractitioner(ExternalPractitionerV1 practitioner)
    {
        return new NormalizedPractitioner
        {
            ExternalPractitionerId = NormalizeString(practitioner.ExternalPractitionerId),
            FirstName = NormalizeString(practitioner.FirstName),
            LastName = NormalizeString(practitioner.LastName),
            Qualification = NormalizeString(practitioner.Qualification),
            Specialty = NormalizeString(practitioner.Specialty),
            PhoneNumber = NormalizePhoneNumber(practitioner.PhoneNumber),
            Email = NormalizeString(practitioner.Email),
            Address = practitioner.Address != null ? NormalizeAddress(practitioner.Address) : null
        };
    }

    private NormalizedEncounter NormalizeEncounter(ExternalEncounterV1 encounter)
    {
        return new NormalizedEncounter
        {
            ExternalEncounterId = NormalizeString(encounter.ExternalEncounterId),
            EncounterType = NormalizeString(encounter.EncounterType),
            Status = NormalizeString(encounter.Status),
            StartDateTime = encounter.StartDateTime,
            EndDateTime = encounter.EndDateTime,
            ReasonCode = NormalizeString(encounter.ReasonCode),
            ReasonDisplay = NormalizeString(encounter.ReasonDisplay),
            Location = NormalizeString(encounter.Location)
        };
    }

    private NormalizedAddress NormalizeAddress(ExternalAddressV1 address)
    {
        return new NormalizedAddress
        {
            Line1 = NormalizeString(address.Line1),
            Line2 = NormalizeString(address.Line2),
            City = NormalizeString(address.City),
            State = NormalizeString(address.State),
            PostalCode = NormalizeString(address.PostalCode),
            Country = NormalizeString(address.Country)
        };
    }

    private string? NormalizeString(string? input)
    {
        return string.IsNullOrWhiteSpace(input) ? input : input.Trim();
    }

    private DateTime NormalizeDateTime(DateTime dateTime)
    {
        return DateTime.SpecifyKind(dateTime, DateTimeKind.Utc);
    }

    private string? NormalizeGender(string? gender)
    {
        if (string.IsNullOrWhiteSpace(gender))
            return gender;

        return gender.Trim().ToLowerInvariant() switch
        {
            "m" or "male" => "male",
            "f" or "female" => "female",
            "o" or "other" => "other",
            "u" or "unknown" => "unknown",
            _ => gender.Trim().ToLowerInvariant()
        };
    }

    private string? NormalizePhoneNumber(string? phoneNumber)
    {
        if (string.IsNullOrWhiteSpace(phoneNumber))
            return phoneNumber;

        var cleaned = Regex.Replace(phoneNumber, @"[^\d+]", "");
        
        if (cleaned.StartsWith("+"))
            return cleaned;
        
        if (cleaned.Length == 10 && !cleaned.StartsWith("1"))
            return $"+1{cleaned}";
        
        if (cleaned.Length == 11 && cleaned.StartsWith("1"))
            return $"+{cleaned}";
        
        return phoneNumber.Trim();
    }
}