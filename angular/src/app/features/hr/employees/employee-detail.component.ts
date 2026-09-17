import { Component, inject, signal } from '@angular/core';
import { TranslatePipe } from '../../../shared/pipes/translate.pipe';
import { CommonModule } from '@angular/common'; // Needed for pipes (number, date, etc.)
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { Employee, AttendanceRecord, LeaveRequest } from '../../../core/models/erp-models';
import { HrApiService } from '../../../core/services/api/hr-api.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-employee-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, TranslatePipe],
  templateUrl: './employee-detail.component.html'
})
export class EmployeeDetailComponent {
  private route = inject(ActivatedRoute);
  private hrApi = inject(HrApiService);
  private toast = inject(ToastService);

  employee = signal<Employee | undefined>(undefined);
  activeTab = signal('HR:TabOverview');
  loading = signal(false);

  showEditModal = signal(false);
  editEmp: Partial<Employee> = {};

  attendance = signal<AttendanceRecord[]>([]);
  leaveRequests = signal<LeaveRequest[]>([]);

  tabs = [
    'HR:TabOverview',
    'HR:TabPersonalInfo',
    'HR:TabEmployment',
    'HR:Attendance',
    'HR:Leave',
    'HR:TabDocuments',
    'HR:Performance',
    'HR:TabActivity'
  ];

  constructor() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.load(id);
    } else {
      this.toast.error('No employee id provided.');
    }
  }

  async load(id: string) {
    this.loading.set(true);
    try {
      const emp = await this.hrApi.getEmployee(id);
      this.employee.set(emp);
      // Both queries run in parallel; attendance is filtered server-side by employee id.
      const [att, leave] = await Promise.all([
        this.hrApi.getAttendance(id),
        this.hrApi.getLeaveRequests()
      ]);
      this.attendance.set(att);
      // Leave requests aren't scoped by employee server-side, so filter client-side.
      this.leaveRequests.set(leave.filter(l => l.employeeId === id));
    } catch (e) {
      console.error('Failed to load employee', e);
      this.toast.error('Could not load the employee from the server.');
    } finally {
      this.loading.set(false);
    }
  }

  openEditModal() {
    const emp = this.employee();
    if (!emp) return;
    this.editEmp = { ...emp };
    this.showEditModal.set(true);
  }

  async saveEmployee() {
    const emp = this.employee();
    if (!emp?.id) return;
    try {
      await this.hrApi.updateEmployee(emp.id, this.editEmp);
      this.toast.success('Employee profile updated successfully.');
      this.showEditModal.set(false);
      await this.load(emp.id);
    } catch (e) {
      console.error('Failed to save employee', e);
      this.toast.error('Failed to save the employee record.');
    }
  }

  sendEmail() {
    const emp = this.employee();
    if (emp?.email) {
      window.location.href = `mailto:${emp.email}`;
    }
  }
}

