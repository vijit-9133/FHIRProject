import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ExternalSystemRegistrationRequest, ExternalSystemRegistrationResponse, ExternalSystemDto } from '../../shared/models/external-system.models';

@Injectable({
  providedIn: 'root'
})
export class ExternalSystemApiService {
  private baseUrl = 'http://localhost:5078/api/admin/external-systems';

  constructor(private http: HttpClient) {}

  register(request: ExternalSystemRegistrationRequest): Observable<ExternalSystemRegistrationResponse> {
    return this.http.post<ExternalSystemRegistrationResponse>(`${this.baseUrl}/register`, request);
  }

  getSystemStatus(clientId: string): Observable<ExternalSystemDto> {
    return this.http.get<ExternalSystemDto>(`${this.baseUrl}/status/${clientId}`);
  }

  getAllSystems(): Observable<ExternalSystemDto[]> {
    return this.http.get<ExternalSystemDto[]>(this.baseUrl);
  }
}
