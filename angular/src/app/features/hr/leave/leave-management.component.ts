import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { LeaveRequest } from '../../../core/models/erp-models';
import { MOCK_LEAVE_REQUESTS } from '../../../core/mock/mock-data';

@Component({
  selector: 'app-leave-management',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './leave-management.component.html'
})
export class LeaveManagementComponent {
  requests = signal<LeaveRequest[]>(MOCK_LEAVE_REQUESTS);
  showModal = signal(false);

  newReq: Partial<LeaveRequest> = {
    leaveType: 'Annual',
    startDate: '2026-08-20',
    endDate: '2026-08-25',
    reason: ''
  };

  openRequestModal() {
    this.showModal.set(true);
  }

  submitRequest() {
    const item: LeaveRequest = {
      id: `lv-${Date.now()}`,
      employeeId: 'emp-101',
      employeeName: 'Ahmed Hamdi',
      avatar: 'https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=150',
      leaveType: this.newReq.leaveType as any,
      startDate: this.newReq.startDate!,
      endDate: this.newReq.endDate!,
      daysCount: 5,
      reason: this.newReq.reason || 'Personal request',
      status: 'Pending',
      appliedDate: new Date().toISOString().split('T')[0]
    };
    this.requests.update(list => [item, ...list]);
    this.showModal.set(false);
  }

  approve(id: string) {
    this.requests.update(list => list.map(r => r.id === id ? { ...r, status: 'Approved' } as LeaveRequest : r));
  }

  reject(id: string) {
    this.requests.update(list => list.map(r => r.id === id ? { ...r, status: 'Rejected' } as LeaveRequest : r));
  }
}
