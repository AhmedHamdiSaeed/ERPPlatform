import { Component, signal } from '@angular/core';
import { RouterModule } from '@angular/router';
import { NotificationItem } from '../../core/models/erp-models';
import { MOCK_NOTIFICATIONS } from '../../core/mock/mock-data';

@Component({
  selector: 'app-notification-center',
  standalone: true,
  imports: [RouterModule],
  template: `
    <div class="space-y-6 animate-fade-in pb-8">
      
      <div class="flex flex-col sm:flex-row sm:items-center justify-between gap-4">
        <div>
          <h1 class="text-xl font-extrabold text-[var(--text-main)] tracking-tight">Notification Center</h1>
          <p class="text-xs text-[var(--text-muted)] mt-0.5">All system alerts, workflow approvals, HR updates, inventory alerts, and AI insights in one unified feed.</p>
        </div>

        <div class="flex items-center gap-2">
          <button (click)="markAllRead()" class="btn-outline text-xs">
            <i class="pi pi-check-circle"></i> Mark All Read
          </button>
        </div>
      </div>

      <!-- Notification Feed -->
      <div class="space-y-3">
        @for (notif of notifications(); track notif.id) {
          <div 
            (click)="markRead(notif.id)"
            class="card-panel flex items-start gap-4 hover:border-blue-500 transition-all cursor-pointer group"
            [class.bg-blue-50]="!notif.read">
            
            <!-- Icon Badge -->
            <div class="shrink-0 w-10 h-10 rounded-xl flex items-center justify-center text-sm shadow-2xs"
              [class.bg-amber-100]="notif.type === 'Workflow Approval'" [class.text-amber-600]="notif.type === 'Workflow Approval'"
              [class.bg-emerald-100]="notif.type === 'Inventory'" [class.text-emerald-600]="notif.type === 'Inventory'"
              [class.bg-indigo-100]="notif.type === 'AI'" [class.text-indigo-600]="notif.type === 'AI'"
              [class.bg-blue-100]="notif.type === 'HR'" [class.text-blue-600]="notif.type === 'HR'"
              [class.bg-rose-100]="notif.type === 'Security'" [class.text-rose-600]="notif.type === 'Security'"
              [class.bg-slate-100]="notif.type === 'System'" [class.text-slate-600]="notif.type === 'System'">
              <i [class]="'pi ' + getIcon(notif.type)"></i>
            </div>

            <div class="flex-1 min-w-0">
              <div class="flex items-center justify-between gap-2">
                <div class="flex items-center gap-2">
                  <h3 class="font-bold text-xs text-[var(--text-main)] group-hover:text-blue-600 transition-colors">{{ notif.title }}</h3>
                  @if (!notif.read) {
                    <span class="w-2 h-2 rounded-full bg-blue-600 shrink-0"></span>
                  }
                </div>
                <span class="text-[10px] text-slate-400 shrink-0">{{ notif.timestamp }}</span>
              </div>
              <p class="text-xs text-[var(--text-muted)] mt-0.5 truncate">{{ notif.message }}</p>
              @if (notif.link) {
                <a [routerLink]="notif.link" class="text-[11px] text-blue-600 font-semibold hover:underline mt-1 inline-block">View Details →</a>
              }
            </div>

            <!-- Delete Action -->
            <button (click)="$event.stopPropagation(); dismiss(notif.id)" class="shrink-0 text-slate-400 hover:text-rose-500 opacity-0 group-hover:opacity-100 transition-opacity p-1">
              <i class="pi pi-times text-xs"></i>
            </button>
          </div>
        } @empty {
          <div class="card-panel text-center py-16 space-y-2">
            <i class="pi pi-bell-slash text-4xl text-slate-300"></i>
            <p class="text-xs text-[var(--text-muted)] font-medium">No notifications. You're all caught up!</p>
          </div>
        }
      </div>

    </div>
  `
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
