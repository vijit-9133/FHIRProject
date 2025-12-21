namespace FhirProject.Api.Models.fhir
{
    public class FhirAddressModel
{
    public List<string> Line { get; set; }
    public string City { get; set; }
    public string State { get; set; }
    public string PostalCode { get; set; }
    public string Country { get; set; }
}
}