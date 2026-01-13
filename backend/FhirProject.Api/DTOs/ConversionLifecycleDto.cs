using FhirProject.Api.Models.enums;

namespace FhirProject.Api.DTOs;

public class ConversionLifecycleDto
{
    public int Id { get; set; }
    public ConversionStatus Status { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? NormalizedAt { get; set; }
    public DateTime? TerminologyMappedAt { get; set; }
    public DateTime? FhirCreatedAt { get; set; }
    public DateTime? FhirValidatedAt { get; set; }
    public DateTime? StoredAt { get; set; }
    public string? FailureReason { get; set; }
    public string? FailureStage { get; set; }
    public string? ErrorMessage { get; set; }
    public TimeSpan? TotalProcessingTime => StoredAt?.Subtract(CreatedAt);
    public List<LifecycleStepDto> Steps { get; set; } = new();
}

public class LifecycleStepDto
{
    public string Stage { get; set; } = string.Empty;
    public DateTime? CompletedAt { get; set; }
    public bool IsCompleted => CompletedAt.HasValue;
    public TimeSpan? Duration { get; set; }
}