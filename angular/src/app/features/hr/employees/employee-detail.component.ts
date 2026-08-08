import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common'; // Needed for pipes (number, date, etc.)
import { ActivatedRoute, RouterModule } from '@angular/router';
import { Employee } from '../../../core/models/erp-models';
import { MOCK_EMPLOYEES, MOCK_LEAVE_REQUESTS, MOCK_ATTENDANCE } from '../../../core/mock/mock-data';

@Component({
  selector: 'app-employee-detail',
  standalone: true,
  imports: [CommonModule, RouterModule],
  templateUrl: './employee-detail.component.html'
})
export class EmployeeDetailComponent {
  private route = inject(ActivatedRoute);

  employee = signal<Employee | undefined>(undefined);
  activeTab = signal('Overview');

  tabs = [
    'Overview',
    'Personal Information',
    'Employment',
    'Attendance',
    'Leave',
    'Documents',
    'Performance',
    'Activity'
  ];

  mockAttendance = MOCK_ATTENDANCE;
  mockLeave = MOCK_LEAVE_REQUESTS;

  constructor() {
    const id = this.route.snapshot.paramMap.get('id');
    const match = MOCK_EMPLOYEES.find(e => e.id === id) || MOCK_EMPLOYEES[0];
    this.employee.set(match);
  }
}
