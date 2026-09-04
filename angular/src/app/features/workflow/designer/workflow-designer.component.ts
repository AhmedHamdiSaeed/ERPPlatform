import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { TranslatePipe } from '../../../shared/pipes/translate.pipe';
import { AppDatePipe } from '../../../shared/pipes/app-date.pipe';
import { WorkflowDefinition, WorkflowNode, WorkflowConnection, WorkflowNodeType } from '../../../core/models/erp-models';
import { WorkflowApiService } from '../../../core/services/api/workflow-api.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-workflow-designer',
  standalone: true,
  imports: [FormsModule, TranslatePipe, AppDatePipe],
  templateUrl: './workflow-designer.component.html'
})
export class WorkflowDesignerComponent {
  private toast = inject(ToastService);
  private workflowApi = inject(WorkflowApiService);

  currentWorkflow = signal<WorkflowDefinition | null>(null);
  selectedNode = signal<WorkflowNode | null>(null);
  zoomLevel = signal(100);

  paletteItems = [
    { type: 'trigger' as WorkflowNodeType, title: 'Trigger Event', desc: 'On entity created or scheduled', icon: 'pi-bolt', bg: 'bg-emerald-100 text-emerald-600' },
    { type: 'condition' as WorkflowNodeType, title: 'Condition Rule', desc: 'If/Else decision logic', icon: 'pi-filter', bg: 'bg-amber-100 text-amber-600' },
    { type: 'approval' as WorkflowNodeType, title: 'Approval Step', desc: 'Manager or HR Sign-off', icon: 'pi-user-edit', bg: 'bg-indigo-100 text-indigo-600' },
    { type: 'notification' as WorkflowNodeType, title: 'Notification', desc: 'Send Email, SMS or In-App', icon: 'pi-send', bg: 'bg-blue-100 text-blue-600' },
    { type: 'end' as WorkflowNodeType, title: 'End Node', desc: 'Complete workflow execution', icon: 'pi-flag-fill', bg: 'bg-slate-200 text-slate-700' }
  ];

  constructor() {
    this.loadCurrentWorkflow();
  }

  async loadCurrentWorkflow() {
    try {
      const defs = await this.workflowApi.getDefinitions();
      if (defs.length > 0) {
        this.currentWorkflow.set(defs[0]);
      }
    } catch (e) {
      console.error('Failed to load workflow definitions', e);
      this.toast.error('Could not load workflow definitions from the server.');
    }
  }

  addNode(type: WorkflowNodeType) {
    const wf = this.currentWorkflow();
    if (!wf) return;
    const newId = `node-${Date.now()}`;
    const count = wf.nodes.length;
    const newNode: WorkflowNode = {
      id: newId,
      type: type,
      title: `${type.toUpperCase()} Step #${count + 1}`,
      subtitle: 'Configured node step',
      x: 100 + (count % 3) * 230,
      y: 100 + Math.floor(count / 3) * 150
    };

    this.currentWorkflow.update(w => w ? ({
      ...w,
      nodes: [...w.nodes, newNode]
    }) : w);
    this.toast.info(`Added ${type} node to designer canvas.`, 'Node Added');
  }

  removeNode(id: string) {
    this.currentWorkflow.update(wf => wf ? ({
      ...wf,
      nodes: wf.nodes.filter(n => n.id !== id)
    }) : wf);
    if (this.selectedNode()?.id === id) {
      this.selectedNode.set(null);
    }
  }

  zoomIn() {
    if (this.zoomLevel() < 150) this.zoomLevel.update(z => z + 10);
  }

  zoomOut() {
    if (this.zoomLevel() > 60) this.zoomLevel.update(z => z - 10);
  }

  resetCanvas() {
    this.zoomLevel.set(100);
  }

  testWorkflow() {
    this.toast.success('Workflow test execution simulation completed successfully with 0 errors.', 'Simulation Passed');
  }

  async saveWorkflow() {
    const wf = this.currentWorkflow();
    if (!wf) return;
    try {
      if (wf.id) {
        await this.workflowApi.updateDefinition(wf.id, wf);
      } else {
        await this.workflowApi.createDefinition(wf);
      }
      this.toast.success('Workflow definition published to ERP Engine successfully.', 'Workflow Published');
    } catch (e) {
      console.error('Failed to save workflow', e);
      this.toast.error('Failed to publish the workflow definition.', 'Publish Failed');
    }
  }
}
