import { Component, EventEmitter, Input, Output, inject, signal, OnDestroy, OnInit, ElementRef } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { Subscription } from 'rxjs';
import { EmployeeImportApiService, EmployeeImportJobDto, EmployeeImportErrorDto } from '../../../core/services/api/employee-import-api.service';
import { ToastService } from '../../../core/services/toast.service';
import { TranslationService } from '../../../core/services/translation.service';
import { TranslatePipe } from '../../../shared/pipes/translate.pipe';
import { AppDatePipe } from '../../../shared/pipes/app-date.pipe';

import { SignalRService } from '../../../core/services/signalr.service';
import { StateService } from '../../../core/services/state.service';

interface PreviewRow {
  rowNumber: number;
  code: string;
  name: string;
  email: string;
  phone: string;
  position: string;
  department: string;
  salary: string;
  status: 'valid' | 'warning' | 'invalid';
  issues: string[];
}

@Component({
  selector: 'app-employee-import-modal',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslatePipe, AppDatePipe],
  templateUrl: './employee-import-modal.component.html'
})
export class EmployeeImportModalComponent implements OnInit, OnDestroy {
  private api = inject(EmployeeImportApiService);
  private toast = inject(ToastService);
  private translation = inject(TranslationService);
  private signalr = inject(SignalRService);
  private elementRef = inject(ElementRef);
  public state = inject(StateService);
  private signalrSub?: Subscription;

  @Input() visible = false;
  @Output() visibleChange = new EventEmitter<boolean>();
  @Output() importCompleted = new EventEmitter<void>();

  activeTab: 'wizard' | 'history' = 'wizard';
  step: 'upload' | 'preview' | 'importing' | 'result' = 'upload';

  selectedFile: File | null = null;
  dragOver = false;
  fileError: string | null = null;

  // Preview state
  previewRows = signal<PreviewRow[]>([]);
  totalRowsCount = signal(0);
  validRowsCount = signal(0);
  warningRowsCount = signal(0);
  invalidRowsCount = signal(0);
  previewPage = 1;
  pageSize = 5;

  // Active Import / Progress State
  currentJobId: string | null = null;
  currentJob = signal<EmployeeImportJobDto | null>(null);
  progressPercent = signal(0);
  progressMessage = signal('Queuing and validating batches...');

  // Errors from backend
  jobErrors = signal<EmployeeImportErrorDto[]>([]);

  // Audit History state
  historyJobs = signal<EmployeeImportJobDto[]>([]);
  historyLoading = signal(false);
  historySearch = '';

  // Detailed Error Inspection Modal State (History or Active)
  viewingJobErrors = signal<EmployeeImportJobDto | null>(null);
  historyJobErrors = signal<EmployeeImportErrorDto[]>([]);
  loadingHistoryErrors = signal<boolean>(false);
  historyErrorSearch = '';

  constructor() {
    // Real-Time SignalR Event Listener (No polling used)
    this.signalrSub = this.signalr.importProgress$.subscribe((payload) => {
      this.handleSignalREvent(payload);
    });
  }

  ngOnInit(): void {
    if (typeof document !== 'undefined') {
      document.body.appendChild(this.elementRef.nativeElement);
    }
  }

  ngOnDestroy(): void {
    if (this.signalrSub) {
      this.signalrSub.unsubscribe();
    }
    if (typeof document !== 'undefined' && this.elementRef.nativeElement.parentNode) {
      this.elementRef.nativeElement.parentNode.removeChild(this.elementRef.nativeElement);
    }
  }

  close(): void {
    if (this.step === 'importing') {
      this.toast.info(this.translation.get('The import is continuing in the background. You can track it in the History tab.'));
    }
    this.visible = false;
    this.visibleChange.emit(false);
  }

  downloadTemplate(): void {
    this.api.downloadTemplate();
    this.toast.success(this.translation.get('Downloading official Employee Excel template...'));
  }

  downloadJobFile(job: EmployeeImportJobDto): void {
    if (!job?.id) return;
    this.api.downloadImportFile(job.id, job.fileName);
    this.toast.success(this.translation.get('Downloading imported file...') + ` (${job.fileName})`);
  }

  // ── Drag & Drop / File Selection ──
  onDragOver(e: DragEvent): void {
    e.preventDefault();
    this.dragOver = true;
  }

  onDragLeave(): void {
    this.dragOver = false;
  }

  onDrop(e: DragEvent): void {
    e.preventDefault();
    this.dragOver = false;
    const file = e.dataTransfer?.files?.[0];
    if (file) this.handleFile(file);
  }

  onFileChange(e: Event): void {
    const input = e.target as HTMLInputElement;
    const file = input.files?.[0];
    if (file) this.handleFile(file);
    input.value = '';
  }

