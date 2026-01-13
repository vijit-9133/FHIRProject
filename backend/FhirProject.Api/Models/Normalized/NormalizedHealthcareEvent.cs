namespace FhirProject.Api.Models.Normalized;

public class NormalizedHealthcareEvent
{
    public required string SourceSystem { get; set; }
    public required string SourceSystemVersion { get; set; }
    public required string ExternalReferenceId { get; set; }
    public DateTime EventTimestamp { get; set; }
    public NormalizedPatient? Patient { get; set; }
    public NormalizedPractitioner? Practitioner { get; set; }
    public NormalizedEncounter? Encounter { get; set; }
}

public class NormalizedPatient
{
    public required string ExternalPatientId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public DateTime? DateOfBirth { get; set; }
    public string? Gender { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public NormalizedAddress? Address { get; set; }
}

public class NormalizedPractitioner
{
    public required string ExternalPractitionerId { get; set; }
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? Qualification { get; set; }
    public string? Specialty { get; set; }
    public string? PhoneNumber { get; set; }
    public string? Email { get; set; }
    public NormalizedAddress? Address { get; set; }
}

public class NormalizedEncounter
{
    public required string ExternalEncounterId { get; set; }
    public string? EncounterType { get; set; }
    public string? Status { get; set; }
    public DateTime? StartDateTime { get; set; }
    public DateTime? EndDateTime { get; set; }
    public string? ReasonCode { get; set; }
    public string? ReasonDisplay { get; set; }
    public string? Location { get; set; }
}

public class NormalizedAddress
{
    public string? Line1 { get; set; }
    public string? Line2 { get; set; }
    public string? City { get; set; }
    public string? State { get; set; }
    public string? PostalCode { get; set; }
    public string? Country { get; set; }
}