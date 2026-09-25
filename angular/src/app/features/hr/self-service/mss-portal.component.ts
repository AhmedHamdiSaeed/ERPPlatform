import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HrApiService } from '../../../core/services/api/hr-api.service';
import { TeamSummary, LeaveRequest, HrAction } from '../../../core/models/erp-models';
import { StateService } from '../../../core/services/state.service';
import { TranslatePipe } from '../../../shared/pipes/translate.pipe';

@Component({
  selector: 'app-mss-portal',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslatePipe],
  templateUrl: './mss-portal.component.html'
})
export class MssPortalComponent implements OnInit {
  private hrApi = inject(HrApiService);
  state = inject(StateService);

  loading = false;
  teamSummary: TeamSummary = {
    totalDirectReports: 0,
    presentToday: 0,
    onLeaveToday: 0,
    pendingLeaveApprovals: 0,
    pendingActionApprovals: 0,
    teamMembers: []
  };

  pendingLeaves: LeaveRequest[] = [];
  pendingActions: HrAction[] = [];

  // Promotion/Transfer Modal
  showActionModal = false;
  actionForm = {
    employeeId: '',
    actionType: 'Promotion',
    effectiveDate: '2026-10-01',
    newPosition: '',
    newDepartmentName: '',
    newSalary: 0,
    remarks: 'Approved by line manager based on exceptional performance.'
  };

  ngOnInit(): void {
    this.loadTeamData();
  }

  async loadTeamData(): Promise<void> {
    this.loading = true;
    try {
      const [sum, leaves, actions] = await Promise.all([
        this.hrApi.getManagerTeamSummary(''),
        this.hrApi.getLeaveRequests(),
        this.hrApi.getHrActions()
      ]);
      this.teamSummary = sum || this.teamSummary;
      this.pendingLeaves = (leaves || []).filter(l => l.status === 'Pending');
      this.pendingActions = (actions || []).filter(a => a.status === 'Pending');
    } catch (err) {
      console.error('Error loading MSS team data', err);
    } finally {
      this.loading = false;
    }
  }

  async approveLeave(leaveId: string): Promise<void> {
    try {
      await this.hrApi.approveLeaveRequest(leaveId);
      this.loadTeamData();
    } catch (err) {
      console.error('Error approving leave', err);
    }
  }

  async rejectLeave(leaveId: string): Promise<void> {
    try {
      await this.hrApi.rejectLeaveRequest(leaveId);
      this.loadTeamData();
    } catch (err) {
      console.error('Error rejecting leave', err);
    }
  }

  async approveAction(actionId: string): Promise<void> {
    try {
      await this.hrApi.approveHrAction(actionId, 'Line Manager');
      this.loadTeamData();
    } catch (err) {
      console.error('Error approving HR action', err);
    }
  }

  openActionModal(empId: string): void {
    const emp = this.teamSummary.teamMembers.find(m => m.id === empId);
    this.actionForm = {
      employeeId: empId,
      actionType: 'Promotion',
      effectiveDate: '2026-10-01',
      newPosition: emp?.position || '',
      newDepartmentName: emp?.departmentName || '',
      newSalary: (emp?.salary || 5000) * 1.15,
      remarks: 'Quarterly promotion recommendation'
    };
    this.showActionModal = true;
  }

  async submitAction(): Promise<void> {
    if (!this.actionForm.employeeId) return;
    try {
      await this.hrApi.createHrAction(this.actionForm);
      this.showActionModal = false;
      this.loadTeamData();
    } catch (err) {
      console.error('Error requesting HR action', err);
    }
  }
}
