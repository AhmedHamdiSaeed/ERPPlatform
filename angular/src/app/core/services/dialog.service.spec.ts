import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { DialogService, ConfirmDialogOptions } from './dialog.service';

describe('DialogService', () => {
  let service: DialogService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(DialogService);
  });

  // ─── Creation ───────────────────────────────────────────────────────────────

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  it('should initialise activeDialog signal as null', () => {
    expect(service.activeDialog()).toBeNull();
  });

  it('should initialise loading signal as false', () => {
    expect(service.loading()).toBeFalse();
  });

  // ─── confirm() ──────────────────────────────────────────────────────────────

  it('confirm() should set activeDialog signal with the provided options', fakeAsync(() => {
    const options: ConfirmDialogOptions = {
      title: 'Delete Record',
      message: 'Are you sure you want to delete this record?',
      confirmText: 'Delete',
      cancelText: 'Abort',
      type: 'danger'
    };

    service.confirm(options);

    const state = service.activeDialog();
    expect(state).not.toBeNull();
    expect(state!.options.title).toBe('Delete Record');
    expect(state!.options.message).toBe('Are you sure you want to delete this record?');
    expect(state!.options.confirmText).toBe('Delete');
    expect(state!.options.cancelText).toBe('Abort');
    expect(state!.options.type).toBe('danger');
  }));

  it('confirm() should use default title "Confirm Action" when no title is provided', fakeAsync(() => {
    service.confirm({ message: 'Proceed with this action?' });

    const state = service.activeDialog();
    expect(state).not.toBeNull();
    expect(state!.options.title).toBe('Confirm Action');
  }));

  it('confirm() should apply provided confirmText', fakeAsync(() => {
    service.confirm({ message: 'Proceed?', confirmText: 'Yes, proceed' });

    expect(service.activeDialog()!.options.confirmText).toBe('Yes, proceed');
  }));

  it('confirm() should apply provided cancelText', fakeAsync(() => {
    service.confirm({ message: 'Proceed?', cancelText: 'No, go back' });

    expect(service.activeDialog()!.options.cancelText).toBe('No, go back');
  }));

  it('confirm() should default confirmText to "Confirm" when not provided', fakeAsync(() => {
    service.confirm({ message: 'Proceed?' });

    expect(service.activeDialog()!.options.confirmText).toBe('Confirm');
  }));

  it('confirm() should default cancelText to "Cancel" when not provided', fakeAsync(() => {
    service.confirm({ message: 'Proceed?' });

    expect(service.activeDialog()!.options.cancelText).toBe('Cancel');
  }));

  it('confirm() should default type to "warning" when not provided', fakeAsync(() => {
    service.confirm({ message: 'Proceed?' });

    expect(service.activeDialog()!.options.type).toBe('warning');
  }));

  it('confirm() should accept "danger" as dialog type', fakeAsync(() => {
    service.confirm({ message: 'Irreversible action!', type: 'danger' });

    expect(service.activeDialog()!.options.type).toBe('danger');
  }));

  it('confirm() should accept "info" as dialog type', fakeAsync(() => {
    service.confirm({ message: 'Just an info prompt.', type: 'info' });

    expect(service.activeDialog()!.options.type).toBe('info');
  }));

  it('confirm() should accept "success" as dialog type', fakeAsync(() => {
    service.confirm({ message: 'Task completed.', type: 'success' });

    expect(service.activeDialog()!.options.type).toBe('success');
  }));

  it('confirm() should store a resolve function on the active dialog state', fakeAsync(() => {
    service.confirm({ message: 'Proceed?' });

    const state = service.activeDialog();
    expect(typeof state!.resolve).toBe('function');
  }));

  it('confirm() should return a Promise', () => {
    const result = service.confirm({ message: 'Proceed?' });

    expect(result).toBeInstanceOf(Promise);
  });

  it('confirm() should preserve an optional icon when provided', fakeAsync(() => {
    service.confirm({ message: 'Proceed?', icon: 'trash' });

    expect(service.activeDialog()!.options.icon).toBe('trash');
  }));

  // ─── handleConfirm() ────────────────────────────────────────────────────────

  it('handleConfirm() should resolve the Promise with true', fakeAsync(() => {
    let result: boolean | undefined;

    service.confirm({ message: 'Delete?' }).then(v => (result = v));
    service.handleConfirm();
    tick();

    expect(result).toBeTrue();
  }));

  it('handleConfirm() should clear activeDialog signal after resolving', fakeAsync(() => {
    service.confirm({ message: 'Delete?' });
    service.handleConfirm();
    tick();

    expect(service.activeDialog()).toBeNull();
  }));

  it('handleConfirm() when no dialog is active should not throw', () => {
    expect(() => service.handleConfirm()).not.toThrow();
  });

  it('handleConfirm() when no dialog is active should leave activeDialog as null', () => {
    service.handleConfirm();

    expect(service.activeDialog()).toBeNull();
  });

  // ─── handleCancel() ─────────────────────────────────────────────────────────

  it('handleCancel() should resolve the Promise with false', fakeAsync(() => {
    let result: boolean | undefined;

    service.confirm({ message: 'Delete?' }).then(v => (result = v));
    service.handleCancel();
    tick();

    expect(result).toBeFalse();
  }));

  it('handleCancel() should clear activeDialog signal after resolving', fakeAsync(() => {
    service.confirm({ message: 'Delete?' });
    service.handleCancel();
    tick();

    expect(service.activeDialog()).toBeNull();
  }));

  it('handleCancel() when no dialog is active should not throw', () => {
    expect(() => service.handleCancel()).not.toThrow();
  });

  it('handleCancel() when no dialog is active should leave activeDialog as null', () => {
    service.handleCancel();

    expect(service.activeDialog()).toBeNull();
  });

  // ─── Sequential dialogs ──────────────────────────────────────────────────────

  it('should support sequential confirm() calls without cross-contamination', fakeAsync(() => {
    let first: boolean | undefined;
    let second: boolean | undefined;

    service.confirm({ message: 'First?' }).then(v => (first = v));
    service.handleConfirm();
    tick();

    service.confirm({ message: 'Second?' }).then(v => (second = v));
    service.handleCancel();
    tick();

    expect(first).toBeTrue();
    expect(second).toBeFalse();
  }));

  it('a second confirm() before the first resolves should overwrite activeDialog', fakeAsync(() => {
    service.confirm({ message: 'First?' });
    service.confirm({ message: 'Second?' });

    expect(service.activeDialog()!.options.message).toBe('Second?');
  }));
});
