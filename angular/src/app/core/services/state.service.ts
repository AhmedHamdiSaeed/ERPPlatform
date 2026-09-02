import { Injectable, signal, computed, inject } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { UserProfile, NotificationItem } from '../models/erp-models';
import { NotificationApiService } from './api/notification-api.service';
import { environment } from '../../../environments/environment';

export type ThemeMode = 'light' | 'dark';
export type LanguageMode = 'en' | 'ar';

const LANG_STORAGE_KEY = 'erp_lang';

export const GUEST_USER: UserProfile = {
  id: '',
  name: '',
  email: '',
  role: 'Employee',
  avatar: '',
  permissions: []
};

@Injectable({
  providedIn: 'root'
})
export class StateService {
  private http = inject(HttpClient);
  private notificationApi = inject(NotificationApiService);

  // Signals State
  currentUser = signal<UserProfile>(GUEST_USER);
  theme = signal<ThemeMode>('light');
  lang = signal<LanguageMode>('en');
  sidebarExpanded = signal<boolean>(true);
  mobileDrawerOpen = signal<boolean>(false);
  globalSearchOpen = signal<boolean>(false);
  aiWidgetOpen = signal<boolean>(false);
  notifications = signal<NotificationItem[]>([]);
  notificationsLoading = signal<boolean>(false);

  unreadNotificationCount = computed(() => 
    this.notifications().filter(n => !n.read).length
  );

  isRtl = computed(() => this.lang() === 'ar');
  sidebarCollapsed = computed(() => !this.sidebarExpanded());
  isDark = computed(() => this.theme() === 'dark');
  language = computed(() => this.lang());

  constructor() {
    this.initLanguage();
    this.initKeyboardShortcuts();
    this.loadAppConfig();
  }

  /** Restores the saved language on startup so the choice survives a reload. */
  private initLanguage() {
    let saved: string | null = null;
    try {
      saved = typeof localStorage !== 'undefined' ? localStorage.getItem(LANG_STORAGE_KEY) : null;
    } catch {
      saved = null;
    }
    const initial: LanguageMode = saved === 'ar' || saved === 'en' ? saved : 'en';
    this.setLanguage(initial);
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

        await this.loadNotifications();
      } else {
        this.setGuestUser();
      }
    } catch (err) {
      console.error('Failed to load application configuration', err);
      this.setGuestUser();
    }
  }

  private setGuestUser() {
    this.currentUser.set(GUEST_USER);
    this.notifications.set([]);
  }

  /** Pulls the current user's notifications from the server. */
  async loadNotifications(): Promise<void> {
    this.notificationsLoading.set(true);
    try {
      this.notifications.set(await this.notificationApi.getNotifications());
    } catch (err) {
      console.error('Failed to load notifications', err);
      this.notifications.set([]);
    } finally {
      this.notificationsLoading.set(false);
    }
  }

  /** Adds a notification pushed from a SignalR hub to the top of the list. */
  pushNotification(notification: NotificationItem): void {
    this.notifications.update(list => [notification, ...list]);
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
    try {
      localStorage.setItem(LANG_STORAGE_KEY, language);
    } catch {
      /* storage unavailable (e.g. private mode) — non-fatal */
    }
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

  async markNotificationAsRead(id: string) {
    this.notifications.update(list =>
      list.map(n => n.id === id ? { ...n, read: true } : n)
    );
    try {
      await this.notificationApi.markAsRead(id);
    } catch (err) {
      console.error('Failed to mark notification as read', err);
    }
  }

  async markAllNotificationsAsRead() {
    this.notifications.update(list =>
      list.map(n => ({ ...n, read: true }))
    );
    try {
      await this.notificationApi.markAllAsRead();
    } catch (err) {
      console.error('Failed to mark all notifications as read', err);
    }
  }

  clearNotifications() {
    this.notifications.set([]);
  }

  /** Removes a notification from the local list (best-effort; the backend has no delete endpoint). */
  dismissNotification(id: string) {
    this.notifications.update(list => list.filter(n => n.id !== id));
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
