import { Component, signal } from '@angular/core';
import { CommonModule } from '@angular/common'; // Needed for pipes (number, date, etc.)
import { FormsModule } from '@angular/forms';
import { Department } from '../../../core/models/erp-models';
import { MOCK_DEPARTMENTS } from '../../../core/mock/mock-data';

@Component({
  selector: 'app-department-list',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './department-list.component.html'
})
export class DepartmentListComponent {
  departments = signal<Department[]>(MOCK_DEPARTMENTS);
  showModal = signal(false);

  newDept: Partial<Department> = {};

  openAddModal() {
    this.newDept = {
      id: `dept-${Date.now()}`,
      employeeCount: 0
    };
    this.showModal.set(true);
  }

  saveDepartment() {
    this.departments.update(list => [...list, this.newDept as Department]);
    this.showModal.set(false);
  }
}
