import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { AuditLogEntry } from '../../../core/models/erp-models';
import { SharedApiService } from '../../../core/services/api/shared-api.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-audit-trail',
  standalone: true,
  imports: [CommonModule],
  templateUrl: './audit-trail.component.html'
})
export class AuditTrailComponent {
  private sharedApi = inject(SharedApiService);
  private toast = inject(ToastService);

  auditLogs = signal<AuditLogEntry[]>([]);

  showDiffModal = signal(false);
  selectedLog: AuditLogEntry | null = null;

  constructor() {
    this.loadLogs();
  }

  async loadLogs() {
    try {
      this.auditLogs.set(await this.sharedApi.getAuditLogs());
    } catch (e) {
      console.error('Failed to load audit logs', e);
      this.toast.error('Could not load audit trail from the server.');
    }
  }

  inspectChanges(log: AuditLogEntry) {
    this.selectedLog = log;
    this.showDiffModal.set(true);
  }
}
