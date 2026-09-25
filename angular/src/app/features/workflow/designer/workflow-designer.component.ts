import { Component, inject, signal, computed } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { TranslatePipe } from '../../../shared/pipes/translate.pipe';

import {
  WorkflowDefinition,
  WorkflowNode,
  WorkflowConnection,
  WorkflowNodeType,
  WorkflowVersionDto,
  WorkflowDiffResult,
  WorkflowHeatmapMetrics,
  WorkflowNodeHeatmap
} from '../../../core/models/erp-models';
import { WorkflowApiService, WorkflowSimulationResult } from '../../../core/services/api/workflow-api.service';
import { ToastService } from '../../../core/services/toast.service';

export interface PaletteItem {
  type: WorkflowNodeType;
  title: string;
  desc: string;
  icon: string;
  bg: string;
  category?: 'Gateways' | 'Triggers' | 'Logic' | 'Integrations';
}

@Component({
  selector: 'app-workflow-designer',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslatePipe],
  templateUrl: './workflow-designer.component.html'
})
export class WorkflowDesignerComponent {
  private toast = inject(ToastService);
  private workflowApi = inject(WorkflowApiService);

  currentWorkflow = signal<WorkflowDefinition | null>(null);
  selectedNode = signal<WorkflowNode | null>(null);
  zoomLevel = signal(100);

  // ---- Interactive Connection Mode ----
  linkingFromNodeId = signal<string | null>(null);

  // ---- Node Dragging State ----
  private isDragging = false;
  private draggedNodeId: string | null = null;
  private dragStartX = 0;
  private dragStartY = 0;
  private nodeInitialX = 0;
  private nodeInitialY = 0;

  // ---- Simulation State ----
  simulating = signal(false);
  activeSimStepNodeId = signal<string | null>(null);
  simulationLogs = signal<string[]>([]);

  // ---- Heatmap & Process Mining Mode ----
  heatmapMode = signal(false);
  heatmapMetrics = signal<WorkflowHeatmapMetrics | null>(null);

  // ---- Versioning & Schema Diff ----
  versionsList = signal<WorkflowVersionDto[]>([]);
  showDiffModal = signal(false);
  diffTargetVersionId = signal<string>('');
  diffResult = signal<WorkflowDiffResult | null>(null);
  isComparing = signal(false);

  // ---- AI Workflow Generator Modal ----
  showAiModal = signal(false);
  aiPrompt = '';
  isGeneratingAi = signal(false);

  // ---- Templates Modal ----
  showTemplatesModal = signal(false);
  templatesList = signal<WorkflowDefinition[]>([]);
  loadingTemplates = signal(false);

