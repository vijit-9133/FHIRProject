namespace FhirProject.Api.Validation
{
    public class FhirValidationException : Exception
    {
        public string ErrorCode { get; }
        public List<string> ValidationErrors { get; }

        public FhirValidationException(List<string> validationErrors)
            : base("FHIR validation failed")
        {
            ErrorCode = "FHIR_VALIDATION_FAILED";
            ValidationErrors = validationErrors;
        }
    }
}