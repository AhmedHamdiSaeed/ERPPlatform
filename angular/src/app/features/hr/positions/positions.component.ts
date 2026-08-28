import { Component, inject, signal } from '@angular/core';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ToastService } from '../../../core/services/toast.service';

export interface JobPosition {
  id: string;
  title: string;
  code: string;
  departmentName: string;
  minSalary: number;
  maxSalary: number;
  status: string;
}

@Component({
  selector: 'app-positions',
  standalone: true,
  imports: [CommonModule, FormsModule],
  templateUrl: './positions.component.html'
})
export class PositionsComponent {
  private toast = inject(ToastService);

  positions = signal<JobPosition[]>([
    { id: 'pos-1', title: 'Senior Software Architect', code: 'POS-ENG-01', departmentName: 'Information Technology', minSalary: 12000, maxSalary: 18000, status: 'Active' },
    { id: 'pos-2', title: 'HR Manager', code: 'POS-HR-01', departmentName: 'Human Resources', minSalary: 9000, maxSalary: 14000, status: 'Active' },
    { id: 'pos-3', title: 'Logistics Lead', code: 'POS-SCM-02', departmentName: 'Supply Chain & Logistics', minSalary: 8000, maxSalary: 12000, status: 'Active' },
    { id: 'pos-4', title: 'Chief Accountant', code: 'POS-FIN-01', departmentName: 'Finance & Accounting', minSalary: 10000, maxSalary: 15000, status: 'Active' }
  ]);

  showModal = signal(false);
  newPos: Partial<JobPosition> = { title: '', code: '', departmentName: 'Information Technology', minSalary: 6000, maxSalary: 10000, status: 'Active' };

  openAddModal() {
    this.newPos = { code: `POS-${Math.floor(Math.random() * 900 + 100)}`, departmentName: 'Information Technology', minSalary: 8000, maxSalary: 14000, status: 'Active' };
    this.showModal.set(true);
  }

  savePosition() {
    if (!this.newPos.title) {
      this.toast.warning('Please enter a position title.');
      return;
    }
    this.positions.update(list => [{ ...this.newPos, id: `pos-${Date.now()}` } as JobPosition, ...list]);
    this.toast.success('Job position registered.');
    this.showModal.set(false);
  }
}
