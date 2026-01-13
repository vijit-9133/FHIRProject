namespace FhirProject.Api.Models.enums
{
    public enum ConversionStatus
    {
        Pending = 0,
        Success = 1,
        Failed = 2,
        Received = 3,
        Normalized = 4,
        TerminologyMapped = 5,
        FhirCreated = 6,
        FhirValidated = 7,
        Stored = 8,
        InProgress = 9,
        Completed = 10
    }
}