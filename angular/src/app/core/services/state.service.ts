import { Injectable, signal, computed, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { UserProfile, NotificationItem } from '../models/erp-models';
import { CURRENT_USER, MOCK_NOTIFICATIONS } from '../mock/mock-data';
import { environment } from '../../../environments/environment';

export type ThemeMode = 'light' | 'dark';
export type LanguageMode = 'en' | 'ar';

@Injectable({
  providedIn: 'root'
})
export class StateService {
  private http = inject(HttpClient);
  // Signals State
  currentUser = signal<UserProfile>(CURRENT_USER);
  theme = signal<ThemeMode>('light');
  lang = signal<LanguageMode>('en');
  sidebarExpanded = signal<boolean>(true);
  mobileDrawerOpen = signal<boolean>(false);
  globalSearchOpen = signal<boolean>(false);
  aiWidgetOpen = signal<boolean>(false);
  notifications = signal<NotificationItem[]>(MOCK_NOTIFICATIONS);

  unreadNotificationCount = computed(() => 
    this.notifications().filter(n => !n.read).length
  );

  isRtl = computed(() => this.lang() === 'ar');
  sidebarCollapsed = computed(() => !this.sidebarExpanded());
  isDark = computed(() => this.theme() === 'dark');
  language = computed(() => this.lang());

  constructor() {
    this.initKeyboardShortcuts();
    this.loadAppConfig();
  }

  async loadAppConfig() {
    try {
      const config = await firstValueFrom(
        this.http.get<any>(`${environment.apis.default.url}/api/abp/application-configuration`)
      );
      if (config && config.currentUser && config.currentUser.isAuthenticated) {
        const userRoles = config.currentUser.roles || [];
        const policies = config.auth?.policies || {};
        const grantedPermissions = Object.keys(policies).filter(key => policies[key] === true);

        this.currentUser.set({
          id: config.currentUser.id,
          name: config.currentUser.userName || config.currentUser.name || 'User',
          email: config.currentUser.email || '',
          role: userRoles.includes('Admin') ? 'Admin' : (userRoles[0] || 'Employee'),
          avatar: 'https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=150',
          permissions: grantedPermissions
        });
      } else {
        this.setGuestUser();
      }
    } catch (err) {
      console.error('Failed to load application configuration', err);
      this.setGuestUser();
    }
  }

  private setGuestUser() {
    this.currentUser.set({
      id: '',
      name: '',
      email: '',
      role: 'Employee',
      avatar: '',
      permissions: []
    });
  }

  hasPermission(permission: string): boolean {
    const user = this.currentUser();
    if (!user) return false;
    if (user.permissions.includes('*')) return true;
    return user.permissions.includes(permission);
  }

  toggleTheme() {
    const nextTheme = this.theme() === 'light' ? 'dark' : 'light';
    this.theme.set(nextTheme);
    document.documentElement.setAttribute('data-theme', nextTheme);
  }

  setLanguage(language: LanguageMode) {
    this.lang.set(language);
    document.documentElement.setAttribute('dir', language === 'ar' ? 'rtl' : 'ltr');
    document.documentElement.setAttribute('lang', language);
  }

  toggleSidebar() {
    this.sidebarExpanded.update(v => !v);
  }

  toggleMobileDrawer() {
    this.mobileDrawerOpen.update(v => !v);
  }

  toggleGlobalSearch(open?: boolean) {
    this.globalSearchOpen.update(v => open !== undefined ? open : !v);
  }

  toggleAiWidget(open?: boolean) {
    this.aiWidgetOpen.update(v => open !== undefined ? open : !v);
  }

  markNotificationAsRead(id: string) {
    this.notifications.update(list => 
      list.map(n => n.id === id ? { ...n, read: true } : n)
    );
  }

  markAllNotificationsAsRead() {
    this.notifications.update(list => 
      list.map(n => ({ ...n, read: true }))
    );
  }

  clearNotifications() {
    this.notifications.set([]);
  }

  private initKeyboardShortcuts() {
    window.addEventListener('keydown', (e: KeyboardEvent) => {
      // Ctrl + K or Cmd + K for Global Search
      if ((e.ctrlKey || e.metaKey) && e.key.toLowerCase() === 'k') {
        e.preventDefault();
        this.toggleGlobalSearch();
      }
    });
  }
}
