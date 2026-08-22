import { Injectable, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { EnvironmentService } from '@abp/ng.core';
import { Observable } from 'rxjs';

export interface FolderDto {
  id: string;
  name: string;
  parentId?: string;
}

export interface DocumentDto {
  id: string;
  title: string;
  extension: string;
  sizeBytes: number;
  contentType: string;
  folderId?: string;
}

@Injectable({ providedIn: 'root' })
export class DocumentService {
  private http = inject(HttpClient);
  private env = inject(EnvironmentService);

  private get baseUrl(): string {
    return this.env.getEnvironment().apis.default.url + '/api/documents';
  }

  // Folders
  getFolders(parentId?: string): Observable<FolderDto[]> {
    const params: any = {};
    if (parentId) params['parentId'] = parentId;
    return this.http.get<FolderDto[]>(`${this.baseUrl}/folders`, { params });
  }

  createFolder(name: string, parentId?: string): Observable<FolderDto> {
    return this.http.post<FolderDto>(`${this.baseUrl}/folders`, { name, parentId });
  }

  deleteFolder(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/folders/${id}`);
  }

  // Documents
  getDocuments(folderId?: string): Observable<DocumentDto[]> {
    const params: any = {};
    if (folderId) params['folderId'] = folderId;
    return this.http.get<DocumentDto[]>(this.baseUrl, { params });
  }

  uploadFile(file: File, folderId?: string): Observable<DocumentDto> {
    const formData = new FormData();
    formData.append('file', file);
    if (folderId) formData.append('folderId', folderId);
    return this.http.post<DocumentDto>(`${this.baseUrl}/upload`, formData);
  }

  downloadFile(id: string): Observable<Blob> {
    return this.http.get(`${this.baseUrl}/${id}/download`, { responseType: 'blob' });
  }

  deleteDocument(id: string): Observable<void> {
    return this.http.delete<void>(`${this.baseUrl}/${id}`);
  }

  formatBytes(bytes: number): string {
    if (bytes === 0) return '0 B';
    const k = 1024;
    const sizes = ['B', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(k));
    return parseFloat((bytes / Math.pow(k, i)).toFixed(1)) + ' ' + sizes[i];
  }
}
