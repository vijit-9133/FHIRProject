import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';

@Injectable({
  providedIn: 'root'
})
export class FhirApiService {
  private baseUrl = 'http://localhost:5078/api/fhir';

  constructor(private http: HttpClient) {}

  convertToFhir(data: any): Observable<any> {
    return this.http.post(`${this.baseUrl}/convert`, data);
  }

  getConversionHistory(): Observable<any[]> {
    return this.http.get<any[]>(`${this.baseUrl}/history`);
  }

  getConversionRequest(id: number): Observable<any> {
    return this.http.get(`${this.baseUrl}/request/${id}`);
  }

  getFhirResource(conversionRequestId: number): Observable<any> {
    return this.http.get(`${this.baseUrl}/${conversionRequestId}`);
  }
}