import { Component, inject, signal } from '@angular/core';
import { TranslatePipe } from '../../../shared/pipes/translate.pipe';
import { FormsModule } from '@angular/forms';
import { LeaveRequest } from '../../../core/models/erp-models';
import { HrApiService } from '../../../core/services/api/hr-api.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-leave-management',
  standalone: true,
  imports: [FormsModule, TranslatePipe],
  templateUrl: './leave-management.component.html'
})
export class LeaveManagementComponent {
  private hrApi = inject(HrApiService);
  private toast = inject(ToastService);

  requests = signal<LeaveRequest[]>([]);
  showModal = signal(false);

  newReq: Partial<LeaveRequest> = {
    leaveType: 'Annual',
    startDate: '2026-08-20',
    endDate: '2026-08-25',
    reason: ''
  };

  constructor() {
    this.loadRequests();
  }

  async loadRequests() {
    try {
      this.requests.set(await this.hrApi.getLeaveRequests());
    } catch (e) {
      console.error('Failed to load leave requests', e);
      this.toast.error('Could not load leave requests from the server.');
    }
  }

  openRequestModal() {
    this.showModal.set(true);
  }

  async submitRequest() {
    const startDate = new Date(this.newReq.startDate!);
    const endDate = new Date(this.newReq.endDate!);
    const daysCount = Math.max(1, Math.round((endDate.getTime() - startDate.getTime()) / 86400000) + 1);

    try {
      const employees = await this.hrApi.getEmployees();
      const empId = employees.length > 0 ? employees[0].id : '00000000-0000-0000-0000-000000000001';
      const empName = employees.length > 0 ? employees[0].name : 'Ahmed Saeed';

      await this.hrApi.createLeaveRequest({
        employeeId: empId,
        employeeName: empName,
        leaveType: this.newReq.leaveType,
        startDate: this.newReq.startDate,
        endDate: this.newReq.endDate,
        daysCount,
        reason: this.newReq.reason || 'Personal request',
        status: 'Pending'
      });
      await this.loadRequests();
      this.toast.success('Leave request submitted for approval.');
    } catch (e) {
      console.error('Failed to submit leave request', e);
      this.toast.error('Failed to submit the leave request.');
    }
    this.showModal.set(false);
  }

  async approve(id: string) {
    try {
      await this.hrApi.approveLeaveRequest(id);
      await this.loadRequests();
      this.toast.success('Leave request approved.');
    } catch (e) {
      console.error('Failed to approve leave request', e);
      this.toast.error('Failed to approve the leave request.');
    }
  }

  async reject(id: string) {
    try {
      await this.hrApi.rejectLeaveRequest(id);
      await this.loadRequests();
      this.toast.success('Leave request rejected.');
    } catch (e) {
      console.error('Failed to reject leave request', e);
      this.toast.error('Failed to reject the leave request.');
    }
  }
}
