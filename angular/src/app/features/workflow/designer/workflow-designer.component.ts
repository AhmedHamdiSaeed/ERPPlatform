import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { WorkflowDefinition, WorkflowNode, WorkflowConnection, WorkflowNodeType } from '../../../core/models/erp-models';
import { MOCK_WORKFLOWS } from '../../../core/mock/mock-data';

@Component({
  selector: 'app-workflow-designer',
  standalone: true,
  imports: [FormsModule],
  template: `
    <div class="space-y-4 animate-fade-in pb-8">
      
      <!-- Top Action Toolbar -->
      <div class="card-panel !p-3 flex flex-col md:flex-row md:items-center justify-between gap-3">
        <div class="flex items-center gap-3">
          <div class="w-9 h-9 rounded-xl bg-indigo-600 text-white flex items-center justify-center text-lg font-bold">
            <i class="pi pi-sitemap"></i>
          </div>
          <div>
            <div class="flex items-center gap-2">
              <h1 class="text-sm font-extrabold text-[var(--text-main)]">{{ currentWorkflow().name }}</h1>
              <span class="px-2 py-0.5 text-[9px] font-bold bg-indigo-100 text-indigo-700 rounded-full">{{ currentWorkflow().version }}</span>
            </div>
            <p class="text-[11px] text-[var(--text-muted)]">{{ currentWorkflow().description }}</p>
          </div>
        </div>

        <div class="flex items-center gap-2">
          <button (click)="testWorkflow()" class="btn-outline text-xs">
            <i class="pi pi-play text-emerald-600"></i> Test Workflow
          </button>
          <button (click)="saveWorkflow()" class="btn-primary text-xs cursor-pointer">
            <i class="pi pi-save"></i> Publish Workflow
          </button>
        </div>
      </div>

      <!-- Main Visual Designer Studio Layout -->
      <div class="grid grid-cols-1 lg:grid-cols-4 gap-4 items-start">
        
        <!-- Left: Node Palette Palette Sidebar -->
        <div class="card-panel space-y-3 lg:col-span-1">
          <h3 class="text-xs font-bold text-[var(--text-main)] border-b border-[var(--border-color)] pb-2">Available Workflow Nodes</h3>
          <p class="text-[11px] text-[var(--text-muted)]">Click or drag any node to insert into canvas:</p>

          <div class="space-y-2">
            @for (palette of paletteItems; track palette.type) {
              <div 
                (click)="addNode(palette.type)"
                class="p-2.5 rounded-xl border border-[var(--border-color)] hover:border-indigo-500 bg-slate-50/50 dark:bg-slate-800/50 cursor-pointer flex items-center gap-3 group transition-all">
                <div [class]="palette.bg" class="w-7 h-7 rounded-lg flex items-center justify-center text-xs shrink-0 shadow-2xs group-hover:scale-105 transition-transform">
                  <i [class]="'pi ' + palette.icon"></i>
                </div>
                <div>
                  <span class="text-xs font-bold text-[var(--text-main)] block group-hover:text-indigo-600">{{ palette.title }}</span>
                  <span class="text-[10px] text-[var(--text-muted)] block">{{ palette.desc }}</span>
                </div>
              </div>
            }
          </div>
        </div>

        <!-- Center: Interactive Node Canvas Studio -->
        <div class="card-panel lg:col-span-3 min-h-[500px] relative bg-slate-900/5 dark:bg-slate-950/40 rounded-2xl overflow-hidden border-2 border-dashed border-slate-300 dark:border-slate-800 p-6">
          
          <!-- Canvas Grid Controls -->
          <div class="absolute top-3 right-3 z-10 flex items-center gap-1.5 bg-[var(--bg-card)] p-1.5 rounded-xl shadow-md border border-[var(--border-color)] text-xs">
            <button (click)="zoomIn()" class="p-1 hover:text-indigo-600"><i class="pi pi-plus"></i></button>
            <span class="px-2 font-mono font-bold text-[10px] text-slate-500">{{ zoomLevel() }}%</span>
            <button (click)="zoomOut()" class="p-1 hover:text-indigo-600"><i class="pi pi-minus"></i></button>
            <div class="w-px h-4 bg-slate-200 dark:bg-slate-700 mx-1"></div>
            <button (click)="resetCanvas()" class="p-1 hover:text-indigo-600" title="Reset Layout"><i class="pi pi-refresh"></i></button>
          </div>

          <!-- Canvas Nodes Render -->
          <div class="relative w-full h-[450px] transition-transform origin-top-left" [style.transform]="'scale(' + zoomLevel() / 100 + ')'">
            
            <!-- Nodes -->
            @for (node of currentWorkflow().nodes; track node.id) {
              <div 
                (click)="selectedNode.set(node)"
                [style.left.px]="node.x"
                [style.top.px]="node.y"
                [class.ring-2]="selectedNode()?.id === node.id"
                class="absolute w-52 card-panel !p-3 cursor-move shadow-md hover:shadow-xl transition-all border-l-4"
                [class.border-l-emerald-500]="node.type === 'trigger'"
                [class.border-l-amber-500]="node.type === 'condition'"
                [class.border-l-indigo-500]="node.type === 'approval'"
                [class.border-l-blue-500]="node.type === 'notification'"
                [class.border-l-slate-500]="node.type === 'end'">
                
                <div class="flex items-center justify-between mb-1">
                  <span class="text-[10px] font-extrabold uppercase tracking-wider text-slate-400">{{ node.type }}</span>
                  <button (click)="$event.stopPropagation(); removeNode(node.id)" class="text-slate-400 hover:text-rose-600"><i class="pi pi-times text-xs"></i></button>
                </div>

                <h4 class="font-bold text-xs text-[var(--text-main)]">{{ node.title }}</h4>
                <p class="text-[10px] text-[var(--text-muted)] mt-0.5 truncate">{{ node.subtitle || 'Click to configure node' }}</p>

                <div class="mt-2 pt-1 border-t border-slate-100 dark:border-slate-700/50 flex justify-between items-center text-[9px] text-slate-400">
                  <span>ID: {{ node.id }}</span>
                  <span class="text-indigo-600 font-semibold">Connect →</span>
                </div>
              </div>
            }

          </div>

        </div>

      </div>

      <!-- Node Properties Configuration Panel -->
      @if (selectedNode()) {
        <div class="card-panel space-y-3 border-indigo-200 dark:border-slate-700">
          <div class="flex items-center justify-between border-b border-[var(--border-color)] pb-2">
            <h4 class="font-bold text-xs text-indigo-600">Configure Node: {{ selectedNode()?.title }}</h4>
            <button (click)="selectedNode.set(null)" class="text-slate-400 hover:text-slate-600"><i class="pi pi-times"></i></button>
          </div>

          <div class="grid grid-cols-1 md:grid-cols-3 gap-3 text-xs">
            <div>
              <label class="font-bold block mb-1">Node Title</label>
              <input type="text" [(ngModel)]="selectedNode()!.title" class="w-full px-3 py-2 bg-slate-100 dark:bg-slate-800 border rounded-lg" />
            </div>
            <div>
              <label class="font-bold block mb-1">Subtitle / Summary</label>
              <input type="text" [(ngModel)]="selectedNode()!.subtitle" class="w-full px-3 py-2 bg-slate-100 dark:bg-slate-800 border rounded-lg" />
            </div>
            <div>
              <label class="font-bold block mb-1">Node Execution Mode</label>
              <select class="w-full px-3 py-2 bg-slate-100 dark:bg-slate-800 border rounded-lg">
                <option>Synchronous</option>
                <option>Asynchronous Queue</option>
              </select>
            </div>
          </div>
        </div>
      }

    </div>
  `
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
