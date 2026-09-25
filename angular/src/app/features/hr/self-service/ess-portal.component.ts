import { Component, OnInit, inject } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { HrApiService } from '../../../core/services/api/hr-api.service';
import { Employee, LeaveRequest, AttendanceRecord, EmployeeLoan, EmployeeDocument } from '../../../core/models/erp-models';
import { StateService } from '../../../core/services/state.service';
import { TranslatePipe } from '../../../shared/pipes/translate.pipe';

@Component({
  selector: 'app-ess-portal',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslatePipe],
  templateUrl: './ess-portal.component.html'
})
export class EssPortalComponent implements OnInit {
  private hrApi = inject(HrApiService);
  state = inject(StateService);

  loading = false;
  currentEmployee: Employee | null = null;
  myLeaves: LeaveRequest[] = [];
  myAttendance: AttendanceRecord[] = [];
  myLoans: EmployeeLoan[] = [];
  myDocuments: EmployeeDocument[] = [];

  // Quick Action Modals
  showLeaveModal = false;
  leaveForm = {
    leaveType: 'Annual',
    startDate: '2026-10-01',
    endDate: '2026-10-05',
    daysCount: 5,
    reason: 'Personal vacation'
  };

  showLoanModal = false;
  loanForm = {
    loanType: 'Personal',
    principalAmount: 10000,
    totalInstallments: 12,
    startDeductionPeriod: '2026-11-01',
    purpose: 'Family emergency support'
  };

  showPunchSuccess = false;
  punchStatusMsg = '';

  ngOnInit(): void {
    this.loadMyProfile();
  }

  async loadMyProfile(): Promise<void> {
    this.loading = true;
    try {
      const emps = await this.hrApi.getEmployees();
      if (emps && emps.length > 0) {
        this.currentEmployee = emps[0]; // Load primary active employee identity
        await this.loadEmployeeData(this.currentEmployee.id);
      }
    } catch (err) {
      console.error('Error loading ESS data', err);
    } finally {
      this.loading = false;
    }
  }

  async loadEmployeeData(empId: string): Promise<void> {
    const [lvs, att, lns, docs] = await Promise.all([
      this.hrApi.getLeaveRequests(),
      this.hrApi.getAttendance(empId),
      this.hrApi.getEmployeeLoans(empId),
      this.hrApi.getEmployeeDocuments(empId)
    ]);
    this.myLeaves = (lvs || []).filter(l => l.employeeId === empId);
    this.myAttendance = att || [];
    this.myLoans = lns || [];
    this.myDocuments = docs || [];
  }

  async quickClockIn(): Promise<void> {
    if (!this.currentEmployee) return;
    try {
      await this.hrApi.checkIn(this.currentEmployee.id, this.currentEmployee.name, this.currentEmployee.departmentName);
      this.punchStatusMsg = 'Clock-in recorded successfully at ' + new Date().toLocaleTimeString();
      this.showPunchSuccess = true;
      setTimeout(() => this.showPunchSuccess = false, 4000);
      this.loadEmployeeData(this.currentEmployee.id);
    } catch (err) {
      console.error('Error clocking in', err);
    }
  }

  async submitLeaveRequest(): Promise<void> {
    if (!this.currentEmployee) return;
    try {
      await this.hrApi.createLeaveRequest({
        employeeId: this.currentEmployee.id,
        employeeName: this.currentEmployee.name,
        leaveType: this.leaveForm.leaveType as LeaveRequest['leaveType'],
        startDate: this.leaveForm.startDate,
        endDate: this.leaveForm.endDate,
        daysCount: this.leaveForm.daysCount,
        reason: this.leaveForm.reason,
        status: 'Pending'
      });
      this.showLeaveModal = false;
      this.loadEmployeeData(this.currentEmployee.id);
    } catch (err) {
      console.error('Error submitting leave', err);
    }
  }

  async submitLoanRequest(): Promise<void> {
    if (!this.currentEmployee) return;
    try {
      await this.hrApi.requestEmployeeLoan({
        employeeId: this.currentEmployee.id,
        loanType: this.loanForm.loanType,
        principalAmount: this.loanForm.principalAmount,
        totalInstallments: this.loanForm.totalInstallments,
        startDeductionPeriod: this.loanForm.startDeductionPeriod,
        purpose: this.loanForm.purpose
      });
      this.showLoanModal = false;
      this.loadEmployeeData(this.currentEmployee.id);
    } catch (err) {
      console.error('Error submitting loan', err);
    }
  }
}
