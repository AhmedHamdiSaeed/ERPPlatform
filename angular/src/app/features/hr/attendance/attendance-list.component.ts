import { Component, inject, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AttendanceRecord } from '../../../core/models/erp-models';
import { MOCK_ATTENDANCE } from '../../../core/mock/mock-data';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-attendance-list',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './attendance-list.component.html'
})
export class AttendanceListComponent {
  private toast = inject(ToastService);

  records = signal<AttendanceRecord[]>(MOCK_ATTENDANCE);
  statusFilter = 'ALL';

  filteredRecords() {
    return this.records().filter(r => this.statusFilter === 'ALL' || r.status === this.statusFilter);
  }

  logCheckIn() {
    const time = new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    this.toast.success(`Check-in logged for Ahmed Hamdi at ${time}`, 'Attendance Logged');
  }
}
