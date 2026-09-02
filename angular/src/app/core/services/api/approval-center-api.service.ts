import { Injectable } from '@angular/core';
import { ErpApiService } from './erp-api.service';

export type ApprovalEntityType =
  | 'LeaveRequest'
  | 'ExpenseRequest'
  | 'SalesOrder'
  | 'PurchaseRequest'
  | 'WorkflowTask';

interface PendingApprovalDto {
  id: string;
  entityType: ApprovalEntityType;
  title: string;
  description: string;
  requestedBy: string;
  createdDate: string;
  status: string;
  amount?: number;
}

export interface PendingApproval {
  id: string;
  entityType: ApprovalEntityType;
  title: string;
  description: string;
  requestedBy: string;
  createdDate: string;
  status: string;
  amount?: number;
}

export interface BatchApproveInput {
  ids: string[];
  entityType: ApprovalEntityType;
  comments?: string;
}

@Injectable({ providedIn: 'root' })
export class ApprovalCenterApiService extends ErpApiService {
  getPendingApprovals(): Promise<PendingApproval[]> {
    return this.getList<PendingApprovalDto>('approval-center/pending-approvals').then(items =>
      items.map(i => ({
        id: i.id,
        entityType: i.entityType,
        title: i.title,
        description: i.description,
        requestedBy: i.requestedBy,
        createdDate: i.createdDate,
        status: i.status,
        amount: i.amount
      }))
    );
  }

  batchApprove(input: BatchApproveInput): Promise<void> {
    return this.post('approval-center/batch-approve', {
      ids: input.ids,
      entityType: input.entityType,
      comments: input.comments ?? ''
    });
  }
}
