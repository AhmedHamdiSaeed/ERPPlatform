import { Component, inject, signal } from '@angular/core';
import { LocalizationPipe } from '@abp/ng.core';
import { FormsModule } from '@angular/forms';
import { AttendanceRecord } from '../../../core/models/erp-models';
import { HrApiService } from '../../../core/services/api/hr-api.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-attendance-list',
  standalone: true,
  imports: [FormsModule, LocalizationPipe],
  templateUrl: './attendance-list.component.html'
})
export class AttendanceListComponent {
  private hrApi = inject(HrApiService);
  private toast = inject(ToastService);

  records = signal<AttendanceRecord[]>([]);
  statusFilter = 'ALL';
  loading = signal(false);

  constructor() {
    this.load();
  }

  async load() {
    this.loading.set(true);
    try {
      this.records.set(await this.hrApi.getAttendance());
    } catch (e) {
      console.error('Failed to load attendance', e);
      this.toast.error('Could not load attendance records from the server.');
    } finally {
      this.loading.set(false);
    }
  }

  filteredRecords() {
    return this.records().filter(r => this.statusFilter === 'ALL' || r.status === this.statusFilter);
  }

  logCheckIn() {
    const time = new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
    this.toast.success(`Check-in logged for Ahmed Hamdi at ${time}`, 'Attendance Logged');
  }
}
