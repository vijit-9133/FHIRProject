namespace FhirProject.Api.Models.fhir
{
    public class FhirHumanNameModel
{
    public string Use { get; set; }
    public string Family { get; set; }
    public List<string> Given { get; set; }
}
}