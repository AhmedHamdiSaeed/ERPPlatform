import { Component, inject, signal } from '@angular/core';
import { TranslatePipe } from '../../../shared/pipes/translate.pipe';
import { AppDatePipe } from '../../../shared/pipes/app-date.pipe';
import { WorkflowExecutionLog } from '../../../core/models/erp-models';
import { WorkflowExecutionApiService } from '../../../core/services/api/workflow-execution-api.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-execution-history',
  standalone: true,
  imports: [TranslatePipe, AppDatePipe],
  templateUrl: './execution-history.component.html'
})
export class ExecutionHistoryComponent {
  private execApi = inject(WorkflowExecutionApiService);
  private toast = inject(ToastService);

  logs = signal<WorkflowExecutionLog[]>([]);
  selectedLog = signal<WorkflowExecutionLog | null>(null);
  loading = signal(false);

  constructor() {
    this.load();
  }

  async load() {
    this.loading.set(true);
    try {
      const list = await this.execApi.getExecutions();
      this.logs.set(list);
      this.selectedLog.set(list[0] ?? null);
    } catch (e) {
      console.error('Failed to load execution logs', e);
      this.toast.error('Could not load workflow execution history from the server.');
    } finally {
      this.loading.set(false);
    }
  }
}