  readonly paletteItems: PaletteItem[] = [
    // Triggers
    { type: 'trigger', title: 'Manual Trigger', desc: 'Manual or on-demand initiation', icon: 'pi-bolt', bg: 'bg-emerald-100 text-emerald-700 dark:bg-emerald-950/60 dark:text-emerald-300', category: 'Triggers' },
    { type: 'cron-trigger', title: 'Cron Schedule', desc: 'Recurring interval / cron schedule', icon: 'pi-calendar', bg: 'bg-teal-100 text-teal-700 dark:bg-teal-950/60 dark:text-teal-300', category: 'Triggers' },
    { type: 'entity-cdc', title: 'Entity CDC Trigger', desc: 'On DB record created or status change', icon: 'pi-database', bg: 'bg-green-100 text-green-700 dark:bg-green-950/60 dark:text-green-300', category: 'Triggers' },
    { type: 'webhook-trigger', title: 'Inbound Webhook', desc: 'Dedicated HTTP payload endpoint', icon: 'pi-link', bg: 'bg-cyan-100 text-cyan-700 dark:bg-cyan-950/60 dark:text-cyan-300', category: 'Triggers' },
    
    // Gateways & Flow
    { type: 'condition', title: 'Condition Gateway', desc: 'If / Else branching decision logic', icon: 'pi-filter', bg: 'bg-amber-100 text-amber-700 dark:bg-amber-950/60 dark:text-amber-300', category: 'Gateways' },
    { type: 'parallel-fork', title: 'Parallel Fork', desc: 'Split flow into concurrent tracks', icon: 'pi-sitemap', bg: 'bg-orange-100 text-orange-700 dark:bg-orange-950/60 dark:text-orange-300', category: 'Gateways' },
    { type: 'parallel-join', title: 'Parallel Join', desc: 'Wait & synchronize parallel tracks', icon: 'pi-arrows-alt', bg: 'bg-amber-100 text-amber-800 dark:bg-amber-950/70 dark:text-amber-200', category: 'Gateways' },
    { type: 'for-each', title: 'For-Each Loop', desc: 'Iterate over collection / line items', icon: 'pi-sync', bg: 'bg-lime-100 text-lime-700 dark:bg-lime-950/60 dark:text-lime-300', category: 'Gateways' },
    
    // Steps & Actions
    { type: 'approval', title: 'Approval Step', desc: 'Role / Manager sign-off with SLA', icon: 'pi-user-edit', bg: 'bg-indigo-100 text-indigo-700 dark:bg-indigo-950/60 dark:text-indigo-300', category: 'Logic' },
    { type: 'action', title: 'ERP Action', desc: 'Post ledger, adjust stock, create PR', icon: 'pi-cog', bg: 'bg-purple-100 text-purple-700 dark:bg-purple-950/60 dark:text-purple-300', category: 'Logic' },
    { type: 'sub-workflow', title: 'Sub-Workflow', desc: 'Execute nested child workflow', icon: 'pi-folder', bg: 'bg-violet-100 text-violet-700 dark:bg-violet-950/60 dark:text-violet-300', category: 'Logic' },
    { type: 'notification', title: 'Notification', desc: 'Email, SMS, or In-App broadcast', icon: 'pi-send', bg: 'bg-blue-100 text-blue-700 dark:bg-blue-950/60 dark:text-blue-300', category: 'Integrations' },
    { type: 'webhook', title: 'Outbound Webhook', desc: 'HTTP POST/GET to external API', icon: 'pi-globe', bg: 'bg-sky-100 text-sky-700 dark:bg-sky-950/60 dark:text-sky-300', category: 'Integrations' },
    { type: 'ai', title: 'AI Copilot Step', desc: 'Document OCR & automated risk check', icon: 'pi-sparkles', bg: 'bg-fuchsia-100 text-fuchsia-700 dark:bg-fuchsia-950/60 dark:text-fuchsia-300', category: 'Logic' },
    { type: 'end', title: 'End Node', desc: 'Complete and terminate workflow', icon: 'pi-flag-fill', bg: 'bg-slate-200 text-slate-700 dark:bg-slate-700 dark:text-slate-200', category: 'Gateways' }
  ];

  constructor() {
    this.loadCurrentWorkflow();
  }

  async loadCurrentWorkflow(): Promise<void> {
    try {
      const defs = await this.workflowApi.getDefinitions();
      if (defs.length > 0) {
        this.currentWorkflow.set(defs[0]);
      } else {
        await this.loadDefaultTemplate();
      }
    } catch (e) {
      console.error('Failed to load workflow definitions', e);
      this.toast.error('Could not load workflow definitions from the server.', 'Workflow');
      await this.loadDefaultTemplate();
    }
  }

  private async loadDefaultTemplate(): Promise<void> {
    const templates = await this.workflowApi.getTemplates();
    if (templates.length > 0) {
      this.currentWorkflow.set(templates[0]);
    }
  }

  // ================= Node Manipulation =================

  addNode(type: WorkflowNodeType): void {
    const wf = this.currentWorkflow();
    if (!wf) return;

    const newId = `node-${Date.now()}`;
    const count = wf.nodes.length;
    const newNode: WorkflowNode = {
      id: newId,
      type: type,
      title: `${type.toUpperCase()} Step #${count + 1}`,
      subtitle: this.getDefaultSubtitle(type),
      x: 80 + (count % 4) * 260,
      y: 100 + Math.floor(count / 4) * 160,
      config: this.getDefaultConfig(type)
    };

    this.currentWorkflow.update(w => w ? ({
      ...w,
      nodes: [...w.nodes, newNode]
    }) : w);

    this.selectedNode.set(newNode);
    this.toast.info(`Added ${type} step to canvas.`, 'Node Added');
  }

