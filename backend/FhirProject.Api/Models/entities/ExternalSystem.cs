using FhirProject.Api.Models.enums;

namespace FhirProject.Api.Models.entities
{
    public class ExternalSystem
    {
        public int Id { get; set; }
        public string ClientId { get; set; }
        public string ClientSecretHash { get; set; }
        public string SystemName { get; set; }
        public ExternalSystemStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public int? ApprovedByUserId { get; set; }
        public DateTime? LastAccessedAt { get; set; }

        // Navigation property
        public UserEntity? ApprovedByUser { get; set; }
    }
}
