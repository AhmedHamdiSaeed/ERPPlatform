import { DestroyRef, Injectable, NgZone, computed, inject, signal } from '@angular/core';
import { Router, UrlTree } from '@angular/router';
import { environment } from '../../../environments/environment';
import {
  MAX_TIMEOUT_MS,
  MOBILE_SESSION_DURATION_MS,
  REFRESH_TOKEN_KEY,
  RETURN_URL_KEY,
  SESSION_DEBUG_DURATION_KEY,
  SESSION_DURATION_MS,
  SESSION_EXPIRES_AT_KEY,
  SESSION_TICK_MS,
  SESSION_WARN_BEFORE_MS,
  TOKEN_KEY,
} from '../constants/session.constants';
import { isMobileDevice } from '../utils/device.util';
import { ToastService } from './toast.service';

export type SessionEndReason = 'timeout' | 'unauthorized';

/** Events that count as "the user made an action on the page". */
const ACTIVITY_EVENTS = ['click', 'mousedown', 'keydown', 'touchstart', 'submit'] as const;

/**
 * Absolute session lifetime, counted from the moment the user logs in:
 *  - desktop / web : 3 hours
 *  - phone / tablet: 6 months
 *
 * Once the session is over:
 *  - navigating to another route is blocked by `authGuard`
 *  - any action on the current page (click / key / API call) is intercepted
 *  - the user is sent to /auth/login?returnUrl=<page they wanted>
 * After logging in again they land back on that exact page (including query params).
 */
@Injectable({
  providedIn: 'root'
})
export class SessionTimeoutService {
  private router = inject(Router);
  private ngZone = inject(NgZone);
  private toast = inject(ToastService);
  private destroyRef = inject(DestroyRef);

  private readonly expiresAt = signal<number | null>(null);
  private readonly now = signal<number>(Date.now());

  private expiryTimer?: ReturnType<typeof setTimeout>;
  private tickTimer?: ReturnType<typeof setInterval>;
  private warned = false;
  private initialized = false;

  /** True while a session exists and has not run out yet. */
  readonly active = computed(() => this.expiresAt() !== null && this.remainingMs() > 0);

  /** True when a session was started but its lifetime is over. */
  readonly expired = computed(() => this.expiresAt() !== null && this.remainingMs() <= 0);

  /** Milliseconds left before the session dies (0 when no session / expired). */
  readonly remainingMs = computed(() => {
    const expiresAt = this.expiresAt();
    return expiresAt === null ? 0 : Math.max(0, expiresAt - this.now());
  });

  readonly remainingMinutes = computed(() => Math.ceil(this.remainingMs() / 60000));

  // ──────────────────────────────────────────────────────────────
  // Bootstrap
  // ──────────────────────────────────────────────────────────────

  /** Called once from `provideAppInitializer` in app.config.ts. */
  init(): void {
    if (this.initialized) {
      return;
    }
    this.initialized = true;

    this.captureDebugDuration();
    this.restore();
    this.attachActivityListeners();
    this.attachStorageListener();

    this.destroyRef.onDestroy(() => this.detachAll());
  }

  /** Rebuilds the session state from localStorage after a page reload. */
  private restore(): void {
    const token = localStorage.getItem(TOKEN_KEY);

    if (!token) {
      this.expiresAt.set(null);
      localStorage.removeItem(SESSION_EXPIRES_AT_KEY);
      localStorage.removeItem(REFRESH_TOKEN_KEY);
      return;
    }

    const rawExpiry = Number(localStorage.getItem(SESSION_EXPIRES_AT_KEY));

    // Token exists but no expiry recorded (e.g. older session): start counting from now.
    if (!rawExpiry || Number.isNaN(rawExpiry)) {
      this.start();
      return;
    }

    // Already expired while the tab was closed -> timers fire immediately and
    // the guard / activity listeners bounce the user to the login page.
    this.applyExpiry(rawExpiry);
  }

  // ──────────────────────────────────────────────────────────────
  // Session lifecycle
  // ──────────────────────────────────────────────────────────────

  /**
   * Starts a brand new session (called right after a successful login).
   * Duration defaults to 3 hours on desktop / 6 months on phones & tablets.
   */
  start(durationMs?: number): void {
    const expiresAt = Date.now() + (durationMs ?? this.resolveDuration());
    // Write first so other tabs can pick the new session up via the storage event.
    localStorage.setItem(SESSION_EXPIRES_AT_KEY, String(expiresAt));
    this.applyExpiry(expiresAt);
  }

  /** Ends the session without redirecting (manual logout – caller navigates). */
  stop(): void {
    this.end(undefined, false);
  }

