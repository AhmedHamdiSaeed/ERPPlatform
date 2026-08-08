import { Component, inject } from '@angular/core';
import { RouterModule } from '@angular/router';
import { StateService } from '../../../core/services/state.service';

interface NavGroup {
  label: string;
  icon: string;
  expanded?: boolean;
  items?: { label: string; link: string; icon: string; badge?: string }[];
  link?: string;
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [RouterModule],
  template: `
    <aside 
      [class.w-64]="state.sidebarExpanded()" 
      [class.w-20]="!state.sidebarExpanded()"
      class="h-[calc(100vh-4rem)] bg-[var(--bg-card)] border-r border-[var(--border-color)] transition-all duration-300 flex flex-col sticky top-16 z-20 select-none overflow-y-auto">
      
      <!-- Navigation Menu List -->
      <div class="p-3 space-y-1 flex-1">
        @for (group of menu; track group.label) {
          <!-- Single Link Item -->
          @if (!group.items) {
            <a 
              [routerLink]="group.link" 
              routerLinkActive="bg-blue-50 text-blue-600 dark:bg-blue-900/30 dark:text-blue-400 font-semibold"
              class="flex items-center gap-3 px-3 py-2.5 rounded-lg text-xs text-[var(--text-main)] hover:bg-slate-100 dark:hover:bg-slate-800 transition-colors group">
              <i [class]="'pi ' + group.icon + ' text-base shrink-0 group-hover:text-blue-600'"></i>
              @if (state.sidebarExpanded()) {
                <span class="truncate">{{ group.label }}</span>
              }
            </a>
          }

          <!-- Nested Submenu Group -->
          @if (group.items) {
            <div>
              <button 
                (click)="group.expanded = !group.expanded"
                class="w-full flex items-center justify-between px-3 py-2.5 rounded-lg text-xs text-[var(--text-main)] hover:bg-slate-100 dark:hover:bg-slate-800 transition-colors group cursor-pointer">
                <div class="flex items-center gap-3 min-w-0">
                  <i [class]="'pi ' + group.icon + ' text-base shrink-0 group-hover:text-blue-600'"></i>
                  @if (state.sidebarExpanded()) {
                    <span class="truncate font-medium">{{ group.label }}</span>
                  }
                </div>
                @if (state.sidebarExpanded()) {
                  <i [class]="group.expanded ? 'pi pi-chevron-down' : 'pi pi-chevron-right'" class="text-[10px] text-slate-400"></i>
                }
              </button>

              <!-- Submenu Items -->
              @if (group.expanded && state.sidebarExpanded()) {
                <div class="ml-4 pl-3 border-l border-slate-200 dark:border-slate-700 mt-1 space-y-1">
                  @for (sub of group.items; track sub.label) {
                    <a 
                      [routerLink]="sub.link" 
                      routerLinkActive="text-blue-600 dark:text-blue-400 font-semibold"
                      class="flex items-center justify-between px-2.5 py-1.5 rounded-md text-xs text-[var(--text-muted)] hover:text-[var(--text-main)] hover:bg-slate-100 dark:hover:bg-slate-800 transition-colors">
                      <span class="truncate">{{ sub.label }}</span>
                      @if (sub.badge) {
                        <span class="px-1.5 py-0.5 text-[9px] font-bold bg-blue-100 text-blue-700 rounded-full">{{ sub.badge }}</span>
                      }
                    </a>
                  }
                </div>
              }
            </div>
          }
        }
      </div>

      <!-- Quick Action / Footer Info -->
      @if (state.sidebarExpanded()) {
        <div class="p-3 border-t border-[var(--border-color)] bg-slate-50/50 dark:bg-slate-900/50">
          <div class="card-panel !p-3 text-center bg-gradient-to-br from-blue-50 to-indigo-50 dark:from-slate-800 dark:to-slate-800/60 border-blue-100 dark:border-slate-700">
            <i class="pi pi-shield text-blue-600 text-xl mb-1 block"></i>
            <p class="text-xs font-bold text-slate-800 dark:text-slate-200">ERP SaaS v9.3</p>
            <p class="text-[10px] text-slate-500 mt-0.5">Connected to ABP Backend</p>
          </div>
        </div>
      }
    </aside>
  `
})
export class SidebarComponent {
  state = inject(StateService);

  menu: NavGroup[] = [
    { label: 'Dashboard', icon: 'pi-home', link: '/dashboard' },
    {
      label: 'HR Module',
      icon: 'pi-users',
      expanded: true,
      items: [
        { label: 'Employees', link: '/hr/employees', icon: 'pi-user' },
        { label: 'Departments', link: '/hr/departments', icon: 'pi-building' },
        { label: 'Attendance', link: '/hr/attendance', icon: 'pi-calendar' },
        { label: 'Leave Management', link: '/hr/leave', icon: 'pi-briefcase' },
        { label: 'Recruitment', link: '/hr/recruitment', icon: 'pi-user-plus', badge: 'New' }
      ]
    },
    {
      label: 'Inventory',
      icon: 'pi-box',
      expanded: true,
      items: [
        { label: 'Products', link: '/inventory/products', icon: 'pi-list' },
        { label: 'Warehouses', link: '/inventory/warehouses', icon: 'pi-building' },
        { label: 'Stock Transfers', link: '/inventory/transfers', icon: 'pi-sync' },
        { label: 'Purchase Orders', link: '/inventory/purchase-orders', icon: 'pi-shopping-bag' }
      ]
    },
    {
      label: 'Workflow Engine',
      icon: 'pi-sitemap',
      expanded: true,
      items: [
        { label: 'Workflow Designer', link: '/workflow/designer', icon: 'pi-palette' },
        { label: 'My Tasks Inbox', link: '/workflow/tasks', icon: 'pi-check-square', badge: '3' },
        { label: 'Execution History', link: '/workflow/history', icon: 'pi-history' }
      ]
    },
    { label: 'AI Assistant', icon: 'pi-sparkles', link: '/ai-assistant' },
    { label: 'Reports Center', icon: 'pi-chart-bar', link: '/reports' },
    { label: 'Notifications', icon: 'pi-bell', link: '/notifications' },
    { label: 'Settings', icon: 'pi-cog', link: '/settings' }
  ];
}
