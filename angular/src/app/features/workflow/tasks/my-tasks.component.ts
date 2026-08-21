import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { WorkflowTask } from '../../../core/models/erp-models';
import { MOCK_TASKS } from '../../../core/mock/mock-data';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-my-tasks',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './my-tasks.component.html'
})
export class MyTasksComponent {
  private toast = inject(ToastService);

  tasks = signal<WorkflowTask[]>(MOCK_TASKS);

  approve(id: string) {
    this.tasks.update(list => list.filter(t => t.id !== id));
    this.toast.success('Workflow approval task completed successfully.', 'Task Approved');
  }

  reject(id: string) {
    this.tasks.update(list => list.filter(t => t.id !== id));
    this.toast.error('Workflow task rejected.', 'Task Rejected');
  }

  requestChanges(id: string) {
    this.toast.warning('Requested modifications sent back to task submitter.', 'Changes Requested');
  }
}
