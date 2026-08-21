import { TestBed } from '@angular/core/testing';
import { MyTasksComponent } from './my-tasks.component';
import { ToastService } from '../../../core/services/toast.service';
import { MOCK_TASKS } from '../../../core/mock/mock-data';

describe('MyTasksComponent', () => {
  let component: MyTasksComponent;
  let toastSpy: jasmine.SpyObj<ToastService>;

  beforeEach(() => {
    toastSpy = jasmine.createSpyObj('ToastService', ['success', 'error', 'warning', 'info']);

    TestBed.configureTestingModule({
      imports: [MyTasksComponent],
      providers: [
        { provide: ToastService, useValue: toastSpy }
      ]
    });

    const fixture = TestBed.createComponent(MyTasksComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create the component', () => {
    expect(component).toBeTruthy();
  });

  it('should load tasks from MOCK_TASKS on initialization', () => {
    expect(component.tasks().length).toBe(MOCK_TASKS.length);
  });

  it('approve() should remove the task from the list', () => {
    const initialCount = component.tasks().length;
    const taskId = component.tasks()[0].id;

    component.approve(taskId);

    expect(component.tasks().length).toBe(initialCount - 1);
    expect(component.tasks().find(t => t.id === taskId)).toBeUndefined();
  });

  it('approve() should call toast.success with correct message', () => {
    const taskId = component.tasks()[0].id;
    component.approve(taskId);

    expect(toastSpy.success).toHaveBeenCalledWith(
      'Workflow approval task completed successfully.',
      'Task Approved'
    );
  });

  it('reject() should remove the task from the list', () => {
    const initialCount = component.tasks().length;
    const taskId = component.tasks()[0].id;

    component.reject(taskId);

    expect(component.tasks().length).toBe(initialCount - 1);
    expect(component.tasks().find(t => t.id === taskId)).toBeUndefined();
  });

  it('reject() should call toast.error with correct message', () => {
    const taskId = component.tasks()[0].id;
    component.reject(taskId);

    expect(toastSpy.error).toHaveBeenCalledWith(
      'Workflow task rejected.',
      'Task Rejected'
    );
  });

  it('requestChanges() should NOT remove task from list', () => {
    const initialCount = component.tasks().length;
    const taskId = component.tasks()[0].id;

    component.requestChanges(taskId);

    expect(component.tasks().length).toBe(initialCount);
    expect(component.tasks().find(t => t.id === taskId)).toBeDefined();
  });

  it('requestChanges() should call toast.warning with correct message', () => {
    component.requestChanges(component.tasks()[0].id);

    expect(toastSpy.warning).toHaveBeenCalledWith(
      'Requested modifications sent back to task submitter.',
      'Changes Requested'
    );
  });

  it('approve() then reject() different tasks should remove both', () => {
    if (component.tasks().length < 2) return;

    const firstId = component.tasks()[0].id;
    const secondId = component.tasks()[1].id;
    const initialCount = component.tasks().length;

    component.approve(firstId);
    component.reject(secondId);

    expect(component.tasks().length).toBe(initialCount - 2);
  });

  it('approve() on non-existent id should not change tasks', () => {
    const initialCount = component.tasks().length;
    component.approve('non-existent-id-123');
    expect(component.tasks().length).toBe(initialCount);
  });
});
