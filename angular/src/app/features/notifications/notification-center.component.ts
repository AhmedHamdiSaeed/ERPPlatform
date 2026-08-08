import { Component, signal } from '@angular/core';
import { RouterModule } from '@angular/router';
import { NotificationItem } from '../../core/models/erp-models';
import { MOCK_NOTIFICATIONS } from '../../core/mock/mock-data';

@Component({
  selector: 'app-notification-center',
  standalone: true,
  imports: [RouterModule],
  templateUrl: './notification-center.component.html'
})
export class NotificationCenterComponent {
  notifications = signal<NotificationItem[]>(MOCK_NOTIFICATIONS);

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
    this.notifications.update(list => list.map(n => n.id === id ? { ...n, read: true } : n));
  }

  markAllRead() {
    this.notifications.update(list => list.map(n => ({ ...n, read: true })));
  }

  dismiss(id: string) {
    this.notifications.update(list => list.filter(n => n.id !== id));
  }
}
