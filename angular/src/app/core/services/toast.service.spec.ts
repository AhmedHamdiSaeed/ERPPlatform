import { TestBed, fakeAsync, tick, discardPeriodicTasks } from '@angular/core/testing';
import { ToastService, ToastItem } from './toast.service';

describe('ToastService', () => {
  let service: ToastService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(ToastService);
  });

  afterEach(() => {
    // Ensure no lingering auto-dismiss timers bleed between tests
    service.clearAll();
  });

  // ─── Creation ───────────────────────────────────────────────────────────────

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should initialise the toasts signal as an empty array', () => {
    expect(service.toasts()).toEqual([]);
  });

  // ─── success() ──────────────────────────────────────────────────────────────

  it('success() should add a toast with type "success" and default title "Success"', fakeAsync(() => {
    service.success('Employee created successfully', undefined, 0);

    const toasts = service.toasts();
    expect(toasts.length).toBe(1);
    expect(toasts[0].type).toBe('success');
    expect(toasts[0].title).toBe('Success');
    expect(toasts[0].message).toBe('Employee created successfully');
  }));

  it('success() should use a custom title when provided', fakeAsync(() => {
    service.success('Record saved.', 'Done!', 0);

    expect(service.toasts()[0].title).toBe('Done!');
  }));

  // ─── error() ────────────────────────────────────────────────────────────────

  it('error() should add a toast with type "error" and default title "Error"', fakeAsync(() => {
    service.error('Connection failed', undefined, 0);

    const toasts = service.toasts();
    expect(toasts.length).toBe(1);
    expect(toasts[0].type).toBe('error');
    expect(toasts[0].title).toBe('Error');
    expect(toasts[0].message).toBe('Connection failed');
  }));

  it('error() should use a custom title when provided', fakeAsync(() => {
    service.error('Network timeout.', 'Server Error', 0);

    expect(service.toasts()[0].title).toBe('Server Error');
  }));

  // ─── warning() ──────────────────────────────────────────────────────────────

  it('warning() should add a toast with type "warning" and default title "Warning"', fakeAsync(() => {
    service.warning('Low stock detected', undefined, 0);

    const toasts = service.toasts();
    expect(toasts.length).toBe(1);
    expect(toasts[0].type).toBe('warning');
    expect(toasts[0].title).toBe('Warning');
    expect(toasts[0].message).toBe('Low stock detected');
  }));

  it('warning() should use a custom title when provided', fakeAsync(() => {
    service.warning('Check inventory.', 'Stock Alert', 0);

    expect(service.toasts()[0].title).toBe('Stock Alert');
  }));

  // ─── info() ─────────────────────────────────────────────────────────────────

  it('info() should add a toast with type "info" and default title "Notification"', fakeAsync(() => {
    service.info('System update available', undefined, 0);

    const toasts = service.toasts();
    expect(toasts.length).toBe(1);
    expect(toasts[0].type).toBe('info');
    expect(toasts[0].title).toBe('Notification');
    expect(toasts[0].message).toBe('System update available');
  }));

  it('info() should use a custom title when provided', fakeAsync(() => {
    service.info('Maintenance tonight.', 'System Info', 0);

    expect(service.toasts()[0].title).toBe('System Info');
  }));

  // ─── show() – ordering ──────────────────────────────────────────────────────

  it('show() should prepend new toasts so the newest appears first in the list', fakeAsync(() => {
    service.show('First message', 'info', undefined, 0);
    service.show('Second message', 'success', undefined, 0);
    service.show('Third message', 'error', undefined, 0);

    const toasts = service.toasts();
    expect(toasts.length).toBe(3);
    expect(toasts[0].message).toBe('Third message');
    expect(toasts[1].message).toBe('Second message');
    expect(toasts[2].message).toBe('First message');
  }));

  it('show() with a custom title should use that title instead of the type default', fakeAsync(() => {
    service.show('Something happened', 'warning', 'Custom Title', 0);

    expect(service.toasts()[0].title).toBe('Custom Title');
  }));

  // ─── Unique IDs ─────────────────────────────────────────────────────────────

  it('each toast should have a unique id', fakeAsync(() => {
    service.success('T1', undefined, 0);
    service.error('T2', undefined, 0);
    service.warning('T3', undefined, 0);
    service.info('T4', undefined, 0);

    const ids = service.toasts().map(t => t.id);
    const uniqueIds = new Set(ids);
    expect(uniqueIds.size).toBe(ids.length);
  }));

  it('each toast id should start with the "toast-" prefix', fakeAsync(() => {
    service.show('Any message', 'info', undefined, 0);

    expect(service.toasts()[0].id).toMatch(/^toast-/);
  }));

  // ─── dismiss() ──────────────────────────────────────────────────────────────

  it('dismiss(id) should remove only the toast with that id', fakeAsync(() => {
    service.success('Message 1', undefined, 0);
    service.info('Message 2', undefined, 0);

    const allToasts = service.toasts();
    const idToDismiss = allToasts[0].id;
    const survivingId = allToasts[1].id;

    service.dismiss(idToDismiss);

    const remaining = service.toasts();
    expect(remaining.length).toBe(1);
    expect(remaining[0].id).toBe(survivingId);
    expect(remaining.find(t => t.id === idToDismiss)).toBeUndefined();
  }));

  it('dismiss() with an unknown id should not change the toasts list', fakeAsync(() => {
    service.success('T1', undefined, 0);
    service.dismiss('non-existent-id');

    expect(service.toasts().length).toBe(1);
  }));

  it('dismiss() on an empty list should not throw', fakeAsync(() => {
    expect(() => service.dismiss('ghost-id')).not.toThrow();
  }));

  // ─── clearAll() ─────────────────────────────────────────────────────────────

  it('clearAll() should remove all toasts', fakeAsync(() => {
    service.success('T1', undefined, 0);
    service.error('T2', undefined, 0);
    service.warning('T3', undefined, 0);

    expect(service.toasts().length).toBe(3);

    service.clearAll();

    expect(service.toasts().length).toBe(0);
    expect(service.toasts()).toEqual([]);
  }));

  it('clearAll() on an already empty list should not throw', () => {
    expect(() => service.clearAll()).not.toThrow();
  });

  // ─── Auto-dismiss ────────────────────────────────────────────────────────────

  it('show() with duration > 0 should auto-dismiss the toast after the given duration', fakeAsync(() => {
    service.show('Temporary toast', 'info', undefined, 2000);

    expect(service.toasts().length).toBe(1);

    tick(2000);

    expect(service.toasts().length).toBe(0);
  }));

  it('show() with duration 0 should NOT auto-dismiss the toast', fakeAsync(() => {
    service.show('Persistent toast', 'info', undefined, 0);

    tick(60_000); // advance far into the future

    expect(service.toasts().length).toBe(1);
  }));

  it('show() with default duration should auto-dismiss after 4000 ms', fakeAsync(() => {
    service.show('Default duration toast', 'success');

    expect(service.toasts().length).toBe(1);

    tick(4000);

    expect(service.toasts().length).toBe(0);
  }));

  // ─── Toast shape ─────────────────────────────────────────────────────────────

  it('each toast should have a non-empty timestamp string', fakeAsync(() => {
    service.show('Check timestamp', 'info', undefined, 0);

    const toast = service.toasts()[0];
    expect(typeof toast.timestamp).toBe('string');
    expect(toast.timestamp.length).toBeGreaterThan(0);
  }));

  it('each toast should carry the correct message and type', fakeAsync(() => {
    service.show('Payload check', 'error', 'Oops', 0);

    const toast: ToastItem = service.toasts()[0];
    expect(toast.message).toBe('Payload check');
    expect(toast.type).toBe('error');
    expect(toast.title).toBe('Oops');
    expect(toast.duration).toBe(0);
  }));
});
