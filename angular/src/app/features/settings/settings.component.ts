import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { LocalizationPipe } from '@abp/ng.core';
import { StateService } from '../../core/services/state.service';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [FormsModule, LocalizationPipe],
  templateUrl: './settings.component.html'
})
export class SettingsComponent {
  state = inject(StateService);

  notificationChannels = [
    { key: 'NotifChannel:InApp', descKey: 'NotifChannel:InAppDesc', enabled: true },
    { key: 'NotifChannel:EmailDigest', descKey: 'NotifChannel:EmailDigestDesc', enabled: true },
    { key: 'NotifChannel:SmsAlerts', descKey: 'NotifChannel:SmsAlertsDesc', enabled: false },
    { key: 'NotifChannel:WorkflowApprovals', descKey: 'NotifChannel:WorkflowApprovalsDesc', enabled: true }
  ];
}
