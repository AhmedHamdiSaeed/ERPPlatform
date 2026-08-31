import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { firstValueFrom } from 'rxjs';
import { FileImportService } from '../../../core/services/file-import.service';

type ImportState = 'idle' | 'ready' | 'uploading' | 'failed' | 'processing' | 'done' | 'invalid';

interface StoredSession {
  uploadId: string;
  fileName: string;
  fileSize: number;
  totalChunks: number;
}

const STORAGE_KEY = 'erp:file-import-session';

/**
 * Global, resumable file import dialog.
 *
 * - Opens from the header button (available on every page).
 * - Splits the file into fixed 1 MB chunks and uploads them sequentially.
 * - On a failed request it keeps the upload id and, on retry, re-queries the
 *   server status and uploads ONLY the missing chunks — so a restart at 30%
 *   continues from 30%, not 0%.
 * - When the backend background job finishes it receives a real-time
 *   "IMPORT_DONE|<file>" SignalR notification and flips to the success state.
 */
@Component({
  selector: 'app-file-import-dialog',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './file-import-dialog.component.html'
})
export class FileImportDialogComponent {
  private fileImport = inject(FileImportService);

  readonly open = this.fileImport.open;
  readonly accept = this.fileImport.ACCEPT_ATTR;

  file = signal<File | null>(null);
  uploadId = signal<string | null>(null);
  totalChunks = signal(1);
  state = signal<ImportState>('idle');
  percent = signal(0);
  errorMsg = signal<string | null>(null);
  resumeAvailable = signal(false);
  resumePercent = signal(0);
  dragOver = signal(false);

  private controller: AbortController | null = null;

  constructor() {
    // Finalize when the backend background job finishes and pushes the notification.
    this.fileImport.importFinished$.subscribe(() => {
      this.state.set('done');
      this.percent.set(100);
      this.clearStoredSession();
    });
  }

  get isUploading(): boolean {
    return this.state() === 'uploading';
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    const f = input.files?.[0];
    if (f) this.selectFile(f);
    input.value = '';
  }

  onDrop(event: DragEvent): void {
    event.preventDefault();
    this.dragOver.set(false);
    const f = event.dataTransfer?.files?.[0];
    if (f) this.selectFile(f);
  }

  onDragOver(event: DragEvent): void {
    event.preventDefault();
    this.dragOver.set(true);
  }

  onDragLeave(): void {
    this.dragOver.set(false);
  }

  private selectFile(f: File): void {
    this.errorMsg.set(null);

    // Reject unsupported file types immediately. The backend enforces the same
    // rule (extension + binary signature), but failing fast here is better UX.
    if (!this.fileImport.isFileTypeAllowed(f)) {
      this.file.set(f);
      this.uploadId.set(null);
      this.resumeAvailable.set(false);
      this.percent.set(0);
      this.state.set('invalid');
      this.errorMsg.set('Only Excel (.xlsx, .xls) or CSV files can be imported.');
      return;
    }

    this.file.set(f);
    this.totalChunks.set(Math.max(1, Math.ceil(f.size / this.fileImport.CHUNK_SIZE)));

    // Resume a previous, interrupted session for the exact same file.
    const stored = this.readStoredSession();
    if (stored && stored.fileName === f.name && stored.fileSize === f.size) {
      this.uploadId.set(stored.uploadId);
      this.resumeAvailable.set(true);
      this.state.set('ready');
      this.refreshResumeProgress();
    } else {
      this.uploadId.set(null);
      this.resumeAvailable.set(false);
      this.percent.set(0);
      this.state.set('ready');
    }
  }

  private refreshResumeProgress(): void {
    const id = this.uploadId();
    if (!id) return;
    this.fileImport.status(id).subscribe({
      next: (st) => {
        const pct = st.totalChunks
          ? Math.round((st.receivedIndices.length / st.totalChunks) * 100)
          : 0;
        this.resumePercent.set(pct);
        this.percent.set(pct);
      },
      error: () => {
        // Server session already expired; start fresh.
        this.resumeAvailable.set(false);
        this.percent.set(0);
      }
    });
  }