  /** Ends the session because it ran out or the backend rejected it, then redirects. */
  expire(reason: SessionEndReason): void {
    this.end(reason, true);
  }

  private applyExpiry(expiresAt: number): void {
    this.clearTimers();
    this.warned = false;
    this.expiresAt.set(expiresAt);
    this.now.set(Date.now());
    this.scheduleTimers();
  }

  private end(reason: SessionEndReason | undefined, redirect: boolean): void {
    const hadSession = this.expiresAt() !== null || !!localStorage.getItem(TOKEN_KEY);

    this.clearTimers();
    this.expiresAt.set(null);
    this.now.set(Date.now());
    localStorage.removeItem(TOKEN_KEY);
    localStorage.removeItem(REFRESH_TOKEN_KEY);
    localStorage.removeItem(SESSION_EXPIRES_AT_KEY);

    if (!redirect || !hadSession) {
      return;
    }

    const currentUrl = this.currentUrl();
    if (this.isAuthRoute(currentUrl)) {
      return;
    }
    this.navigateToLogin(currentUrl, reason);
  }

  private scheduleTimers(): void {
    const expiresAt = this.expiresAt();
    if (expiresAt === null) {
      return;
    }

    // Refreshes the countdown signal and emits the "about to expire" warning.
    this.tickTimer = setInterval(
      () =>
        this.ngZone.run(() => {
          this.now.set(Date.now());
          if (!this.warned && this.active() && this.remainingMs() <= SESSION_WARN_BEFORE_MS) {
            this.warnOnce();
          }
        }),
      SESSION_TICK_MS
    );

    // A 6 month session is far longer than setTimeout can handle, so the wait is
    // capped and re-armed until the real expiry is reached.
    const wait = Math.max(0, Math.min(expiresAt - Date.now(), MAX_TIMEOUT_MS));
    this.expiryTimer = setTimeout(
      () =>
        this.ngZone.run(() => {
          if (Date.now() >= expiresAt) {
            this.expire('timeout');
          } else {
            this.scheduleTimers();
          }
        }),
      wait
    );
  }

  private warnOnce(): void {
    if (this.warned || !this.active()) {
      return;
    }
    this.warned = true;
    this.toast.warning(
      'Your session will expire in 5 minutes. Save your work, you will be asked to sign in again.',
      'Session expiring soon',
      20000
    );
  }

  // ──────────────────────────────────────────────────────────────
  // Guards / interceptors API
  // ──────────────────────────────────────────────────────────────

  /** Synchronous check: is there a live session right now? */
  isSessionValid(): boolean {
    if (!localStorage.getItem(TOKEN_KEY)) {
      return false;
    }
    const expiresAt = this.expiresAt();
    return expiresAt === null || Date.now() < expiresAt;
  }

  /** Synchronous check: is the session lifetime over? */
  isExpired(): boolean {
    const expiresAt = this.expiresAt();
    return expiresAt !== null && Date.now() >= expiresAt;
  }

  /**
   * Call this whenever the user does something in the app.
   * If the session is gone the user is redirected to the login page right away.
   */
  registerActivity(): boolean {
    this.now.set(Date.now());

    if (!localStorage.getItem(TOKEN_KEY)) {
      this.end('unauthorized', true);
      return false;
    }
    if (this.isExpired()) {
      this.end('timeout', true);
      return false;
    }
    return true;
  }

  /**
   * Used by `authGuard`: remembers where the user wanted to go and returns
   * the UrlTree that sends them to the login page.
   */
  redirectToLogin(returnUrl?: string, reason?: SessionEndReason): UrlTree {
    const safeUrl = returnUrl && this.isSafeReturnUrl(returnUrl) ? returnUrl : '';
    if (safeUrl) {
      this.storeReturnUrl(safeUrl);
    }
    return this.router.createUrlTree(['/auth/login'], {
      queryParams: {
        ...(safeUrl ? { returnUrl: safeUrl } : {}),
        ...(reason ? { reason } : {})
      }
    });
  }

  /** Reads (and clears) the page the user originally wanted. */
  consumeReturnUrl(queryReturnUrl?: string | null): string {
    const stored = sessionStorage.getItem(RETURN_URL_KEY);
    sessionStorage.removeItem(RETURN_URL_KEY);

    const candidate = queryReturnUrl || stored || '';
    return this.isSafeReturnUrl(candidate) ? candidate : '';
  }

  // ──────────────────────────────────────────────────────────────
  // Activity / cross-tab listeners
  // ──────────────────────────────────────────────────────────────

  private readonly onUserActivity = (): void => {
    this.ngZone.run(() => this.registerActivity());
  };

