import { Injectable, signal, computed } from '@angular/core';
import { UserProfile, NotificationItem } from '../models/erp-models';
import { CURRENT_USER, MOCK_NOTIFICATIONS } from '../mock/mock-data';

export type ThemeMode = 'light' | 'dark';
export type LanguageMode = 'en' | 'ar';

@Injectable({
  providedIn: 'root'
})
export class StateService {
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
