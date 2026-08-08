import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { StateService } from '../../core/services/state.service';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './settings.component.html'
})
export class SettingsComponent {
  state = inject(StateService);

  notificationChannels = [
    { label: 'In-App Notifications', description: 'Desktop and in-app push alerts.', enabled: true },
    { label: 'Email Digest', description: 'Daily email summary of activity.', enabled: true },
    { label: 'SMS Alerts', description: 'Critical alerts via text message.', enabled: false },
    { label: 'Workflow Approvals', description: 'Real-time approval reminders.', enabled: true }
  ];
}
