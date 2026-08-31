/**
 * Session / authentication storage keys and timing configuration.
 */

/** localStorage key holding the JWT access token. */
export const TOKEN_KEY = 'erp_access_token';

/** localStorage key holding the refresh token used to renew the access token. */
export const REFRESH_TOKEN_KEY = 'erp_refresh_token';

/** localStorage key holding the absolute session expiry (epoch milliseconds). */
export const SESSION_EXPIRES_AT_KEY = 'erp_session_expires_at';

/** sessionStorage key holding the URL the user wanted before being sent to login. */
export const RETURN_URL_KEY = 'erp_return_url';

/** Absolute session lifetime on desktop / web: 3 hours from login. */
export const SESSION_DURATION_MS = 3 * 60 * 60 * 1000;

/**
 * Absolute session lifetime on phones and tablets: 6 months (180 days).
 * Mobile users stay signed in like in a native app.
 */
export const MOBILE_SESSION_DURATION_MS = 180 * 24 * 60 * 60 * 1000;

/**
 * setTimeout() silently overflows past 2^31-1 ms (~24.8 days) and fires immediately,
 * which would log the user out straight away on a 6 month session.
 * Long waits are therefore re-armed in chunks no bigger than this.
 */
export const MAX_TIMEOUT_MS = 2_147_483_647 - 60_000;

/** How long before expiry the user gets a heads-up warning. */
export const SESSION_WARN_BEFORE_MS = 5 * 60 * 1000;

/** How often the countdown signal is refreshed (keeps change detection cheap). */
export const SESSION_TICK_MS = 30 * 1000;

/**
 * Dev-only helper: visit /auth/login?sessionSeconds=20 to shrink the session to
 * 20 seconds and test the expiry flow. Ignored in production builds.
 */
export const SESSION_DEBUG_DURATION_KEY = 'erp_session_debug_seconds';
