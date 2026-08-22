import { Injectable } from '@angular/core';
import { ErpApiService, toDateString, AbpEntity } from './erp-api.service';
import { WorkflowDefinition, WorkflowTask } from '../../models/erp-models';

interface WorkflowDefinitionDto extends AbpEntity {
  code: string; name: string; description: string; category: string;
  status: string; graphJson: string; version: number;
}

interface WorkflowTaskDto extends AbpEntity {
  taskNumber: string; workflowName: string; requestedBy: string;
  requestedByAvatar?: string; details: string; createdDate: string;
  status: string; comments?: string;
}

@Injectable({ providedIn: 'root' })
export class WorkflowApiService extends ErpApiService {
  getDefinitions(): Promise<WorkflowDefinition[]> {
    return this.getList<WorkflowDefinitionDto>('workflow-definition').then(items =>
      items.map(d => {
        const graph = this.parseGraph(d.graphJson);
        return {
          id: d.id,
          name: d.name,
          description: d.description,
          version: `v${d.version}.0`,
          triggerType: (graph.triggerType || 'Manual') as WorkflowDefinition['triggerType'],
          status: d.status === 'Active' ? 'Published' : d.status,
          createdBy: d.category,
          createdDate: '',
          nodes: graph.nodes || [],
          connections: graph.connections || []
        } as WorkflowDefinition;
      })
    );
  }

  createDefinition(def: Partial<WorkflowDefinition>): Promise<void> {
    return this.post('workflow-definition', {
      code: def.id ?? '',
      name: def.name,
      description: def.description ?? '',
      category: def.createdBy || 'General',
      status: def.status === 'Published' ? 'Active' : (def.status ?? 'Draft'),
      graphJson: JSON.stringify({
        nodes: def.nodes ?? [],
        connections: def.connections ?? [],
        triggerType: def.triggerType
      }),
      version: parseInt((def.version ?? 'v1.0').replace('v', ''), 10) || 1
    });
  }

  updateDefinition(id: string, def: Partial<WorkflowDefinition>): Promise<void> {
    return this.put(`workflow-definition/${id}`, {
      code: def.id ?? id,
      name: def.name,
      description: def.description ?? '',
      category: def.createdBy || 'General',
      status: def.status === 'Published' ? 'Active' : (def.status ?? 'Draft'),
      graphJson: JSON.stringify({
        nodes: def.nodes ?? [],
        connections: def.connections ?? [],
        triggerType: def.triggerType
      }),
      version: parseInt((def.version ?? 'v1.0').replace('v', ''), 10) || 1
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

  private mapTaskStatus(status: string): WorkflowTask['status'] {
    switch (status) {
      case 'Approved': return 'Approved';
      case 'Rejected': return 'Rejected';
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
