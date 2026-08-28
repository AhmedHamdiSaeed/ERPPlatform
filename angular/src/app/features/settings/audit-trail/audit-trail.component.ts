import { Component, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { AuditLogEntry } from '../../../core/models/erp-models';
import { SharedApiService } from '../../../core/services/api/shared-api.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-audit-trail',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './audit-trail.component.html'
})
export class AuditTrailComponent {
  private sharedApi = inject(SharedApiService);
  private toast = inject(ToastService);

  auditLogs = signal<AuditLogEntry[]>([]);
  searchQuery = signal('');
  selectedAction = signal<string>('ALL');
  selectedEntity = signal<string>('ALL');

  showDiffModal = signal(false);
  selectedLog: AuditLogEntry | null = null;

  filteredLogs = computed(() => {
    return this.auditLogs().filter(log => {
      const matchSearch = !this.searchQuery() ||
        log.entityName.toLowerCase().includes(this.searchQuery().toLowerCase()) ||
        log.userName.toLowerCase().includes(this.searchQuery().toLowerCase()) ||
        log.entityId.toLowerCase().includes(this.searchQuery().toLowerCase());

      const matchAction = this.selectedAction() === 'ALL' || log.action === this.selectedAction();
      const matchEntity = this.selectedEntity() === 'ALL' || log.entityName === this.selectedEntity();

      return matchSearch && matchAction && matchEntity;
    });
  });

  entityTypes = computed(() => {
    const types = new Set(this.auditLogs().map(l => l.entityName));
    return Array.from(types);
  });

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

  formatJson(jsonStr?: string): string {
    if (!jsonStr || jsonStr === '{}') return '{}';
    try {
      return JSON.stringify(JSON.parse(jsonStr), null, 2);
    } catch {
      return jsonStr;
    }
  }
}