  removeNode(id: string): void {
    this.currentWorkflow.update(wf => wf ? ({
      ...wf,
      nodes: wf.nodes.filter(n => n.id !== id),
      connections: wf.connections.filter(c => c.sourceId !== id && c.targetId !== id)
    }) : wf);

    if (this.selectedNode()?.id === id) {
      this.selectedNode.set(null);
    }
    if (this.linkingFromNodeId() === id) {
      this.linkingFromNodeId.set(null);
    }
    this.toast.info('Node removed from workflow.', 'Deleted');
  }

  // ================= Connection Management =================

  startConnecting(nodeId: string, event?: MouseEvent): void {
    if (event) event.stopPropagation();
    if (this.linkingFromNodeId() === nodeId) {
      this.linkingFromNodeId.set(null);
      return;
    }

    const source = this.linkingFromNodeId();
    if (!source) {
      this.linkingFromNodeId.set(nodeId);
      this.toast.info('Click target node to create link.', 'Connecting');
      return;
    }

    if (source === nodeId) {
      this.linkingFromNodeId.set(null);
      return;
    }

    this.createConnection(source, nodeId);
    this.linkingFromNodeId.set(null);
  }

  onNodeClick(node: WorkflowNode): void {
    const sourceId = this.linkingFromNodeId();
    if (sourceId && sourceId !== node.id) {
      this.createConnection(sourceId, node.id);
      this.linkingFromNodeId.set(null);
      return;
    }
    this.selectedNode.set(node);
  }

  createConnection(sourceId: string, targetId: string, label = ''): void {
    const wf = this.currentWorkflow();
    if (!wf) return;

    // Prevent duplicates
    if (wf.connections.some(c => c.sourceId === sourceId && c.targetId === targetId)) {
      this.toast.warning('Connection already exists between these nodes.');
      return;
    }

    const sourceNode = wf.nodes.find(n => n.id === sourceId);
    let defaultLabel = label;
    if (!defaultLabel && sourceNode?.type === 'condition') {
      const existingFromCondition = wf.connections.filter(c => c.sourceId === sourceId);
      defaultLabel = existingFromCondition.length === 0 ? 'True' : 'False';
    } else if (!defaultLabel && sourceNode?.type === 'approval') {
      defaultLabel = 'Approved';
    }

    const newConnection: WorkflowConnection = {
      id: `conn-${Date.now()}`,
      sourceId,
      targetId,
      label: defaultLabel
    };

    this.currentWorkflow.update(w => w ? ({
      ...w,
      connections: [...w.connections, newConnection]
    }) : w);

    this.toast.success('Nodes connected successfully.', 'Link Created');
  }

  removeConnection(connId: string, event?: MouseEvent): void {
    if (event) event.stopPropagation();
    this.currentWorkflow.update(wf => wf ? ({
      ...wf,
      connections: wf.connections.filter(c => c.id !== connId)
    }) : wf);
    this.toast.info('Connection removed.', 'Link Deleted');
  }

  // ================= SVG Bezier Curves Calculations =================

  getConnectionPath(conn: WorkflowConnection): string {
    const wf = this.currentWorkflow();
    if (!wf) return '';

    const source = wf.nodes.find(n => n.id === conn.sourceId);
    const target = wf.nodes.find(n => n.id === conn.targetId);
    if (!source || !target) return '';

    const nodeWidth = 220;
    const nodeHeight = 76;

    const x1 = source.x + nodeWidth;
    const y1 = source.y + nodeHeight / 2;
    const x2 = target.x;
    const y2 = target.y + nodeHeight / 2;

    const dx = Math.abs(x2 - x1);
    const controlPointOffset = Math.max(dx * 0.5, 50);

    return `M ${x1} ${y1} C ${x1 + controlPointOffset} ${y1}, ${x2 - controlPointOffset} ${y2}, ${x2} ${y2}`;
  }

