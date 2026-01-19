namespace FhirProject.Api.DTOs
{
    public class ExternalSystemDto
    {
        public int Id { get; set; }
        public string ClientId { get; set; }
        public string SystemName { get; set; }
        public string Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? ApprovedByUsername { get; set; }
        public DateTime? LastAccessedAt { get; set; }
    }
}
