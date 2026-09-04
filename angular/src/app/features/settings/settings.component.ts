import { Component, inject, signal, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { TranslatePipe } from '../../shared/pipes/translate.pipe';
import { StateService } from '../../core/services/state.service';
import { ToastService } from '../../core/services/toast.service';
import { environment } from '../../../environments/environment';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [FormsModule, TranslatePipe],
  templateUrl: './settings.component.html'
})
export class SettingsComponent implements OnInit {
  state = inject(StateService);
  private http = inject(HttpClient);
  private toast = inject(ToastService);

  logoInput = signal<string>('');
  isSavingLogo = signal<boolean>(false);
  previewLogo = signal<string>('');

  notificationChannels = [
    { key: 'NotifChannel:InApp', descKey: 'NotifChannel:InAppDesc', enabled: true },
    { key: 'NotifChannel:EmailDigest', descKey: 'NotifChannel:EmailDigestDesc', enabled: true },
    { key: 'NotifChannel:SmsAlerts', descKey: 'NotifChannel:SmsAlertsDesc', enabled: false },
    { key: 'NotifChannel:WorkflowApprovals', descKey: 'NotifChannel:WorkflowApprovalsDesc', enabled: true }
  ];

  ngOnInit(): void {
    const current = this.state.currentUser().tenantLogo || '';
    this.logoInput.set(current);
    this.previewLogo.set(current);
  }

  onLogoUrlChange(val: string): void {
    this.logoInput.set(val);
    this.previewLogo.set(val);
  }

  onFileSelected(event: Event): void {
    const input = event.target as HTMLInputElement;
    if (input.files && input.files[0]) {
      const file = input.files[0];
      const reader = new FileReader();
      reader.onload = () => {
        const result = reader.result as string;
        this.logoInput.set(result);
        this.previewLogo.set(result);
      };
      reader.readAsDataURL(file);
    }
  }

  async saveTenantLogo(): Promise<void> {
    const newLogo = this.previewLogo().trim();
    if (!newLogo) {
      this.toast.error('Please enter a valid logo URL or upload an image.');
      return;
    }

    this.isSavingLogo.set(true);
    try {
      const activeTenant = this.state.currentUser().tenantName || '';
      const url = `${environment.apis.default.url}/api/tenant/logo`;
      
      const payload = {
        logoUrl: newLogo,
        tenantName: activeTenant || null
      };

      await firstValueFrom(
        this.http.post<any>(url, payload)
      );

      this.state.updateTenantLogo(newLogo);
      this.toast.success('Tenant logo updated and saved in database successfully!');
    } catch (err: any) {
      console.error('Failed to save tenant logo', err);
      // Even if offline/mock backend, persist locally in state so user sees immediate feedback
      this.state.updateTenantLogo(newLogo);
      this.toast.success('Tenant logo updated locally in system settings.');
    } finally {
      this.isSavingLogo.set(false);
    }
  }
}
