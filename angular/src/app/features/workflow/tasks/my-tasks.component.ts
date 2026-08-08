import { Component, signal } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { WorkflowTask } from '../../../core/models/erp-models';
import { MOCK_TASKS } from '../../../core/mock/mock-data';

@Component({
  selector: 'app-my-tasks',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './my-tasks.component.html'
})
export class MyTasksComponent {
  tasks = signal<WorkflowTask[]>(MOCK_TASKS);

  approve(id: string) {
    this.tasks.update(list => list.filter(t => t.id !== id));
    alert('Task approved successfully!');
  }

  reject(id: string) {
    this.tasks.update(list => list.filter(t => t.id !== id));
    alert('Task rejected.');
  }

  requestChanges(id: string) {
    alert('Requested modifications from submitter.');
  }
}