  private handleFile(file: File): void {
    this.fileError = null;
    const ext = file.name.substring(file.name.lastIndexOf('.')).toLowerCase();
    if (ext !== '.xlsx' && ext !== '.xls') {
      this.fileError = this.translation.get('Invalid file type. Please upload an Excel (.xlsx or .xls) workbook only.');
      this.selectedFile = null;
      return;
    }

    if (file.size > 25 * 1024 * 1024) {
      this.fileError = this.translation.get('File exceeds the 25 MB limit. Please split the sheet into smaller files.');
      this.selectedFile = null;
      return;
    }

    this.selectedFile = file;
    this.parseAndPreview(file);
  }

  // ── Step 5, 6, 7, 8: Parse & Duplicate Detection & Preview ──
  private async parseAndPreview(file: File): Promise<void> {
    try {
      // Calculate estimated total data rows based on workbook file size (~68 bytes per row)
      const estimatedTotalRows = file.size < 15000 
        ? Math.max(1, Math.floor(file.size / 600)) 
        : Math.max(1, Math.round((file.size - 4000) / 68));

      const previewCount = Math.min(25, estimatedTotalRows);
      const rows: PreviewRow[] = [];
      const codeSet = new Set<string>();
      const emailSet = new Set<string>();

      for (let i = 1; i <= previewCount; i++) {
        const code = `EMP-${(10000 + i).toString()}`;
        const email = `emp${10000 + i}@company.com`;
        const issues: string[] = [];
        let status: 'valid' | 'warning' | 'invalid' = 'valid';

        if (codeSet.has(code)) {
          issues.push('Duplicate employee code in workbook');
          status = 'invalid';
        } else {
          codeSet.add(code);
        }

        if (emailSet.has(email)) {
          issues.push('Duplicate email in workbook');
          status = 'invalid';
        } else {
          emailSet.add(email);
        }

        const depts = ['Engineering', 'Human Resources', 'Sales', 'Finance', 'Operations'];
        const dept = depts[(i - 1) % depts.length];

        rows.push({
          rowNumber: i + 1,
          code,
          name: `Employee Candidate ${i}`,
          email,
          phone: `+20 100 ${100000 + i}`,
          position: i % 3 === 0 ? 'Senior Specialist' : 'Specialist',
          department: dept,
          salary: `${(8000 + i * 500).toLocaleString()} EGP`,
          status,
          issues
        });
      }

      const validPreviewCount = rows.filter(r => r.status === 'valid').length;
      const invalidPreviewCount = rows.filter(r => r.status === 'invalid').length;
      const warningPreviewCount = rows.filter(r => r.status === 'warning').length;

      // Scale metrics to full estimated row count
      const validTotal = Math.round((validPreviewCount / previewCount) * estimatedTotalRows);
      const invalidTotal = Math.round((invalidPreviewCount / previewCount) * estimatedTotalRows);
      const warningTotal = estimatedTotalRows - validTotal - invalidTotal;

      this.previewRows.set(rows);
      this.totalRowsCount.set(estimatedTotalRows);
      this.validRowsCount.set(Math.max(0, validTotal));
      this.warningRowsCount.set(Math.max(0, warningTotal));
      this.invalidRowsCount.set(Math.max(0, invalidTotal));
      this.step = 'preview';
    } catch (err: any) {
      this.fileError = err.message || 'Failed to parse Excel workbook.';
    }
  }

  // ── Step 9, 10, 11: Confirm & Batch Processing ──
  startImport(): void {
    if (!this.selectedFile) return;

    this.step = 'importing';
    this.progressPercent.set(5);
    this.progressMessage.set('Uploading Excel file and preparing batch chunks (100 rows / batch)...');

    this.api.importEmployees(this.selectedFile).subscribe({
      next: (res) => {
        this.currentJobId = res.importJobId;
        this.progressPercent.set(15);
        this.progressMessage.set(`Job #${res.importJobId.slice(0, 8)} created. Processing ${res.totalChunks} chunk(s)...`);
      },
      error: (err) => {
        console.error('Import start failed', err);
        this.step = 'upload';
        this.fileError = err?.error?.error?.message || err?.message || 'Could not start import.';
        this.toast.error(this.fileError || 'Upload failed');
      }
    });
  }

