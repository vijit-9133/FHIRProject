export interface ConvertToFhirRequest {
  resourceType: number;
  data: PatientData;
}

export interface PatientData {
  firstName: string;
  lastName: string;
  dateOfBirth: string;
  gender: string;
  phoneNumber?: string;
  email?: string;
  address?: Address;
}

export interface Address {
  line1: string;
  city: string;
  state: string;
  postalCode: string;
  country: string;
}

export interface ConvertToFhirResponse {
  id: number;
  success: boolean;
  message: string;
  fhirResource?: any;
}

export interface ConversionRequest {
  id: number;
  resourceType: string;
  inputDataJson: string;
  createdAt: string;
  status: number;
  errorMessage?: string;
  mappingVersion: string;
}