import { Injectable } from '@angular/core';
import { ErpApiService, toDateString, AbpEntity } from './erp-api.service';
import { WorkflowDefinition, WorkflowTask } from '../../models/erp-models';
import { environment } from '../../../../environments/environment';

interface WorkflowDefinitionDto extends AbpEntity {
  code: string; name: string; description: string; category: string;
  status: string; graphJson: string; version: number;
  isActiveVersion?: boolean; parentDefinitionId?: string; versionNotes?: string;
  triggerType?: string; executionMode?: string; slaHours?: number;
  webhookSecret?: string; cronExpression?: string; cdcEntityName?: string; cdcEvent?: string;
}

interface WorkflowTaskDto extends AbpEntity {
  taskNumber: string; workflowName: string; requestedBy: string;
  requestedByAvatar?: string; details: string; createdDate: string;
  dueDate?: string; priority?: 'Low' | 'Normal' | 'High' | 'Urgent';
  stepOrder?: number; assignedToRole?: string; assignedToUserId?: string;
  originalAssigneeId?: string; delegatedFromUserId?: string;
  isEscalated?: boolean; escalatedToUserId?: string; escalatedAt?: string;
  actionToken?: string; currentNodeId?: string; status: string; comments?: string;
}

export interface WorkflowSimulationResult {
  success: boolean;
  message: string;
  evaluatedNodeIds: string[];
  traversedConnectionIds: string[];
  stepOutputs: Record<string, string>;
  finalStatus: string;
}

import { WorkflowVersionDto, WorkflowDiffResult, WorkflowHeatmapMetrics } from '../../models/erp-models';

@Injectable({ providedIn: 'root' })
export class WorkflowApiService extends ErpApiService {
  protected override apiPrefix(): string {
    return `${environment.apis.default.url}/api/workflow`;
  }

  getDefinitions(): Promise<WorkflowDefinition[]> {
    return this.getList<WorkflowDefinitionDto>('workflow-definition').then(items =>
      items.map(d => this.mapDefinition(d))
    );
  }

  getTemplates(): Promise<WorkflowDefinition[]> {
    return this.get<WorkflowDefinitionDto[]>('workflow-definition/templates').then(items =>
      (items || []).map(d => this.mapDefinition(d))
    ).catch(() => []);
  }

  generateFromAi(prompt: string, category = 'General'): Promise<WorkflowDefinition> {
    return this.post<WorkflowDefinitionDto>('workflow-definition/generate-from-ai', { prompt, category })
      .then(d => this.mapDefinition(d));
  }

  simulateWorkflow(id: string): Promise<WorkflowSimulationResult> {
    return this.post<WorkflowSimulationResult>(`workflow-definition/${id}/simulate`, {});
  }

  getVersions(code: string): Promise<WorkflowVersionDto[]> {
    return this.get<WorkflowVersionDto[]>(`workflow-definition/versions?code=${encodeURIComponent(code)}`).catch(() => []);
  }

  createNewVersion(id: string, notes = ''): Promise<WorkflowDefinition> {
    return this.post<WorkflowDefinitionDto>(`workflow-definition/${id}/new-version?versionNotes=${encodeURIComponent(notes)}`, {})
      .then(d => this.mapDefinition(d));
  }

  publishVersion(id: string): Promise<WorkflowDefinition> {
    return this.post<WorkflowDefinitionDto>(`workflow-definition/${id}/publish`, {})
      .then(d => this.mapDefinition(d));
  }

  compareVersions(id1: string, id2: string): Promise<WorkflowDiffResult> {
    return this.get<WorkflowDiffResult>(`workflow-definition/compare?id1=${id1}&id2=${id2}`).catch(() => ({
      isIdentical: false,
      addedNodes: [],
      removedNodes: [],
      modifiedNodes: ['Schema updated between versions'],
      addedConnections: [],
      removedConnections: []
    }));
  }

  getHeatmapMetrics(id: string): Promise<WorkflowHeatmapMetrics> {
    return this.get<WorkflowHeatmapMetrics>(`workflow-definition/${id}/heatmap-metrics`).catch(() => ({
      workflowId: id,
      totalExecutions: 45,
      averageCompletionHours: 3.2,
      slaCompliancePercent: 95.0,
      nodes: []
    }));
  }

  createDefinition(def: Partial<WorkflowDefinition>): Promise<void> {
    return this.post('workflow-definition', {
      code: def.code || def.id || '',
      name: def.name,
      description: def.description ?? '',
      category: def.createdBy || 'General',
      status: def.status === 'Published' ? 'Active' : (def.status ?? 'Draft'),
      graphJson: JSON.stringify({
        nodes: def.nodes ?? [],
        connections: def.connections ?? [],
        triggerType: def.triggerType
      }),
      version: typeof def.version === 'number' ? def.version : parseInt((def.version ?? 'v1.0').replace('v', ''), 10) || 1,
      triggerType: def.triggerType || 'Manual',
      executionMode: def.executionMode || 'Sequential',
      slaHours: def.slaHours || 24,
      cronExpression: def.cronExpression || '',
      cdcEntityName: def.cdcEntityName || '',
      cdcEvent: def.cdcEvent || ''
    });
  }

