import { Component, inject } from '@angular/core';
import { RouterModule } from '@angular/router';
import { NotificationItem } from '../../core/models/erp-models';
import { StateService } from '../../core/services/state.service';

@Component({
  selector: 'app-notification-center',
  standalone: true,
  imports: [RouterModule],
  templateUrl: './notification-center.component.html'
})
export class NotificationCenterComponent {
  private state = inject(StateService);

  /** Re-exposes the global notifications signal so the header bell count stays in sync. */
  notifications = this.state.notifications;

  getIcon(type: string): string {
    const map: Record<string, string> = {
      'Workflow Approval': 'pi-check-square',
      'Inventory': 'pi-box',
      'AI': 'pi-sparkles',
      'HR': 'pi-users',
      'Security': 'pi-shield',
      'System': 'pi-cog'
    };
    return map[type] || 'pi-bell';
  }

  markRead(id: string) {
    this.state.markNotificationAsRead(id);
  }

  markAllRead() {
    this.state.markAllNotificationsAsRead();
  }

  dismiss(id: string) {
    this.state.dismissNotification(id);
  }
}
