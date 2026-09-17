import { Injectable, inject } from '@angular/core';
import { HttpClient, HttpParams } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { ToastService } from '../toast.service';

export interface EmployeeImportJobDto {
  id: string;
  fileName: string;
  fileSize: number;
  status: number;
  statusText: string;
  totalRows: number;
  processedRows: number;
  successfulRows: number;
  failedRows: number;
  chunkSize: number;
  totalChunks: number;
  completedChunks: number;
  failedChunks: number;
  currentChunk: number;
  progressPercentage: number;
  startedAt?: string;
  completedAt?: string;
  cancelledAt?: string;
  creationTime: string;
  createdByUserName: string;
  retryCount: number;
  lastError?: string;
  canRetry: boolean;
  canCancel: boolean;
}

export interface EmployeeImportErrorDto {
  id: string;
  importJobId: string;
  chunkId?: string;
  rowNumber: number;
  columnName?: string;
  value?: string;
  errorMessage: string;
  createdAt: string;
}

export interface EmployeeImportStartResultDto {
  importJobId: string;
  status: string;
  fileName: string;
  fileSize: number;
  totalRows: number;
  totalChunks: number;
  chunkSize: number;
}

export interface PagedResult<T> {
  totalCount: number;
  items: T[];
}

@Injectable({ providedIn: 'root' })
export class EmployeeImportApiService {
  private http = inject(HttpClient);
  private toast = inject(ToastService);
  private baseUrl = `${environment.apis.default.url}/api/app/employee-import`;

  downloadTemplate(): void {
    this.http.get(`${this.baseUrl}/template`, { responseType: 'blob' }).subscribe({
      next: (blob) => {
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = 'EmployeeImportTemplate.xlsx';
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
      },
      error: (err) => {
        console.error('Failed to download template', err);
      }
    });
  }

  downloadImportFile(id: string, fileName?: string): void {
    this.http.get(`${this.baseUrl}/${id}/file`, { responseType: 'blob', observe: 'response' }).subscribe({
      next: (res) => {
        const blob = res.body;
        if (!blob) return;
        const url = window.URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url;
        a.download = fileName || `Imported_Employees_${id.slice(0, 8)}.xlsx`;
        document.body.appendChild(a);
        a.click();
        document.body.removeChild(a);
        window.URL.revokeObjectURL(url);
      },
      error: (err) => {
        if (err.error instanceof Blob) {
          const reader = new FileReader();
          reader.onload = () => {
            try {
              const json = JSON.parse(reader.result as string);
              const msg = json?.error?.message || json?.error?.details || 'Failed to download file';
              this.toast.error(msg);
            } catch {
              this.toast.error('Failed to download imported file');
            }
          };
          reader.readAsText(err.error);
        } else {
          this.toast.error(err?.error?.error?.message || err?.message || 'Failed to download imported file');
        }
      }
    });
  }

  importEmployees(file: File): Observable<EmployeeImportStartResultDto> {
    const formData = new FormData();
    formData.append('file', file, file.name);
    return this.http.post<EmployeeImportStartResultDto>(`${this.baseUrl}/import-employees`, formData);
  }

  getHistory(maxResultCount = 20, skipCount = 0, search = ''): Observable<PagedResult<EmployeeImportJobDto>> {
    let params = new HttpParams()
      .set('maxResultCount', maxResultCount.toString())
      .set('skipCount', skipCount.toString());
    if (search) params = params.set('search', search);

    return this.http.get<PagedResult<EmployeeImportJobDto>>(this.baseUrl, { params });
  }

  getActiveJobs(): Observable<{ items: EmployeeImportJobDto[] }> {
    return this.http.get<{ items: EmployeeImportJobDto[] }>(`${this.baseUrl}/active`);
  }

  getJobStatus(id: string): Observable<EmployeeImportJobDto> {
    return this.http.get<EmployeeImportJobDto>(`${this.baseUrl}/${id}/status`);
  }

  getJobErrors(id: string, maxResultCount = 100, skipCount = 0): Observable<PagedResult<EmployeeImportErrorDto>> {
    const params = new HttpParams()
      .set('maxResultCount', maxResultCount.toString())
      .set('skipCount', skipCount.toString());
    return this.http.get<PagedResult<EmployeeImportErrorDto>>(`${this.baseUrl}/${id}/errors`, { params });
  }

  retryJob(id: string): Observable<EmployeeImportJobDto> {
    return this.http.post<EmployeeImportJobDto>(`${this.baseUrl}/${id}/retry`, {});
  }

  cancelJob(id: string): Observable<EmployeeImportJobDto> {
    return this.http.post<EmployeeImportJobDto>(`${this.baseUrl}/${id}/cancel`, {});
  }
}
