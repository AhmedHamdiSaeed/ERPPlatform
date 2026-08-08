import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { WorkflowDefinition, WorkflowNode, WorkflowConnection, WorkflowNodeType } from '../../../core/models/erp-models';
import { MOCK_WORKFLOWS } from '../../../core/mock/mock-data';

@Component({
  selector: 'app-workflow-designer',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './workflow-designer.component.html'
})
export class WorkflowDesignerComponent {
  currentWorkflow = signal<WorkflowDefinition>(MOCK_WORKFLOWS[0]);
  selectedNode = signal<WorkflowNode | null>(null);
  zoomLevel = signal(100);

  paletteItems = [
    { type: 'trigger' as WorkflowNodeType, title: 'Trigger Event', desc: 'On entity created or scheduled', icon: 'pi-bolt', bg: 'bg-emerald-100 text-emerald-600' },
    { type: 'condition' as WorkflowNodeType, title: 'Condition Rule', desc: 'If/Else decision logic', icon: 'pi-filter', bg: 'bg-amber-100 text-amber-600' },
    { type: 'approval' as WorkflowNodeType, title: 'Approval Step', desc: 'Manager or HR Sign-off', icon: 'pi-user-edit', bg: 'bg-indigo-100 text-indigo-600' },
    { type: 'notification' as WorkflowNodeType, title: 'Notification', desc: 'Send Email, SMS or In-App', icon: 'pi-send', bg: 'bg-blue-100 text-blue-600' },
    { type: 'end' as WorkflowNodeType, title: 'End Node', desc: 'Complete workflow execution', icon: 'pi-flag-fill', bg: 'bg-slate-200 text-slate-700' }
  ];

  addNode(type: WorkflowNodeType) {
    const newId = `node-${Date.now()}`;
    const count = this.currentWorkflow().nodes.length;
    const newNode: WorkflowNode = {
      id: newId,
      type: type,
      title: `${type.toUpperCase()} Step #${count + 1}`,
      subtitle: 'Configured node step',
      x: 100 + (count % 3) * 230,
      y: 100 + Math.floor(count / 3) * 150
    };

    this.currentWorkflow.update(wf => ({
      ...wf,
      nodes: [...wf.nodes, newNode]
    }));
  }

  removeNode(id: string) {
    this.currentWorkflow.update(wf => ({
      ...wf,
      nodes: wf.nodes.filter(n => n.id !== id)
    }));
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
    alert('Workflow test execution simulation completed successfully with 0 errors!');
  }

  saveWorkflow() {
    alert('Workflow definition published to ERP Engine successfully!');
  }
}
