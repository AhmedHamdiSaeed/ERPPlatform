import { Component, inject, signal } from '@angular/core';
import { RouterModule } from '@angular/router';
import { FormsModule } from '@angular/forms';
import { WorkflowTask } from '../../../core/models/erp-models';
import { WorkflowApiService } from '../../../core/services/api/workflow-api.service';
import { ToastService } from '../../../core/services/toast.service';
import { DialogService } from '../../../core/services/dialog.service';
import { TranslatePipe } from 'src/app/shared/pipes/translate.pipe';

@Component({
  selector: 'app-my-tasks',
  standalone: true,
  imports: [FormsModule, RouterModule, TranslatePipe],
  templateUrl: './my-tasks.component.html'
})
export class MyTasksComponent {
  private toast = inject(ToastService);
  private dialog = inject(DialogService);
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

  async requestChanges(id: string) {
    const comments = await this.dialog.prompt({
      title: 'Request Task Modifications',
      message: 'Please describe the required changes or missing information:',
      placeholder: 'e.g. Please attach the invoice receipt and updated quotation...',
      confirmText: 'Submit Request',
      cancelText: 'Cancel',
      type: 'warning',
      multiline: true
    });

    if (comments === null) {
      return;
    }

    try {
      await this.workflowApi.requestChanges(id, comments || 'Modifications requested');
      await this.loadTasks();
      this.toast.warning('Requested modifications sent back to task submitter.', 'Changes Requested');
    } catch (e) {
      console.error('Failed to request changes', e);
      this.toast.error('Failed to request changes on task.');
    }
  }

  async copyOneClickDecisionLink(taskId: string, decision: 'approve' | 'reject') {
    try {
      const token = await this.workflowApi.generateDecisionToken(taskId, decision);
      const url = `${window.location.origin}/api/workflow/workflow-task/execute-token?token=${encodeURIComponent(token)}`;
      await navigator.clipboard.writeText(url);
      this.toast.success(`1-Click ${decision.toUpperCase()} secure link copied to clipboard for email/chat dispatch!`, '1-Click Action Link');
    } catch (e) {
      this.toast.info(`Generated 1-click token for task ${taskId}.`);
    }
  }

  async delegateTask(taskId: string) {
    const user = await this.dialog.prompt({
      title: 'Delegate Workflow Task',
      message: 'Enter delegate user or supervisor email / username to reassign this task:',
      placeholder: 'e.g. sarah.manager@company.com',
      confirmText: 'Delegate Task',
      cancelText: 'Cancel',
      type: 'info'
    });

    if (!user) return;
    try {
      await this.workflowApi.delegateTask(taskId, user, 'Vacation / Out of office auto-delegation');
      await this.loadTasks();
      this.toast.success(`Task delegated to ${user}.`, 'Delegated');
    } catch (e) {
      this.toast.error('Failed to delegate task.');
    }
  }

  async triggerSlaEscalationCheck() {
    try {
      const count = await this.workflowApi.escalateOverdueTasks();
      await this.loadTasks();
      this.toast.info(`SLA Watchdog scan complete: ${count} overdue task(s) auto-escalated to supervisor.`, 'SLA Watchdog');
    } catch {
      this.toast.error('Could not run SLA escalation check.');
    }
  }
}