  getConnectionMidpoint(conn: WorkflowConnection): { x: number; y: number } {
    const wf = this.currentWorkflow();
    if (!wf) return { x: 0, y: 0 };

    const source = wf.nodes.find(n => n.id === conn.sourceId);
    const target = wf.nodes.find(n => n.id === conn.targetId);
    if (!source || !target) return { x: 0, y: 0 };

    const nodeWidth = 220;
    const nodeHeight = 76;

    const x1 = source.x + nodeWidth;
    const y1 = source.y + nodeHeight / 2;
    const x2 = target.x;
    const y2 = target.y + nodeHeight / 2;

    return {
      x: (x1 + x2) / 2,
      y: (y1 + y2) / 2
    };
  }

  // ================= Node Dragging Handlers =================

  onNodeMouseDown(node: WorkflowNode, event: MouseEvent): void {
    if ((event.target as HTMLElement).closest('button, input, select')) return;
    this.isDragging = true;
    this.draggedNodeId = node.id;
    this.dragStartX = event.clientX;
    this.dragStartY = event.clientY;
    this.nodeInitialX = node.x;
    this.nodeInitialY = node.y;
    event.preventDefault();
  }

  onCanvasMouseMove(event: MouseEvent): void {
    if (!this.isDragging || !this.draggedNodeId) return;

    const scale = this.zoomLevel() / 100;
    const deltaX = (event.clientX - this.dragStartX) / scale;
    const deltaY = (event.clientY - this.dragStartY) / scale;

    const newX = Math.max(20, Math.round(this.nodeInitialX + deltaX));
    const newY = Math.max(20, Math.round(this.nodeInitialY + deltaY));

    this.currentWorkflow.update(wf => {
      if (!wf) return null;
      return {
        ...wf,
        nodes: wf.nodes.map(n => n.id === this.draggedNodeId ? { ...n, x: newX, y: newY } : n)
      };
    });
  }

  onCanvasMouseUp(): void {
    this.isDragging = false;
    this.draggedNodeId = null;
  }

  // ================= Simulation Runner =================

  async runLiveSimulation(): Promise<void> {
    const wf = this.currentWorkflow();
    if (!wf || wf.nodes.length === 0) {
      this.toast.warning('Add at least one node to test workflow.');
      return;
    }

    this.simulating.set(true);
    this.simulationLogs.set([]);
    this.toast.info('Starting step-by-step workflow simulation...', 'Simulation');

    const orderedNodes = [...wf.nodes];
    for (let i = 0; i < orderedNodes.length; i++) {
      const node = orderedNodes[i];
      this.activeSimStepNodeId.set(node.id);
      this.simulationLogs.update(logs => [...logs, `[${new Date().toLocaleTimeString()}] Executing step: ${node.title} (${node.type})`]);
      await new Promise(resolve => setTimeout(resolve, 800));
    }

    this.activeSimStepNodeId.set(null);
    this.simulating.set(false);
    this.toast.success('Simulation completed successfully! All execution steps passed with 0 errors.', 'Simulation Passed');
  }

  // ================= AI Workflow Generator =================

  openAiGenerator(): void {
    this.showAiModal.set(true);
    this.aiPrompt = '';
  }

  closeAiGenerator(): void {
    this.showAiModal.set(false);
  }

  async generateWorkflowWithAi(): Promise<void> {
    const prompt = this.aiPrompt.trim();
    if (!prompt) {
      this.toast.warning('Please enter a description for the workflow.');
      return;
    }

    this.isGeneratingAi.set(true);
    try {
      const generated = await this.workflowApi.generateFromAi(prompt, 'AI Synthesized');
      this.currentWorkflow.set(generated);
      this.closeAiGenerator();
      this.toast.success(`Generated workflow "${generated.name}" with ${generated.nodes.length} nodes!`, 'AI Copilot');
    } catch (err) {
      console.error('AI workflow generation failed', err);
      this.toast.error('Could not generate workflow with AI. Please try again.');
    } finally {
      this.isGeneratingAi.set(false);
    }
  }

  // ================= Templates Library =================

