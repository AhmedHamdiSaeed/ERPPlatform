/**
 * Lightweight device detection used to pick the right session lifetime.
 */

/**
 * True when the app runs on a phone or tablet (mobile browser or installed PWA).
 *
 * Combines three signals because no single one is reliable on its own:
 *  - user agent keywords (Android / iPhone / iPad / Windows Phone / …)
 *  - coarse pointer (touch screen)
 *  - iPadOS 13+ reports itself as "Macintosh", so touch points + small viewport catch it
 */
export function isMobileDevice(): boolean {
  if (typeof navigator === 'undefined' || typeof window === 'undefined') {
    return false;
  }

  const userAgent = navigator.userAgent || '';
  const mobileUserAgent = /Android|webOS|iPhone|iPad|iPod|BlackBerry|IEMobile|Opera Mini|Windows Phone|Mobile/i.test(userAgent);

  if (mobileUserAgent) {
    return true;
  }

  const hasTouch = (navigator.maxTouchPoints || 0) > 0 || 'ontouchstart' in window;
  const coarsePointer = !!window.matchMedia?.('(pointer: coarse)').matches;

  return hasTouch && coarsePointer && window.innerWidth < 1024;
}
