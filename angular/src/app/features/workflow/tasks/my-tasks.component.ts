import { Component, inject, signal } from '@angular/core';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { WorkflowTask } from '../../../core/models/erp-models';
import { WorkflowApiService } from '../../../core/services/api/workflow-api.service';
import { ToastService } from '../../../core/services/toast.service';
import { TranslatePipe } from 'src/app/shared/pipes/translate.pipe';

@Component({
  selector: 'app-my-tasks',
  standalone: true,
  imports: [FormsModule, RouterModule,TranslatePipe],
  templateUrl: './my-tasks.component.html'
})
export class MyTasksComponent {
  private toast = inject(ToastService);
  private workflowApi = inject(WorkflowApiService);

  tasks = signal<WorkflowTask[]>([]);

  constructor() {
    this.loadTasks();
  }

  async loadTasks() {
    try {
      this.tasks.set(await this.workflowApi.getTasks());
    } catch (e) {
      console.error('Failed to load workflow tasks', e);
      this.toast.error('Could not load workflow tasks from the server.');
    }
  }

  async approve(id: string) {
    try {
      await this.workflowApi.approveTask(id);
      await this.loadTasks();
      this.toast.success('Workflow approval task completed successfully.', 'Task Approved');
    } catch (e) {
      console.error('Failed to approve task', e);
      this.toast.error('Failed to approve the workflow task.', 'Approval Failed');
    }
  }

  async reject(id: string) {
    try {
      await this.workflowApi.rejectTask(id);
      await this.loadTasks();
      this.toast.error('Workflow task rejected.', 'Task Rejected');
    } catch (e) {
      console.error('Failed to reject task', e);
      this.toast.error('Failed to reject the workflow task.');
    }
  }

  requestChanges(id: string) {
    this.toast.warning('Requested modifications sent back to task submitter.', 'Changes Requested');
  }
}