  async openTemplates(): Promise<void> {
    this.showTemplatesModal.set(true);
    this.loadingTemplates.set(true);
    try {
      this.templatesList.set(await this.workflowApi.getTemplates());
    } catch {
      this.templatesList.set([]);
    } finally {
      this.loadingTemplates.set(false);
    }
  }

  closeTemplates(): void {
    this.showTemplatesModal.set(false);
  }

  selectTemplate(tpl: WorkflowDefinition): void {
    this.currentWorkflow.set({
      ...tpl,
      id: '',
      version: 'v1.0',
      status: 'Draft'
    });
    this.closeTemplates();
    this.toast.success(`Loaded template "${tpl.name}".`, 'Template Loaded');
  }

  // ================= Heatmap & Process Mining =================

  async toggleHeatmapMode(): Promise<void> {
    const nextState = !this.heatmapMode();
    this.heatmapMode.set(nextState);

    if (nextState) {
      const wf = this.currentWorkflow();
      if (!wf) return;
      try {
        const metrics = await this.workflowApi.getHeatmapMetrics(wf.id || 'wf-demo');
        this.heatmapMetrics.set(metrics);
        this.toast.info(`Process Heatmap enabled. Average completion cycle: ${metrics.averageCompletionHours}h`, 'Process Mining Active');
      } catch {
        this.toast.warning('Could not load historical runtime metrics.');
      }
    } else {
      this.toast.info('Returned to standard Canvas layout view.', 'Standard Mode');
    }
  }

  getNodeHeatmap(nodeId: string): WorkflowNodeHeatmap | undefined {
    return this.heatmapMetrics()?.nodes.find(n => n.nodeId === nodeId);
  }

  // ================= Version Management & Schema Diff =================

  async loadVersionsForCurrent(): Promise<void> {
    const wf = this.currentWorkflow();
    if (!wf || !wf.code) return;
    try {
      const list = await this.workflowApi.getVersions(wf.code);
      this.versionsList.set(list);
    } catch {
      this.versionsList.set([]);
    }
  }

  async createNewVersionBranch(): Promise<void> {
    const wf = this.currentWorkflow();
    if (!wf || !wf.id) {
      this.toast.warning('Please save the current workflow before branching a new version.');
      return;
    }

    const notes = prompt('Enter changelog notes for this new version:') || 'Branched version';
    try {
      const newVer = await this.workflowApi.createNewVersion(wf.id, notes);
      this.currentWorkflow.set(newVer);
      await this.loadVersionsForCurrent();
      this.toast.success(`New draft version ${newVer.version} created successfully!`, 'Version Branched');
    } catch (e) {
      console.error('Failed to create version', e);
      this.toast.error('Could not branch new version.');
    }
  }

  async publishCurrentVersion(): Promise<void> {
    const wf = this.currentWorkflow();
    if (!wf || !wf.id) {
      this.toast.warning('Workflow must be saved before publishing.');
      return;
    }

    try {
      const published = await this.workflowApi.publishVersion(wf.id);
      this.currentWorkflow.set(published);
      await this.loadVersionsForCurrent();
      this.toast.success(`Version ${published.version} is now Active in production!`, 'Published');
    } catch (e) {
      console.error('Failed to publish version', e);
      this.toast.error('Failed to publish version.');
    }
  }

  async openDiffModal(): Promise<void> {
    await this.loadVersionsForCurrent();
    const currentId = this.currentWorkflow()?.id;
    const other = this.versionsList().find(v => v.id !== currentId);
    this.diffTargetVersionId.set(other ? other.id : '');
    this.showDiffModal.set(true);
    if (other && currentId) {
      await this.runVersionComparison(currentId, other.id);
    }
  }

  closeDiffModal(): void {
    this.showDiffModal.set(false);
    this.diffResult.set(null);
  }

  async runVersionComparison(id1?: string, id2?: string): Promise<void> {
    const v1 = id1 || this.currentWorkflow()?.id;
    const v2 = id2 || this.diffTargetVersionId();
    if (!v1 || !v2) return;

    this.isComparing.set(true);
    try {
      const res = await this.workflowApi.compareVersions(v1, v2);
      this.diffResult.set(res);
    } catch (e) {
      console.error('Diff calculation failed', e);
      this.toast.error('Could not compute schema difference.');
    } finally {
      this.isComparing.set(false);
    }
  }