  updateDefinition(id: string, def: Partial<WorkflowDefinition>): Promise<void> {
    return this.put(`workflow-definition/${id}`, {
      code: def.code || def.id || id,
      name: def.name,
      description: def.description ?? '',
      category: def.createdBy || 'General',
      status: def.status === 'Published' ? 'Active' : (def.status ?? 'Draft'),
      graphJson: JSON.stringify({
        nodes: def.nodes ?? [],
        connections: def.connections ?? [],
        triggerType: def.triggerType
      }),
      version: typeof def.version === 'number' ? def.version : parseInt((def.version ?? 'v1.0').replace('v', ''), 10) || 1,
      triggerType: def.triggerType || 'Manual',
      executionMode: def.executionMode || 'Sequential',
      slaHours: def.slaHours || 24,
      cronExpression: def.cronExpression || '',
      cdcEntityName: def.cdcEntityName || '',
      cdcEvent: def.cdcEvent || ''
    });
  }

  deleteDefinition(id: string): Promise<void> {
    return this.delete(`workflow-definition/${id}`);
  }

  getTasks(): Promise<WorkflowTask[]> {
    return this.getList<WorkflowTaskDto>('workflow-task').then(items =>
      items.map(t => ({
        id: t.id,
        taskNumber: t.taskNumber,
        workflowName: t.workflowName,
        requestedBy: t.requestedBy,
        requestedByAvatar: t.requestedByAvatar || 'https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=150',
        type: 'Leave Request',
        details: t.details,
        createdDate: toDateString(t.createdDate),
        dueDate: t.dueDate ? toDateString(t.dueDate) : undefined,
        priority: t.priority || 'Normal',
        stepOrder: t.stepOrder || 1,
        assignedToRole: t.assignedToRole,
        assignedToUserId: t.assignedToUserId,
        originalAssigneeId: t.originalAssigneeId,
        delegatedFromUserId: t.delegatedFromUserId,
        isEscalated: t.isEscalated,
        escalatedToUserId: t.escalatedToUserId,
        escalatedAt: t.escalatedAt ? toDateString(t.escalatedAt) : undefined,
        actionToken: t.actionToken,
        currentNodeId: t.currentNodeId,
        status: this.mapTaskStatus(t.status),
        comments: t.comments ? [t.comments] : []
      })) as WorkflowTask[]
    );
  }

  approveTask(id: string, comments = ''): Promise<void> {
    return this.post(`workflow-task/${id}/approve?comments=${encodeURIComponent(comments)}`, {});
  }

  rejectTask(id: string, comments = ''): Promise<void> {
    return this.post(`workflow-task/${id}/reject?comments=${encodeURIComponent(comments)}`, {});
  }

  requestChanges(id: string, comments = ''): Promise<void> {
    return this.post(`workflow-task/${id}/request-changes?comments=${encodeURIComponent(comments)}`, {});
  }

  generateDecisionToken(taskId: string, decision: 'approve' | 'reject' | 'changes'): Promise<string> {
    return this.post<string>(`workflow-task/${taskId}/generate-token?decision=${decision}`, {});
  }

  executeDecisionToken(token: string): Promise<{ success: boolean; message: string; newStatus?: string }> {
    return this.post<{ success: boolean; message: string; newStatus?: string }>(`workflow-task/execute-token?token=${encodeURIComponent(token)}`, {});
  }

  escalateOverdueTasks(): Promise<number> {
    return this.post<number>('workflow-task/escalate-overdue', {});
  }

  delegateTask(taskId: string, delegateUserId: string, reason = 'Vacation delegation'): Promise<void> {
    return this.post(`workflow-task/${taskId}/delegate?delegateUserId=${encodeURIComponent(delegateUserId)}&reason=${encodeURIComponent(reason)}`, {});
  }

  private mapDefinition(d: WorkflowDefinitionDto): WorkflowDefinition {
    const graph = this.parseGraph(d.graphJson);
    return {
      id: d.id,
      code: d.code,
      name: d.name,
      description: d.description,
      version: `v${d.version}.0`,
      isActiveVersion: d.isActiveVersion,
      parentDefinitionId: d.parentDefinitionId,
      versionNotes: d.versionNotes,
      triggerType: (graph.triggerType || d.triggerType || 'Manual') as WorkflowDefinition['triggerType'],
      executionMode: d.executionMode || 'Sequential',
      slaHours: d.slaHours || 24,
      webhookSecret: d.webhookSecret,
      cronExpression: d.cronExpression,
      cdcEntityName: d.cdcEntityName,
      cdcEvent: d.cdcEvent,
      status: d.status === 'Active' ? 'Published' : (d.status as WorkflowDefinition['status']),
      createdBy: d.category,
      createdDate: '',
      nodes: graph.nodes || [],
      connections: graph.connections || []
    } as WorkflowDefinition;
  }

  private mapTaskStatus(status: string): WorkflowTask['status'] {
    switch (status) {
      case 'Approved': return 'Approved';
      case 'Rejected': return 'Rejected';
      case 'ChangesRequested': return 'Changes Requested';
      default: return 'Waiting Approval';
    }
  }

  private parseGraph(graphJson?: string): { nodes?: WorkflowDefinition['nodes']; connections?: WorkflowDefinition['connections']; triggerType?: string } {
    try {
      return graphJson && graphJson !== '{}' ? JSON.parse(graphJson) : {};
    } catch {
      return {};
    }
  }
}