  private readonly onVisibilityChange = (): void => {
    if (document.visibilityState !== 'visible') {
      return;
    }
    this.ngZone.run(() => this.registerActivity());
  };

  private readonly onStorage = (event: StorageEvent): void => {
    if (event.key !== SESSION_EXPIRES_AT_KEY && event.key !== TOKEN_KEY) {
      return;
    }

    this.ngZone.run(() => {
      if (event.key === SESSION_EXPIRES_AT_KEY && event.newValue) {
        // Another tab logged in -> adopt the new session in this tab too.
        const expiresAt = Number(event.newValue);
        if (!Number.isNaN(expiresAt)) {
          this.applyExpiry(expiresAt);
        }
        return;
      }

      // Session/token removed in another tab -> mark this tab's session as over
      // (expiresAt stays non-null so the next action still triggers a redirect).
      if (event.newValue === null && this.expiresAt() !== null) {
        this.clearTimers();
        this.expiresAt.set(0);
        this.now.set(Date.now());
      }
    });
  };

  private attachActivityListeners(): void {
    for (const eventName of ACTIVITY_EVENTS) {
      document.addEventListener(eventName, this.onUserActivity, { capture: true, passive: true });
    }
    document.addEventListener('visibilitychange', this.onVisibilityChange);
  }

  private attachStorageListener(): void {
    window.addEventListener('storage', this.onStorage);
  }

  private detachAll(): void {
    for (const eventName of ACTIVITY_EVENTS) {
      document.removeEventListener(eventName, this.onUserActivity, { capture: true });
    }
    document.removeEventListener('visibilitychange', this.onVisibilityChange);
    window.removeEventListener('storage', this.onStorage);
    this.clearTimers();
  }

  // ──────────────────────────────────────────────────────────────
  // Helpers
  // ──────────────────────────────────────────────────────────────

  /** Dev only: `?sessionSeconds=30` on the login page shortens the session for testing. */
  private captureDebugDuration(): void {
    if (environment.production) {
      return;
    }
    const raw = new URLSearchParams(window.location.search).get('sessionSeconds');
    if (raw === null) {
      return;
    }
    const seconds = Number(raw);
    if (seconds > 0) {
      localStorage.setItem(SESSION_DEBUG_DURATION_KEY, String(seconds));
    } else {
      localStorage.removeItem(SESSION_DEBUG_DURATION_KEY);
    }
  }

  /**
   * Picks the session lifetime: dev override wins, then the device type.
   * Phones and tablets keep the user signed in for 6 months, desktops for 3 hours.
   */
  private resolveDuration(): number {
    if (!environment.production) {
      const seconds = Number(localStorage.getItem(SESSION_DEBUG_DURATION_KEY));
      if (seconds > 0 && !Number.isNaN(seconds)) {
        return seconds * 1000;
      }
    }
    return isMobileDevice() ? MOBILE_SESSION_DURATION_MS : SESSION_DURATION_MS;
  }

  private navigateToLogin(returnUrl: string, reason?: SessionEndReason): void {
    this.storeReturnUrl(returnUrl);

    this.ngZone.run(() => {
      this.router.navigate(['/auth/login'], {
        queryParams: {
          returnUrl,
          ...(reason ? { reason } : {})
        },
        replaceUrl: true
      });
    });
  }

  private storeReturnUrl(url: string): void {
    if (!this.isSafeReturnUrl(url)) {
      return;
    }
    sessionStorage.setItem(RETURN_URL_KEY, url);
  }

  /**
   * Best effort current URL: the router URL once navigation happened,
   * otherwise the browser URL (still being resolved at startup).
   */
  private currentUrl(): string {
    const routerUrl = this.router.url;
    if (routerUrl && routerUrl !== '/') {
      return routerUrl;
    }
    return (window.location.pathname || '/') + (window.location.search || '');
  }

  private isAuthRoute(url: string): boolean {
    return url.startsWith('/auth/');
  }

  /** Blocks open redirects: only internal, non-auth routes are accepted. */
  private isSafeReturnUrl(url: string): boolean {
    if (!url || typeof url !== 'string') {
      return false;
    }
    return (
      url.startsWith('/') &&
      !url.startsWith('//') &&
      !url.startsWith('/\\') &&
      !this.isAuthRoute(url)
    );
  }

  private clearTimers(): void {
    if (this.expiryTimer) {
      clearTimeout(this.expiryTimer);
      this.expiryTimer = undefined;
    }
    if (this.tickTimer) {
      clearInterval(this.tickTimer);
      this.tickTimer = undefined;
    }
  }
}