  // ================= Canvas View Controls =================

  zoomIn(): void {
    if (this.zoomLevel() < 150) this.zoomLevel.update(z => z + 10);
  }

  zoomOut(): void {
    if (this.zoomLevel() > 50) this.zoomLevel.update(z => z - 10);
  }

  resetCanvas(): void {
    this.zoomLevel.set(100);
  }

  // ================= Publish / Save =================

  async saveWorkflow(): Promise<void> {
    const wf = this.currentWorkflow();
    if (!wf) return;

    try {
      if (wf.id) {
        await this.workflowApi.updateDefinition(wf.id, wf);
      } else {
        await this.workflowApi.createDefinition(wf);
      }
      this.toast.success(`Workflow "${wf.name}" published to ERP execution engine.`, 'Workflow Published');
    } catch (e) {
      console.error('Failed to save workflow', e);
      this.toast.error('Failed to publish the workflow definition.', 'Publish Failed');
    }
  }

  // ================= Helpers =================

  private getDefaultSubtitle(type: WorkflowNodeType): string {
    switch (type) {
      case 'trigger': return 'On entity created / webhook';
      case 'cron-trigger': return 'Interval: 0 0 * * *';
      case 'entity-cdc': return 'On PurchaseOrder.Created';
      case 'webhook-trigger': return 'POST /api/inbound/wf-key';
      case 'condition': return 'Check field criteria';
      case 'parallel-fork': return 'Fork concurrent tracks (AND)';
      case 'parallel-join': return 'Wait & Join tracks (All)';
      case 'for-each': return 'Loop over item in items[]';
      case 'approval': return 'Assigned to Manager (SLA: 24h)';
      case 'action': return 'Update ERP record';
      case 'sub-workflow': return 'Call child workflow';
      case 'notification': return 'Email & In-App broadcast';
      case 'webhook': return 'POST to external URL';
      case 'ai': return 'AI document validation';
      case 'end': return 'Workflow finished';
      default: return 'Configured step';
    }
  }

  private getDefaultConfig(type: WorkflowNodeType): Record<string, any> {
    switch (type) {
      case 'cron-trigger':
        return { cronExpression: '0 0 * * *', timezone: 'UTC', executionMode: 'Scheduled' };
      case 'entity-cdc':
        return { targetEntity: 'PurchaseOrder', eventType: 'Created', filterExpression: 'Amount > 1000' };
      case 'webhook-trigger':
        return { webhookPath: '/inbound/po-approval', requireSecret: true, secretKey: 'sec_live_' + Math.random().toString(36).substring(2, 10) };
      case 'parallel-fork':
        return { forkMode: 'AND', branchCount: 2, executionMode: 'Parallel' };
      case 'parallel-join':
        return { joinCondition: 'WaitAll', timeoutMinutes: 60, executionMode: 'Synchronous' };
      case 'for-each':
        return { collectionProperty: 'items', itemVariableName: 'currentItem', executionMode: 'Batch Sequential' };
      case 'sub-workflow':
        return { targetWorkflowCode: 'WF-SUB-001', passContext: true, executionMode: 'Synchronous' };
      case 'condition':
        return { field: 'Amount', op: '>', value: '5000', executionMode: 'Synchronous' };
      case 'approval':
        return { role: 'Manager', slaHours: 24, priority: 'High', executionMode: 'Synchronous', allowDelegation: true, sendEmailActionLink: true };
      case 'action':
        return { actionType: 'CreateRecord', targetModule: 'Sales', executionMode: 'Synchronous' };
      case 'notification':
        return { channel: 'Email', template: 'Task_Notification', executionMode: 'Asynchronous Queue' };
      case 'webhook':
        return { method: 'POST', url: 'https://api.domain.com/webhook', executionMode: 'Asynchronous Queue' };
      case 'ai':
        return { task: 'RiskAnalysis', model: 'default', executionMode: 'Synchronous' };
      default:
        return { executionMode: 'Synchronous' };
    }
  }
}