  async begin(): Promise<void> {
    const file = this.file();
    if (!file) return;

    // 1. Ensure we have a server-side upload session.
    let uploadId = this.uploadId();
    if (!uploadId) {
      try {
        const res = await firstValueFrom(
          this.fileImport.start(file.name, file.type, file.size, this.totalChunks())
        );
        uploadId = res.uploadId;
        this.uploadId.set(uploadId);
        this.storeSession({
          uploadId,
          fileName: file.name,
          fileSize: file.size,
          totalChunks: this.totalChunks()
        });
      } catch {
        this.fail('Could not start the import session. Please try again.');
        return;
      }
    }

    // 2. Work out which chunks still need to go up.
    let indices: number[];
    let baseBytes = 0;

    if (this.resumeAvailable()) {
      try {
        const st = await firstValueFrom(this.fileImport.status(uploadId));
        const received = new Set(st.receivedIndices);
        indices = [];
        for (let i = 0; i < this.totalChunks(); i++) {
          if (!received.has(i)) indices.push(i);
        }
        // Bytes the server already holds -> progress starts from here.
        baseBytes = st.receivedIndices.reduce((sum, i) => {
          const start = i * this.fileImport.CHUNK_SIZE;
          const end = Math.min(start + this.fileImport.CHUNK_SIZE, file.size);
          return sum + (end - start);
        }, 0);
        this.resumePercent.set(st.percent);
        this.percent.set(st.percent);
      } catch {
        this.fail('Could not read upload status. Please try again.');
        return;
      }
    } else {
      indices = Array.from({ length: this.totalChunks() }, (_, i) => i);
    }

    // 3. Nothing missing? Go straight to finalization.
    if (indices.length === 0) {
      this.state.set('processing');
      try {
        await firstValueFrom(this.fileImport.complete(uploadId));
      } catch {
        this.fail('Could not finalize the import. Please try again.');
      }
      return;
    }

    // 4. Upload the (missing) chunks.
    this.state.set('uploading');
    this.errorMsg.set(null);
    this.controller = new AbortController();

    try {
      await this.fileImport.uploadChunks(
        file,
        uploadId,
        indices,
        baseBytes,
        (pct) => this.percent.set(pct),
        this.controller.signal
      );
      // 5. Hand off to the backend background job.
      this.state.set('processing');
      await firstValueFrom(this.fileImport.complete(uploadId));
      // Real-time notification will flip this to 'done'.
    } catch (err: any) {
      if (err?.name === 'AbortError') return; // user cancelled
      this.fail(err?.message || 'Upload failed. You can resume from where it stopped.');
    }
  }

  private fail(msg: string): void {
    this.state.set('failed');
    this.errorMsg.set(msg);
    this.resumeAvailable.set(true);
  }

  cancel(): void {
    this.controller?.abort();
    this.controller = null;
    // If we were mid-upload, let the user resume later instead of losing progress.
    if (this.state() === 'uploading') {
      this.state.set('ready');
      this.resumeAvailable.set(true);
    }
    this.fileImport.closeDialog();
  }

  removeFile(): void {
    this.controller?.abort();
    this.controller = null;
    this.file.set(null);
    this.uploadId.set(null);
    this.resumeAvailable.set(false);
    this.percent.set(0);
    this.state.set('idle');
    this.clearStoredSession();
  }

  formatBytes(bytes: number): string {
    if (!bytes) return '0 B';
    const units = ['B', 'KB', 'MB', 'GB'];
    const i = Math.floor(Math.log(bytes) / Math.log(1024));
    return `${(bytes / Math.pow(1024, i)).toFixed(1)} ${units[i]}`;
  }

  private storeSession(s: StoredSession): void {
    try {
      sessionStorage.setItem(STORAGE_KEY, JSON.stringify(s));
    } catch {
      /* ignore */
    }
  }

  private readStoredSession(): StoredSession | null {
    try {
      const raw = sessionStorage.getItem(STORAGE_KEY);
      return raw ? (JSON.parse(raw) as StoredSession) : null;
    } catch {
      return null;
    }
  }

  private clearStoredSession(): void {
    try {
      sessionStorage.removeItem(STORAGE_KEY);
    } catch {
      /* ignore */
    }
  }
}
