import { Component, inject, signal } from '@angular/core';
import { TranslatePipe } from '../../../shared/pipes/translate.pipe';
import { CommonModule } from '@angular/common'; // Needed for pipes (number, date, etc.)
import { FormsModule } from '@angular/forms';
import { Department } from '../../../core/models/erp-models';
import { HrApiService } from '../../../core/services/api/hr-api.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-department-list',
  standalone: true,
  imports: [CommonModule, FormsModule, TranslatePipe],
  templateUrl: './department-list.component.html'
})
export class DepartmentListComponent {
  private hrApi = inject(HrApiService);
  private toast = inject(ToastService);

  departments = signal<Department[]>([]);
  showModal = signal(false);

  newDept: Partial<Department> = {};

  constructor() {
    this.loadDepartments();
  }

  async loadDepartments() {
    try {
      this.departments.set(await this.hrApi.getDepartments());
    } catch (e) {
      console.error('Failed to load departments', e);
      this.toast.error('Could not load departments from the server.');
    }
  }

  openAddModal() {
    this.newDept = {
      employeeCount: 0,
      budget: 0
    };
    this.showModal.set(true);
  }

  async saveDepartment() {
    try {
      await this.hrApi.createDepartment(this.newDept);
      await this.loadDepartments();
      this.toast.success('Department created successfully.');
    } catch (e) {
      console.error('Failed to create department', e);
      this.toast.error('Failed to create the department.');
    }
    this.showModal.set(false);
  }
}
