export interface ExternalSystemRegistrationRequest {
  systemName: string;
}

export interface ExternalSystemRegistrationResponse {
  systemId: number;
  clientId: string;
  clientSecret: string;
  systemName: string;
  status: string;
}

export interface ExternalSystemDto {
  id: number;
  clientId: string;
  systemName: string;
  status: 'PendingApproval' | 'Active' | 'Suspended';
  createdAt: string;
  approvedAt?: string;
  lastAccessedAt?: string;
  approvedByUser?: {
    userId: number;
    username: string;
  };
}

export interface ConversionRequestDto {
  id: number;
  resourceType: string;
  status: string;
  sourceSystem?: string;
  createdAt: string;
  errorMessage?: string;
}
