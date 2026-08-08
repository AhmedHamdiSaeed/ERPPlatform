import { Component, inject } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { StateService } from '../../core/services/state.service';

@Component({
  selector: 'app-settings',
  standalone: true,
  imports: [FormsModule],
  template: `
    <div class="space-y-6 animate-fade-in pb-8">
      
      <div>
        <h1 class="text-xl font-extrabold text-[var(--text-main)] tracking-tight">System Settings & Preferences</h1>
        <p class="text-xs text-[var(--text-muted)] mt-0.5">Configure application appearance, localization, notification channels, user profile, and security settings.</p>
      </div>

      <div class="grid grid-cols-1 lg:grid-cols-2 gap-6">
        
        <!-- Appearance Settings Card -->
        <div class="card-panel space-y-5">
          <div class="flex items-center gap-3 border-b border-[var(--border-color)] pb-3">
            <div class="w-9 h-9 rounded-xl bg-indigo-100 dark:bg-indigo-900/30 text-indigo-600 flex items-center justify-center">
              <i class="pi pi-palette"></i>
            </div>
            <div>
              <h3 class="font-bold text-sm text-[var(--text-main)]">Appearance & Theme</h3>
              <p class="text-[11px] text-[var(--text-muted)]">Customize the visual experience.</p>
            </div>
          </div>

          <div class="space-y-4 text-xs">
            <div class="flex items-center justify-between">
              <div>
                <span class="font-bold text-[var(--text-main)] block">Dark Mode</span>
                <span class="text-[11px] text-[var(--text-muted)]">Switch between light and dark color schemes.</span>
              </div>
              <button (click)="state.toggleTheme()" class="relative w-12 h-6 rounded-full transition-colors cursor-pointer" [class.bg-blue-600]="state.isDark()" [class.bg-slate-300]="!state.isDark()">
                <span class="absolute top-0.5 w-5 h-5 bg-white rounded-full shadow-md transition-transform" [class.translate-x-6]="state.isDark()" [class.translate-x-0.5]="!state.isDark()"></span>
              </button>
            </div>

            <div class="flex items-center justify-between">
              <div>
                <span class="font-bold text-[var(--text-main)] block">Compact Sidebar</span>
                <span class="text-[11px] text-[var(--text-muted)]">Collapse sidebar to icon-only mode.</span>
              </div>
              <button (click)="state.toggleSidebar()" class="relative w-12 h-6 rounded-full transition-colors cursor-pointer" [class.bg-blue-600]="state.sidebarCollapsed()" [class.bg-slate-300]="!state.sidebarCollapsed()">
                <span class="absolute top-0.5 w-5 h-5 bg-white rounded-full shadow-md transition-transform" [class.translate-x-6]="state.sidebarCollapsed()" [class.translate-x-0.5]="!state.sidebarCollapsed()"></span>
              </button>
            </div>
          </div>
        </div>

        <!-- Localization Settings Card -->
        <div class="card-panel space-y-5">
          <div class="flex items-center gap-3 border-b border-[var(--border-color)] pb-3">
            <div class="w-9 h-9 rounded-xl bg-emerald-100 dark:bg-emerald-900/30 text-emerald-600 flex items-center justify-center">
              <i class="pi pi-globe"></i>
            </div>
            <div>
              <h3 class="font-bold text-sm text-[var(--text-main)]">Localization & Language</h3>
              <p class="text-[11px] text-[var(--text-muted)]">Language, RTL, and regional formatting.</p>
            </div>
          </div>

          <div class="space-y-4 text-xs">
            <div>
              <span class="font-bold text-[var(--text-main)] block mb-1.5">Display Language</span>
              <div class="flex gap-2">
                <button (click)="state.setLanguage('en')" class="flex-1 py-2.5 rounded-xl border text-center font-bold transition-colors" [class.bg-blue-600]="state.language() === 'en'" [class.text-white]="state.language() === 'en'" [class.border-blue-600]="state.language() === 'en'" [class.bg-slate-100]="state.language() !== 'en'" [class.dark:bg-slate-800]="state.language() !== 'en'">
                  🇺🇸 English
                </button>
                <button (click)="state.setLanguage('ar')" class="flex-1 py-2.5 rounded-xl border text-center font-bold transition-colors" [class.bg-blue-600]="state.language() === 'ar'" [class.text-white]="state.language() === 'ar'" [class.border-blue-600]="state.language() === 'ar'" [class.bg-slate-100]="state.language() !== 'ar'" [class.dark:bg-slate-800]="state.language() !== 'ar'">
                  🇸🇦 العربية
                </button>
              </div>
            </div>

            <div>
              <span class="font-bold text-[var(--text-main)] block mb-1.5">Timezone</span>
              <select class="w-full px-3 py-2 bg-slate-100 dark:bg-slate-800 border border-slate-200 dark:border-slate-700 rounded-xl">
                <option>(UTC+02:00) Cairo</option>
                <option>(UTC+03:00) Riyadh</option>
                <option>(UTC+00:00) London</option>
                <option>(UTC-05:00) New York</option>
              </select>
            </div>

            <div>
              <span class="font-bold text-[var(--text-main)] block mb-1.5">Date Format</span>
              <select class="w-full px-3 py-2 bg-slate-100 dark:bg-slate-800 border border-slate-200 dark:border-slate-700 rounded-xl">
                <option>YYYY-MM-DD</option>
                <option>DD/MM/YYYY</option>
                <option>MM/DD/YYYY</option>
              </select>
            </div>
          </div>
        </div>

        <!-- Notification Preferences Card -->
        <div class="card-panel space-y-5">
          <div class="flex items-center gap-3 border-b border-[var(--border-color)] pb-3">
            <div class="w-9 h-9 rounded-xl bg-amber-100 dark:bg-amber-900/30 text-amber-600 flex items-center justify-center">
              <i class="pi pi-bell"></i>
            </div>
            <div>
              <h3 class="font-bold text-sm text-[var(--text-main)]">Notification Channels</h3>
              <p class="text-[11px] text-[var(--text-muted)]">Configure notification delivery methods.</p>
            </div>
          </div>

          <div class="space-y-3 text-xs">
            @for (channel of notificationChannels; track channel.label) {
              <div class="flex items-center justify-between py-1.5">
                <div>
                  <span class="font-bold text-[var(--text-main)] block">{{ channel.label }}</span>
                  <span class="text-[11px] text-[var(--text-muted)]">{{ channel.description }}</span>
                </div>
                <button (click)="channel.enabled = !channel.enabled" class="relative w-12 h-6 rounded-full transition-colors cursor-pointer" [class.bg-blue-600]="channel.enabled" [class.bg-slate-300]="!channel.enabled">
                  <span class="absolute top-0.5 w-5 h-5 bg-white rounded-full shadow-md transition-transform" [class.translate-x-6]="channel.enabled" [class.translate-x-0.5]="!channel.enabled"></span>
                </button>
              </div>
            }
          </div>
        </div>

        <!-- Security Settings Card -->
        <div class="card-panel space-y-5">
          <div class="flex items-center gap-3 border-b border-[var(--border-color)] pb-3">
            <div class="w-9 h-9 rounded-xl bg-rose-100 dark:bg-rose-900/30 text-rose-600 flex items-center justify-center">
              <i class="pi pi-shield"></i>
            </div>
            <div>
              <h3 class="font-bold text-sm text-[var(--text-main)]">Security & Authentication</h3>
              <p class="text-[11px] text-[var(--text-muted)]">Two-factor, session, and access controls.</p>
            </div>
          </div>

          <div class="space-y-4 text-xs">
            <div class="flex items-center justify-between">
              <div>
                <span class="font-bold text-[var(--text-main)] block">Two-Factor Authentication (2FA)</span>
                <span class="text-[11px] text-[var(--text-muted)]">Add an extra layer of sign-in security.</span>
              </div>
              <span class="status-badge active">Enabled</span>
            </div>

            <div>
              <span class="font-bold text-[var(--text-main)] block mb-1.5">Session Timeout</span>
              <select class="w-full px-3 py-2 bg-slate-100 dark:bg-slate-800 border border-slate-200 dark:border-slate-700 rounded-xl">
                <option>30 minutes</option>
                <option>1 hour</option>
                <option>4 hours</option>
                <option>8 hours</option>
              </select>
            </div>

            <button class="w-full py-2.5 bg-rose-50 dark:bg-rose-900/20 text-rose-600 font-bold text-xs rounded-xl border border-rose-200 dark:border-rose-800 hover:bg-rose-100 transition-colors">
              <i class="pi pi-sign-out text-[10px]"></i> Sign Out All Other Sessions
            </button>
          </div>
        </div>

      </div>

    </div>
  `
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
