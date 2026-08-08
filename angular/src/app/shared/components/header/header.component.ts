import { Component, inject } from '@angular/core';
import { RouterModule } from '@angular/router';
import { StateService } from '../../../core/services/state.service';

@Component({
  selector: 'app-header',
  standalone: true,
  imports: [RouterModule],
  template: `
    <header class="h-16 bg-[var(--bg-card)] border-b border-[var(--border-color)] px-4 flex items-center justify-between sticky top-0 z-30 shadow-xs">
      <!-- Left: Logo & Sidebar Toggle -->
      <div class="flex items-center gap-3">
        <button 
          (click)="state.toggleSidebar()" 
          class="p-2 rounded-md hover:bg-slate-100 dark:hover:bg-slate-800 text-[var(--text-muted)] transition-colors focus:outline-hidden"
          title="Toggle Sidebar">
          <i class="pi pi-bars text-lg"></i>
        </button>

        <a routerLink="/dashboard" class="flex items-center gap-2.5 group decoration-none">
          <div class="w-9 h-9 rounded-lg bg-blue-600 flex items-center justify-center text-white font-bold text-lg shadow-sm group-hover:scale-105 transition-transform">
            <i class="pi pi-box"></i>
          </div>
          <div class="hidden sm:block">
            <span class="font-bold text-slate-800 dark:text-slate-100 text-base leading-tight block">ERP Platform</span>
            <span class="text-[10px] text-blue-600 font-semibold tracking-widest uppercase block">Enterprise Suite</span>
          </div>
        </a>
      </div>

      <!-- Center: Global Search Input -->
      <div class="flex-1 max-w-xl mx-4 hidden md:block">
        <button 
          (click)="state.toggleGlobalSearch(true)"
          class="w-full flex items-center justify-between px-3.5 py-2 rounded-lg bg-slate-100 dark:bg-slate-800/80 border border-slate-200 dark:border-slate-700 text-slate-500 hover:border-blue-500 transition-colors text-sm cursor-pointer">
          <div class="flex items-center gap-2">
            <i class="pi pi-search text-slate-400"></i>
            <span>Search employees, products, workflows, orders...</span>
          </div>
          <kbd class="px-2 py-0.5 text-[11px] font-mono font-semibold bg-white dark:bg-slate-700 border border-slate-300 dark:border-slate-600 rounded text-slate-500 shadow-2xs">Ctrl K</kbd>
        </button>
      </div>

      <!-- Right: Controls & Profile -->
      <div class="flex items-center gap-2">
        <!-- Global Search Mobile Button -->
        <button 
          (click)="state.toggleGlobalSearch(true)"
          class="md:hidden p-2 rounded-md hover:bg-slate-100 dark:hover:bg-slate-800 text-[var(--text-muted)]">
          <i class="pi pi-search text-lg"></i>
        </button>

        <!-- AI Assistant Button -->
        <button 
          (click)="state.toggleAiWidget()"
          class="relative flex items-center gap-1.5 px-3 py-1.5 rounded-lg bg-gradient-to-r from-indigo-500 to-blue-600 text-white font-medium text-xs shadow-xs hover:opacity-95 transition-opacity cursor-pointer">
          <i class="pi pi-sparkles"></i>
          <span class="hidden sm:inline">AI Assistant</span>
        </button>

        <!-- Light/Dark Theme Toggle -->
        <button 
          (click)="state.toggleTheme()" 
          class="p-2 rounded-md hover:bg-slate-100 dark:hover:bg-slate-800 text-[var(--text-muted)] transition-colors"
          [title]="'Switch to ' + (state.theme() === 'light' ? 'Dark' : 'Light') + ' mode'">
          <i [class]="state.theme() === 'light' ? 'pi pi-moon' : 'pi pi-sun'" class="text-lg"></i>
        </button>

        <!-- Language / RTL Switcher -->
        <button 
          (click)="state.setLanguage(state.lang() === 'en' ? 'ar' : 'en')"
          class="px-2.5 py-1 rounded-md border border-[var(--border-color)] text-xs font-semibold text-[var(--text-main)] hover:bg-slate-100 dark:hover:bg-slate-800 transition-colors">
          {{ state.lang() === 'en' ? 'العربية' : 'English' }}
        </button>

        <!-- Notifications Dropdown Toggle -->
        <div class="relative">
          <button 
            (click)="showNotifDropdown = !showNotifDropdown"
            class="p-2 rounded-md hover:bg-slate-100 dark:hover:bg-slate-800 text-[var(--text-muted)] relative transition-colors">
            <i class="pi pi-bell text-lg"></i>
            @if (state.unreadNotificationCount() > 0) {
              <span class="absolute top-1 right-1 w-2 h-2 bg-red-500 rounded-full ring-2 ring-white"></span>
            }
          </button>

          <!-- Notifications Popup -->
          @if (showNotifDropdown) {
            <div class="absolute right-0 mt-2 w-80 bg-[var(--bg-card)] border border-[var(--border-color)] rounded-xl shadow-lg z-50 overflow-hidden animate-fade-in">
              <div class="p-3 border-b border-[var(--border-color)] flex items-center justify-between">
                <h4 class="font-semibold text-sm">Notifications</h4>
                <button (click)="state.markAllNotificationsAsRead()" class="text-xs text-blue-600 hover:underline">Mark all read</button>
              </div>
              <div class="max-h-72 overflow-y-auto divide-y divide-[var(--border-color)]">
                @for (n of state.notifications(); track n.id) {
                  <div (click)="state.markNotificationAsRead(n.id); showNotifDropdown = false" class="p-3 hover:bg-slate-50 dark:hover:bg-slate-800/50 cursor-pointer transition-colors" [class.opacity-60]="n.read">
                    <div class="flex items-start gap-2.5">
                      <div class="w-7 h-7 rounded-full bg-blue-100 text-blue-600 flex items-center justify-center text-xs shrink-0 mt-0.5">
                        <i class="pi pi-info-circle"></i>
                      </div>
                      <div>
                        <p class="text-xs font-semibold text-[var(--text-main)]">{{ n.title }}</p>
                        <p class="text-xs text-[var(--text-muted)] line-clamp-2 mt-0.5">{{ n.message }}</p>
                        <span class="text-[10px] text-slate-400 mt-1 block">{{ n.timestamp }}</span>
                      </div>
                    </div>
                  </div>
                } @empty {
                  <div class="p-6 text-center text-xs text-[var(--text-muted)]">
                    No notifications right now.
                  </div>
                }
              </div>
              <div class="p-2 border-t border-[var(--border-color)] text-center bg-slate-50 dark:bg-slate-900/50">
                <a routerLink="/notifications" (click)="showNotifDropdown = false" class="text-xs text-blue-600 font-medium hover:underline">View Notification Center</a>
              </div>
            </div>
          }
        </div>

        <!-- User Profile Dropdown -->
        <div class="relative ml-1">
          <button (click)="showUserMenu = !showUserMenu" class="flex items-center gap-2 p-1 rounded-lg hover:bg-slate-100 dark:hover:bg-slate-800 transition-colors">
            <img [src]="state.currentUser().avatar" [alt]="state.currentUser().name" class="w-8 h-8 rounded-full object-cover ring-2 ring-blue-500/30" />
            <div class="hidden lg:block text-left">
              <span class="text-xs font-semibold block leading-tight text-[var(--text-main)]">{{ state.currentUser().name }}</span>
              <span class="text-[10px] text-[var(--text-muted)] block">{{ state.currentUser().role }}</span>
            </div>
            <i class="pi pi-chevron-down text-xs text-[var(--text-muted)] hidden lg:inline"></i>
          </button>

          @if (showUserMenu) {
            <div class="absolute right-0 mt-2 w-56 bg-[var(--bg-card)] border border-[var(--border-color)] rounded-xl shadow-lg z-50 p-1 animate-fade-in">
              <div class="px-3 py-2 border-b border-[var(--border-color)] mb-1">
                <p class="text-xs font-bold text-[var(--text-main)]">{{ state.currentUser().name }}</p>
                <p class="text-[11px] text-[var(--text-muted)] truncate">{{ state.currentUser().email }}</p>
              </div>
              <a routerLink="/settings" (click)="showUserMenu = false" class="flex items-center gap-2.5 px-3 py-2 text-xs rounded-md hover:bg-slate-100 dark:hover:bg-slate-800 text-[var(--text-main)]">
                <i class="pi pi-user text-slate-400"></i> My Profile & Settings
              </a>
              <a routerLink="/workflow/tasks" (click)="showUserMenu = false" class="flex items-center gap-2.5 px-3 py-2 text-xs rounded-md hover:bg-slate-100 dark:hover:bg-slate-800 text-[var(--text-main)]">
                <i class="pi pi-check-square text-slate-400"></i> My Tasks Inbox
              </a>
              <div class="border-t border-[var(--border-color)] my-1"></div>
              <a routerLink="/auth/login" (click)="showUserMenu = false" class="flex items-center gap-2.5 px-3 py-2 text-xs rounded-md hover:bg-red-50 text-red-600">
                <i class="pi pi-sign-out"></i> Log Out
              </a>
            </div>
          }
        </div>

      </div>
    </header>
  `
})
export class HeaderComponent {
  state = inject(StateService);
  showNotifDropdown = false;
  showUserMenu = false;
}
