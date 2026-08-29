import { TestBed } from '@angular/core/testing';
import { StateService } from './state.service';
import { CURRENT_USER, MOCK_NOTIFICATIONS } from '../mock/mock-data';

describe('StateService', () => {
  let service: StateService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(StateService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  // ─── INITIAL STATE ────────────────────────────────────────────────────────

  it('should initialize currentUser from CURRENT_USER mock', () => {
    expect(service.currentUser()).toEqual(CURRENT_USER);
  });

  it('should initialize theme as light', () => {
    expect(service.theme()).toBe('light');
  });

  it('should initialize lang as en', () => {
    expect(service.lang()).toBe('en');
  });

  it('should initialize sidebarExpanded as true', () => {
    expect(service.sidebarExpanded()).toBeTrue();
  });

  it('should initialize mobileDrawerOpen as false', () => {
    expect(service.mobileDrawerOpen()).toBeFalse();
  });

  it('should initialize globalSearchOpen as false', () => {
    expect(service.globalSearchOpen()).toBeFalse();
  });

  it('should initialize aiWidgetOpen as false', () => {
    expect(service.aiWidgetOpen()).toBeFalse();
  });

  // ─── COMPUTED SIGNALS ─────────────────────────────────────────────────────

  it('isDark computed should be false when theme is light', () => {
    service.theme.set('light');
    expect(service.isDark()).toBeFalse();
  });

  it('isDark computed should be true when theme is dark', () => {
    service.theme.set('dark');
    expect(service.isDark()).toBeTrue();
  });

  it('isRtl computed should be false when lang is en', () => {
    service.lang.set('en');
    expect(service.isRtl()).toBeFalse();
  });

  it('isRtl computed should be true when lang is ar', () => {
    service.lang.set('ar');
    expect(service.isRtl()).toBeTrue();
  });

  it('sidebarCollapsed computed should invert sidebarExpanded', () => {
    service.sidebarExpanded.set(true);
    expect(service.sidebarCollapsed()).toBeFalse();

    service.sidebarExpanded.set(false);
    expect(service.sidebarCollapsed()).toBeTrue();
  });

  it('unreadNotificationCount should count unread notifications', () => {
    const unread = MOCK_NOTIFICATIONS.filter(n => !n.read).length;
    expect(service.unreadNotificationCount()).toBe(unread);
  });

  // ─── THEME TOGGLE ─────────────────────────────────────────────────────────

  it('toggleTheme() should switch from light to dark', () => {
    service.theme.set('light');
    service.toggleTheme();
    expect(service.theme()).toBe('dark');
  });

  it('toggleTheme() should switch from dark back to light', () => {
    service.theme.set('dark');
    service.toggleTheme();
    expect(service.theme()).toBe('light');
  });

  it('toggleTheme() twice should return to original theme', () => {
    const initial = service.theme();
    service.toggleTheme();
    service.toggleTheme();
    expect(service.theme()).toBe(initial);
  });

  // ─── SIDEBAR ──────────────────────────────────────────────────────────────

  it('toggleSidebar() should collapse an expanded sidebar', () => {
    service.sidebarExpanded.set(true);
    service.toggleSidebar();
    expect(service.sidebarExpanded()).toBeFalse();
  });

  it('toggleSidebar() should expand a collapsed sidebar', () => {
    service.sidebarExpanded.set(false);
    service.toggleSidebar();
    expect(service.sidebarExpanded()).toBeTrue();
  });

  // ─── MOBILE DRAWER ────────────────────────────────────────────────────────

  it('toggleMobileDrawer() should open a closed drawer', () => {
    service.mobileDrawerOpen.set(false);
    service.toggleMobileDrawer();
    expect(service.mobileDrawerOpen()).toBeTrue();
  });

  it('toggleMobileDrawer() should close an open drawer', () => {
    service.mobileDrawerOpen.set(true);
    service.toggleMobileDrawer();
    expect(service.mobileDrawerOpen()).toBeFalse();
  });

  // ─── GLOBAL SEARCH ────────────────────────────────────────────────────────

  it('toggleGlobalSearch() with no arg should toggle state', () => {
    service.globalSearchOpen.set(false);
    service.toggleGlobalSearch();
    expect(service.globalSearchOpen()).toBeTrue();

    service.toggleGlobalSearch();
    expect(service.globalSearchOpen()).toBeFalse();
  });

  it('toggleGlobalSearch(true) should force open state', () => {
    service.globalSearchOpen.set(false);
    service.toggleGlobalSearch(true);
    expect(service.globalSearchOpen()).toBeTrue();
  });

  it('toggleGlobalSearch(false) should force closed state', () => {
    service.globalSearchOpen.set(true);
    service.toggleGlobalSearch(false);
    expect(service.globalSearchOpen()).toBeFalse();
  });

  // ─── AI WIDGET ────────────────────────────────────────────────────────────

  it('toggleAiWidget() with no arg should toggle state', () => {
    service.aiWidgetOpen.set(false);
    service.toggleAiWidget();
    expect(service.aiWidgetOpen()).toBeTrue();
  });

  it('toggleAiWidget(true) should force open', () => {
    service.aiWidgetOpen.set(false);
    service.toggleAiWidget(true);
    expect(service.aiWidgetOpen()).toBeTrue();
  });

  // ─── NOTIFICATIONS ────────────────────────────────────────────────────────

  it('markNotificationAsRead() should mark only that notification as read', () => {
    const notifications = service.notifications();
    const unreadNotification = notifications.find(n => !n.read);

    if (!unreadNotification) return; // Skip if all read

    service.markNotificationAsRead(unreadNotification.id);
    const updated = service.notifications().find(n => n.id === unreadNotification.id);
    expect(updated?.read).toBeTrue();
  });

  it('markNotificationAsRead() should not affect other notifications', () => {
    const notifications = service.notifications();
    if (notifications.length < 2) return;

    const firstId = notifications[0].id;
    const secondId = notifications[1].id;
    const secondWasRead = notifications[1].read;

    service.markNotificationAsRead(firstId);
    const second = service.notifications().find(n => n.id === secondId);
    expect(second?.read).toBe(secondWasRead); // Unchanged
  });

  it('markAllNotificationsAsRead() should set all notifications to read', () => {
    service.markAllNotificationsAsRead();
    const allRead = service.notifications().every(n => n.read);
    expect(allRead).toBeTrue();
    expect(service.unreadNotificationCount()).toBe(0);
  });

  it('clearNotifications() should empty the notifications list', () => {
    service.clearNotifications();
    expect(service.notifications().length).toBe(0);
    expect(service.unreadNotificationCount()).toBe(0);
  });

  // ─── LANGUAGE ─────────────────────────────────────────────────────────────

  it('setLanguage(ar) should set lang to ar and isRtl to true', () => {
    service.setLanguage('ar');
    expect(service.lang()).toBe('ar');
    expect(service.isRtl()).toBeTrue();
  });

  it('setLanguage(en) should set lang to en and isRtl to false', () => {
    service.setLanguage('en');
    expect(service.lang()).toBe('en');
    expect(service.isRtl()).toBeFalse();
  });

  // ─── PERMISSIONS ──────────────────────────────────────────────────────────

  it('hasPermission should return true for wildcard permissions', () => {
    service.currentUser.set({
      id: 'usr-1',
      name: 'Admin',
      email: 'admin@erpplatform.com',
      role: 'Admin',
      avatar: '',
      permissions: ['*']
    });
    expect(service.hasPermission('ERPPlatform.Dashboard.View')).toBeTrue();
    expect(service.hasPermission('ERPPlatform.Customers')).toBeTrue();
  });

  it('hasPermission should check specific granted permissions correctly', () => {
    service.currentUser.set({
      id: 'usr-2',
      name: 'Sales Viewer',
      email: 'sales.viewer@erpplatform.com',
      role: 'Sales Viewer',
      avatar: '',
      permissions: ['ERPPlatform.Dashboard.View', 'ERPPlatform.Customers', 'ERPPlatform.Invoices']
    });
    expect(service.hasPermission('ERPPlatform.Dashboard.View')).toBeTrue();
    expect(service.hasPermission('ERPPlatform.Customers')).toBeTrue();
    expect(service.hasPermission('ERPPlatform.Invoices')).toBeTrue();
    expect(service.hasPermission('ERPPlatform.Users')).toBeFalse();
    expect(service.hasPermission('ERPPlatform.Roles')).toBeFalse();
  });
});