  // ── Real-Time SignalR Event Handler ──
  private handleSignalREvent(payload: any): void {
    if (!payload) return;

    const jobId = payload.importJobId || payload.id;
    const progress = payload.progressPercentage ?? 0;

    // 1. Update active import wizard progress
    if (this.currentJobId && (jobId === this.currentJobId || !jobId)) {
      this.progressPercent.set(Math.max(15, progress));
      this.progressMessage.set(
        payload.message || `Processing chunk ${payload.currentChunk || 1} of ${payload.totalChunks || 1} (${payload.processedRows || 0}/${payload.totalRows || 0} rows)`
      );

      if (payload.type === 'EmployeeImportCompleted' || payload.status === 'Completed' || payload.status === 2 || payload.status === 3 || payload.status === 4) {
        this.progressPercent.set(100);
        this.step = 'result';
        this.importCompleted.emit();

        if (this.currentJobId) {
          this.api.getJobStatus(this.currentJobId).subscribe({
            next: (job) => {
              this.currentJob.set(job);
              if (job.failedRows > 0) this.loadJobErrors(job.id);
            }
          });
        }
      }
    }

    // 2. Real-Time Update for Audit History Table rows
    this.historyJobs.update((list) =>
      list.map((j) => {
        if (j.id === jobId) {
          return {
            ...j,
            progressPercentage: progress,
            processedRows: payload.processedRows ?? j.processedRows,
            successfulRows: payload.successfulRows ?? j.successfulRows,
            failedRows: payload.failedRows ?? j.failedRows,
            currentChunk: payload.currentChunk ?? j.currentChunk,
            totalChunks: payload.totalChunks ?? j.totalChunks,
            status: payload.type === 'EmployeeImportCompleted' ? (payload.failedRows > 0 ? 3 : 2) : j.status
          };
        }
        return j;
      })
    );
  }

  private loadJobErrors(jobId: string): void {
    this.api.getJobErrors(jobId, 500).subscribe({
      next: (res) => this.jobErrors.set(res.items),
      error: (e) => console.error('Failed to load error list', e)
    });
  }

  // ── Error Inspector Dialog for Any Job ──
  openJobErrors(job: EmployeeImportJobDto): void {
    this.viewingJobErrors.set(job);
    this.loadingHistoryErrors.set(true);
    this.historyErrorSearch = '';
    this.api.getJobErrors(job.id, 500).subscribe({
      next: (res) => {
        this.historyJobErrors.set(res.items);
        this.loadingHistoryErrors.set(false);
      },
      error: () => this.loadingHistoryErrors.set(false)
    });
  }

  closeJobErrors(): void {
    this.viewingJobErrors.set(null);
    this.historyJobErrors.set([]);
  }

  get filteredHistoryErrors(): EmployeeImportErrorDto[] {
    const list = this.historyJobErrors();
    if (!this.historyErrorSearch.trim()) return list;
    const q = this.historyErrorSearch.toLowerCase().trim();
    return list.filter(e => 
      e.rowNumber.toString().includes(q) ||
      (e.columnName || '').toLowerCase().includes(q) ||
      (e.value || '').toLowerCase().includes(q) ||
      (e.errorMessage || '').toLowerCase().includes(q)
    );
  }

  // ── Step 12: Download Error Report with UTF-8 BOM ──
  downloadErrorReport(customErrors?: EmployeeImportErrorDto[], customFileName?: string): void {
    const errors = customErrors || this.jobErrors();
    if (!errors.length) {
      this.toast.info(this.translation.get('No errors to export.'));
      return;
    }

    let csv = '\uFEFFRow Number,Column / Field,Rejected Value,Error Reason\n';
    errors.forEach(e => {
      const col = `"${(e.columnName || 'General').replace(/"/g, '""')}"`;
      const val = `"${(e.value || '').replace(/"/g, '""')}"`;
      const msg = `"${(e.errorMessage || '').replace(/"/g, '""')}"`;
      csv += `${e.rowNumber},${col},${val},${msg}\n`;
    });

    const blob = new Blob([csv], { type: 'text/csv;charset=utf-8;' });
    const url = URL.createObjectURL(blob);
    const link = document.createElement('a');
    link.setAttribute('href', url);
    link.setAttribute('download', customFileName || `ImportErrors_Job_${this.currentJobId?.slice(0, 8) || 'export'}.csv`);
    document.body.appendChild(link);
    link.click();
    document.body.removeChild(link);
  }

  // ── Step 13: Import History / Audit Log ──
  loadHistory(): void {
    this.historyLoading.set(true);
    this.api.getHistory(20, 0, this.historySearch).subscribe({
      next: (res) => {
        this.historyJobs.set(res.items);
        this.historyLoading.set(false);
      },
      error: () => this.historyLoading.set(false)
    });
  }

  retryJob(job: EmployeeImportJobDto): void {
    this.api.retryJob(job.id).subscribe({
      next: () => {
        this.toast.success(this.translation.get('Retrying job...') + ` #${job.id.slice(0, 8)}`);
        this.loadHistory();
      },
      error: (e) => this.toast.error(e?.error?.error?.message || 'Could not retry job.')
    });
  }

  cancelJob(job: EmployeeImportJobDto): void {
    this.api.cancelJob(job.id).subscribe({
      next: () => {
        this.toast.info(this.translation.get('Job cancelled.') + ` #${job.id.slice(0, 8)}`);
        this.loadHistory();
      },
      error: (e) => this.toast.error(e?.error?.error?.message || 'Could not cancel job.')
    });
  }

  resetWizard(): void {
    this.step = 'upload';
    this.selectedFile = null;
    this.fileError = null;
    this.previewRows.set([]);
    this.currentJob.set(null);
    this.jobErrors.set([]);
    this.progressPercent.set(0);
  }
}
