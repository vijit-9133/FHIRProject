export interface ConvertToFhirRequest {
  resourceType: number;
  data: PatientData | PractitionerData | OrganizationData;
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

export interface PractitionerData {
  firstName: string;
  lastName: string;
  gender: string;
  qualification: string;
  speciality: string;
  licenseNumber: string;
  phoneNumber?: string;
  email?: string;
  organizationName?: string;
}

export interface OrganizationData {
  name: string;
  type: string;
  registrationNumber: string;
  phoneNumber?: string;
  email?: string;
  addressLine?: string;
  city?: string;
  state?: string;
  postalCode?: string;
  country?: string;
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

export enum FhirResourceType {
  Patient = 1,
  Practitioner = 2,
  Organization = 3
}