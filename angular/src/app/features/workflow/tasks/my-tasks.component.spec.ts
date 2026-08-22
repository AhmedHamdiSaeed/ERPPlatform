import { TestBed } from '@angular/core/testing';
import { MyTasksComponent } from './my-tasks.component';
import { ToastService } from '../../../core/services/toast.service';
import { WorkflowApiService } from '../../../core/services/api/workflow-api.service';
import { MOCK_TASKS } from '../../../core/mock/mock-data';

describe('MyTasksComponent', () => {
  let component: MyTasksComponent;
  let toastSpy: jasmine.SpyObj<ToastService>;
  let workflowApiSpy: jasmine.SpyObj<WorkflowApiService>;

  beforeEach(async () => {
    toastSpy = jasmine.createSpyObj('ToastService', ['success', 'error', 'warning', 'info']);
    workflowApiSpy = jasmine.createSpyObj('WorkflowApiService', ['getTasks', 'approveTask', 'rejectTask']);
    workflowApiSpy.getTasks.and.resolveTo(MOCK_TASKS.map(t => ({ ...t })));
    workflowApiSpy.approveTask.and.resolveTo(undefined);
    workflowApiSpy.rejectTask.and.resolveTo(undefined);

    await TestBed.configureTestingModule({
      imports: [MyTasksComponent],
      providers: [
        { provide: ToastService, useValue: toastSpy },
        { provide: WorkflowApiService, useValue: workflowApiSpy }
      ]
    }).compileComponents();

    const fixture = TestBed.createComponent(MyTasksComponent);
    component = fixture.componentInstance;
    await fixture.whenStable();
  });

  it('should create the component', () => {
    expect(component).toBeTruthy();
  });

  it('should load tasks from the API on initialization', () => {
    expect(workflowApiSpy.getTasks).toHaveBeenCalled();
    expect(component.tasks().length).toBe(MOCK_TASKS.length);
  });

  it('approve() should call the API and show success toast', async () => {
    const taskId = component.tasks()[0].id;

    await component.approve(taskId);

    expect(workflowApiSpy.approveTask).toHaveBeenCalledWith(taskId);
    expect(toastSpy.success).toHaveBeenCalledWith(
      'Workflow approval task completed successfully.',
      'Task Approved'
    );
  });

  it('reject() should call the API and show rejection toast', async () => {
    const taskId = component.tasks()[0].id;

    await component.reject(taskId);

    expect(workflowApiSpy.rejectTask).toHaveBeenCalledWith(taskId);
    expect(toastSpy.error).toHaveBeenCalledWith(
      'Workflow task rejected.',
      'Task Rejected'
    );
  });

  it('approve() failure should show error toast without success message', async () => {
    workflowApiSpy.approveTask.and.rejectWith(new Error('server down'));
    const taskId = component.tasks()[0].id;

    await component.approve(taskId);

    expect(toastSpy.success).not.toHaveBeenCalled();
    expect(toastSpy.error).toHaveBeenCalledWith(
      'Failed to approve the workflow task.',
      'Approval Failed'
    );
  });

  it('requestChanges() should NOT touch the task list or API', () => {
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
});
