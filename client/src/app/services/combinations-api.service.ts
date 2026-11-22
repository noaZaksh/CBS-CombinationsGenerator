import { HttpClient, HttpParams } from '@angular/common/http';
import { Injectable, inject } from '@angular/core';
import { Observable } from 'rxjs';

import { AppSettings } from '../app.settings';
import { NextResponse, PageResponse, StartResponse } from '../models/combinations.models';

/**
 * Talks to the server. Only HTTP calls – no logic and no state.
 */
@Injectable({ providedIn: 'root' })
export class CombinationsApiService {
  private readonly http = inject(HttpClient);
  private readonly baseUrl = AppSettings.apiUrl;

  /** StartAPI */
  start(n: number): Observable<StartResponse> {
    return this.http.post<StartResponse>(`${this.baseUrl}/start`, { n });
  }

  /** GetNextAPI */
  getNext(sessionId: string): Observable<NextResponse> {
    return this.http.post<NextResponse>(`${this.baseUrl}/${sessionId}/next`, null);
  }

  /** GetAllAPI – page number is sent as text, because it can be bigger than a JavaScript number. */
  getAll(sessionId: string, pageSize: number, pageNumber: bigint): Observable<PageResponse> {
    const params = new HttpParams().set('pageSize', pageSize).set('pageNumber', pageNumber.toString());
    return this.http.get<PageResponse>(`${this.baseUrl}/${sessionId}/all`, { params });
  }

  /** "Back" button */
  back(sessionId: string): Observable<NextResponse> {
    return this.http.post<NextResponse>(`${this.baseUrl}/${sessionId}/back`, null);
  }
}
