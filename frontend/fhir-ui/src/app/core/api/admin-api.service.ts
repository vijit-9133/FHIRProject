import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { ExternalSystemDto, ConversionRequestDto } from '../../shared/models/external-system.models';

@Injectable({
  providedIn: 'root'
})
export class AdminApiService {
  private baseUrl = 'http://localhost:5078/api/admin/external-systems';
  private conversionBaseUrl = 'http://localhost:5078/api/admin/conversion-requests';

  constructor(private http: HttpClient) {}

  getAllSystems(): Observable<ExternalSystemDto[]> {
    return this.http.get<ExternalSystemDto[]>(this.baseUrl);
  }

  approveSystem(id: number): Observable<ExternalSystemDto> {
    return this.http.post<ExternalSystemDto>(`${this.baseUrl}/${id}/approve`, {});
  }

  suspendSystem(id: number): Observable<ExternalSystemDto> {
    return this.http.post<ExternalSystemDto>(`${this.baseUrl}/${id}/suspend`, {});
  }

  rejectSystem(id: number): Observable<any> {
    return this.http.delete(`${this.baseUrl}/${id}`);
  }

  activateSystem(id: number): Observable<ExternalSystemDto> {
    return this.http.post<ExternalSystemDto>(`${this.baseUrl}/${id}/activate`, {});
  }

  getAllConversionRequests(): Observable<ConversionRequestDto[]> {
    return this.http.get<ConversionRequestDto[]>(this.conversionBaseUrl);
  }
}
