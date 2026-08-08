import { Component, signal } from '@angular/core';
import { WorkflowExecutionLog } from '../../../core/models/erp-models';
import { MOCK_EXECUTION_LOGS } from '../../../core/mock/mock-data';

@Component({
  selector: 'app-execution-history',
  standalone: true,
  imports: [],
  templateUrl: './execution-history.component.html'
})
export class ExecutionHistoryComponent {
  logs = signal<WorkflowExecutionLog[]>(MOCK_EXECUTION_LOGS);
  selectedLog = signal<WorkflowExecutionLog | null>(MOCK_EXECUTION_LOGS[0]);
}
