import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { AttendanceRecord } from '../../../core/models/erp-models';
import { MOCK_ATTENDANCE } from '../../../core/mock/mock-data';

@Component({
  selector: 'app-attendance-list',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './attendance-list.component.html'
})
export class AttendanceListComponent {
  records = signal<AttendanceRecord[]>(MOCK_ATTENDANCE);
  statusFilter = 'ALL';

  filteredRecords() {
    return this.records().filter(r => this.statusFilter === 'ALL' || r.status === this.statusFilter);
  }

  logCheckIn() {
    alert('Check-in logged for Ahmed Hamdi at ' + new Date().toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' }));
  }
}
