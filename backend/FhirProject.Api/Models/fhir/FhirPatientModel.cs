namespace FhirProject.Api.Models.fhir
{
    public class FhirPatientModel
{
    public string ResourceType => "Patient";

    public List<FhirHumanNameModel> Name { get; set; }
    public string Gender { get; set; }
    public DateTime? BirthDate { get; set; }

    public List<FhirContactPointModel> Telecom { get; set; }
    public List<FhirAddressModel> Address { get; set; }
}
}