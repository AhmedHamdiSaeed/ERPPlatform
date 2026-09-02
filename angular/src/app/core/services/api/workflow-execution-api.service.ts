import { Injectable } from '@angular/core';
import { ErpApiService, AbpEntity } from './erp-api.service';
import { WorkflowExecutionLog } from '../../models/erp-models';

interface WorkflowExecutionStepDto extends AbpEntity {
  workflowExecutionLogId: string;
  stepName: string;
  timestamp: string;
  status: WorkflowExecutionLog['steps'][number]['status'];
  details: string;
  order: number;
}

interface WorkflowExecutionLogDto extends AbpEntity {
  executionCode: string;
  workflowName: string;
  workflowDefinitionId: string | null;
  triggeredBy: string;
  startTime: string;
  endTime: string | null;
  duration: string;
  status: WorkflowExecutionLog['status'];
  steps: WorkflowExecutionStepDto[];
}

@Injectable({ providedIn: 'root' })
export class WorkflowExecutionApiService extends ErpApiService {
  getExecutions(status?: string, maxResultCount = 50): Promise<WorkflowExecutionLog[]> {
    const params = new URLSearchParams({ maxResultCount: String(maxResultCount) });
    if (status) params.set('status', status);

    return this.getList<WorkflowExecutionLogDto>(`workflow-execution-log?${params.toString()}`)
      .then(items => items.map(mapLog));
  }

  getExecution(id: string): Promise<WorkflowExecutionLog> {
    return this.get<WorkflowExecutionLogDto>(`workflow-execution-log/${id}`).then(mapLog);
  }

  startExecution(input: {
    workflowName: string;
    workflowDefinitionId?: string;
    triggeredBy: string;
    executionCode?: string;
  }): Promise<WorkflowExecutionLog> {
    return this.post<WorkflowExecutionLogDto>('workflow-execution-log/start', input).then(mapLog);
  }

  appendStep(id: string, step: {
    stepName: string;
    status?: WorkflowExecutionLog['steps'][number]['status'];
    details?: string;
  }): Promise<WorkflowExecutionLog> {
    return this.post<WorkflowExecutionLogDto>(`workflow-execution-log/${id}/append-step`, step).then(mapLog);
  }

  completeExecution(id: string, status: WorkflowExecutionLog['status'] = 'Completed'): Promise<WorkflowExecutionLog> {
    return this.post<WorkflowExecutionLogDto>(
      `workflow-execution-log/${id}/complete?status=${encodeURIComponent(status)}`, {}
    ).then(mapLog);
  }

  deleteExecution(id: string): Promise<void> {
    return this.delete(`workflow-execution-log/${id}`);
  }
}

function mapLog(l: WorkflowExecutionLogDto): WorkflowExecutionLog {
  return {
    id: l.id,
    executionCode: l.executionCode,
    workflowName: l.workflowName,
    triggeredBy: l.triggeredBy,
    startTime: l.startTime,
    duration: l.duration,
    status: l.status,
    steps: (l.steps ?? []).map(s => ({
      stepName: s.stepName,
      timestamp: s.timestamp,
      status: s.status,
      details: s.details
    }))
  };
}
