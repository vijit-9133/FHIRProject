namespace FhirProject.Api.Services.Llm;

public static class ResourceSchemas
{
    public const string PatientSchema = @"{
  ""firstName"": ""string|null"",
  ""lastName"": ""string|null"",
  ""dateOfBirth"": ""string|null (ISO format)"",
  ""gender"": ""string|null (male/female/other)"",
  ""phoneNumber"": ""string|null"",
  ""email"": ""string|null"",
  ""address"": {
    ""line1"": ""string|null"",
    ""city"": ""string|null"",
    ""state"": ""string|null"",
    ""postalCode"": ""string|null"",
    ""country"": ""string|null""
  }
}";

    public const string PractitionerSchema = @"{
  ""firstName"": ""string|null"",
  ""lastName"": ""string|null"",
  ""gender"": ""string|null (male/female/other)"",
  ""qualification"": ""string|null"",
  ""speciality"": ""string|null"",
  ""licenseNumber"": ""string|null"",
  ""phoneNumber"": ""string|null"",
  ""email"": ""string|null""
}";

    public const string OrganizationSchema = @"{
  ""name"": ""string|null"",
  ""type"": ""string|null"",
  ""registrationNumber"": ""string|null"",
  ""phoneNumber"": ""string|null"",
  ""email"": ""string|null"",
  ""address"": {
    ""line1"": ""string|null"",
    ""city"": ""string|null"",
    ""state"": ""string|null"",
    ""postalCode"": ""string|null"",
    ""country"": ""string|null""
  }
}";
}