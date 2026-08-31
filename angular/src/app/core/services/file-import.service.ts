import { Injectable, inject, signal } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable, Subject } from 'rxjs';
import { environment } from '../../../environments/environment';
import { AuthService } from './auth.service';
import { ToastService } from './toast.service';
import { SignalrService } from '../../shared/services/signalr.service';

export interface ImportStatus {
  uploadId: string;
  receivedIndices: number[];
  totalChunks: number;
  percent: number;
  status: string;
}

/**
 * Global, resumable file import.
 *
 * Flow: `start` -> upload fixed-size `chunks` (the server remembers which it has) ->
 * if a chunk fails, re-query `status` and continue from the last received chunk
 * (not from the beginning) -> `complete` enqueues a background job -> a real-time
 * notification fires when processing finishes.
 */
@Injectable({ providedIn: 'root' })
export class FileImportService {
  private http = inject(HttpClient);
  private auth = inject(AuthService);
  private toast = inject(ToastService);
  private realtime = inject(SignalrService);

  readonly open = signal(false);
  readonly CHUNK_SIZE = 1024 * 1024; // 1 MB

  /** File types the import accepts. Keep in sync with the backend allow-list. */
  readonly ALLOWED_EXTENSIONS = ['.xlsx', '.xls', '.csv'] as const;
  readonly ACCEPT_ATTR = '.xlsx,.xls,.csv';

  private baseUrl = `${environment.apis.default.url}/api/file-import`;
  private importFinished = new Subject<string>();
  readonly importFinished$ = this.importFinished.asObservable();

  constructor() {
    // Bridge real-time import-done notifications into the service.
    this.realtime.notifications$.subscribe((raw: string) => {
      if (raw?.startsWith('IMPORT_DONE|')) {
        const fileName = raw.slice('IMPORT_DONE|'.length);
        this.toast.success(`File "${fileName}" was imported successfully.`, 'Import complete');
        this.importFinished.next(fileName);
      }
    });
  }

  openDialog(): void {
    // Make sure the notification hub is connected so completion alerts arrive.
    this.realtime.startConnections();
    this.open.set(true);
  }

  closeDialog(): void {
    this.open.set(false);
  }

  /** Frontend guard: reject anything that is not an Excel/CSV file before upload. */
  isFileTypeAllowed(file: File): boolean {
    const name = (file.name || '').toLowerCase();
    return this.ALLOWED_EXTENSIONS.some(ext => name.endsWith(ext));
  }

  start(fileName: string, contentType: string, totalSize: number, totalChunks: number): Observable<{ uploadId: string }> {
    return this.http.post<{ uploadId: string }>(`${this.baseUrl}/start`, {
      fileName,
      contentType,
      totalSize,
      totalChunks
    });
  }

  status(uploadId: string): Observable<ImportStatus> {
    return this.http.get<ImportStatus>(`${this.baseUrl}/status`, { params: { uploadId } });
  }

  complete(uploadId: string): Observable<{ enqueued: boolean }> {
    return this.http.post<{ enqueued: boolean }>(`${this.baseUrl}/complete`, { uploadId });
  }

  /**
   * Uploads the given chunk indices sequentially, posting cumulative progress (0-100).
   * Only missing indices are passed in, so a failed upload resumes from the last
   * received chunk. `baseBytes` is the amount already acknowledged by the server.
   */
  uploadChunks(
    file: File,
    uploadId: string,
    indices: number[],
    baseBytes: number,
    onProgress: (percent: number) => void,
    signal?: AbortSignal
  ): Promise<void> {
    const totalBytes = file.size;

    return new Promise((resolve, reject) => {
      let cursor = 0;
      let uploaded = baseBytes;

      const sendNext = (): void => {
        if (signal?.aborted) {
          reject(new DOMException('Upload aborted', 'AbortError'));
          return;
        }
        if (cursor >= indices.length) {
          resolve();
          return;
        }

        const index = indices[cursor++];
        const start = index * this.CHUNK_SIZE;
        const end = Math.min(start + this.CHUNK_SIZE, totalBytes);
        const blob = file.slice(start, end);

        const form = new FormData();
        form.append('uploadId', uploadId);
        form.append('index', String(index));
        form.append('chunk', blob, file.name);

        const xhr = new XMLHttpRequest();
        xhr.open('POST', `${this.baseUrl}/chunk`);
        const token = this.auth.getToken();
        if (token) {
          xhr.setRequestHeader('Authorization', `Bearer ${token}`);
        }

        xhr.upload.onprogress = (e: ProgressEvent) => {
          if (e.lengthComputable) {
            const current = uploaded + e.loaded;
            onProgress(Math.min(100, Math.round((current / totalBytes) * 100)));
          }
        };

        xhr.onload = () => {
          if (xhr.status >= 200 && xhr.status < 300) {
            uploaded += end - start;
            onProgress(Math.min(100, Math.round((uploaded / totalBytes) * 100)));
            sendNext();
          } else {
            reject(new Error(`Chunk ${index} failed with HTTP ${xhr.status}`));
          }
        };
        xhr.onerror = () => reject(new Error(`Chunk ${index} network error`));
        xhr.onabort = () => reject(new DOMException('Upload aborted', 'AbortError'));

        xhr.send(form);
      };

      sendNext();
    });
  }
}
