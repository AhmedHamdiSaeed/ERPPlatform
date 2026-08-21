import { TestBed, fakeAsync, tick } from '@angular/core/testing';
import { EmployeeListComponent } from './employee-list.component';
import { ToastService } from '../../../core/services/toast.service';
import { DialogService } from '../../../core/services/dialog.service';
import { MOCK_EMPLOYEES } from '../../../core/mock/mock-data';
import { RouterModule } from '@angular/router';

describe('EmployeeListComponent', () => {
  let component: EmployeeListComponent;
  let toastSpy: jasmine.SpyObj<ToastService>;
  let dialogSpy: jasmine.SpyObj<DialogService>;

  beforeEach(() => {
    toastSpy = jasmine.createSpyObj('ToastService', ['success', 'error', 'warning', 'info']);
    dialogSpy = jasmine.createSpyObj('DialogService', ['confirm']);

    TestBed.configureTestingModule({
      imports: [EmployeeListComponent, RouterModule.forRoot([])],
      providers: [
        { provide: ToastService, useValue: toastSpy },
        { provide: DialogService, useValue: dialogSpy }
      ]
    });

    const fixture = TestBed.createComponent(EmployeeListComponent);
    component = fixture.componentInstance;
    fixture.detectChanges();
  });

  it('should create the component', () => {
    expect(component).toBeTruthy();
  });

  it('should load employees from MOCK_EMPLOYEES on initialization', () => {
    expect(component.employees().length).toBe(MOCK_EMPLOYEES.length);
  });

  it('filteredEmployees() should return all employees when no filters applied', () => {
    component.searchQuery = '';
    component.statusFilter = 'ALL';
    component.departmentFilter = 'ALL';
    expect(component.filteredEmployees().length).toBe(MOCK_EMPLOYEES.length);
  });

  it('filteredEmployees() should filter by name search query', () => {
    const firstEmployee = MOCK_EMPLOYEES[0];
    component.searchQuery = firstEmployee.name.substring(0, 4);
    const results = component.filteredEmployees();
    expect(results.every(e => e.name.toLowerCase().includes(component.searchQuery.toLowerCase()))).toBeTrue();
  });

  it('filteredEmployees() should filter by email search query', () => {
    const firstEmployee = MOCK_EMPLOYEES[0];
    component.searchQuery = firstEmployee.email.split('@')[0];
    const results = component.filteredEmployees();
    expect(results.length).toBeGreaterThanOrEqualTo(1);
    expect(results.some(e => e.email === firstEmployee.email)).toBeTrue();
  });

  it('filteredEmployees() should filter by employee code', () => {
    const firstEmployee = MOCK_EMPLOYEES[0];
    component.searchQuery = firstEmployee.employeeCode;
    const results = component.filteredEmployees();
    expect(results.length).toBe(1);
    expect(results[0].employeeCode).toBe(firstEmployee.employeeCode);
  });

  it('filteredEmployees() should filter by status Active', () => {
    component.statusFilter = 'Active';
    const results = component.filteredEmployees();
    expect(results.every(e => e.status === 'Active')).toBeTrue();
  });

  it('filteredEmployees() should return empty when status has no match', () => {
    component.statusFilter = 'NonExistentStatus';
    const results = component.filteredEmployees();
    expect(results.length).toBe(0);
  });

  it('filteredEmployees() should filter by department', () => {
    const dept = MOCK_EMPLOYEES[0].departmentName;
    component.departmentFilter = dept;
    const results = component.filteredEmployees();
    expect(results.every(e => e.departmentName === dept)).toBeTrue();
  });

  it('filteredEmployees() should combine search and status filters', () => {
    component.statusFilter = 'Active';
    component.searchQuery = 'a';
    const results = component.filteredEmployees();
    expect(results.every(e =>
      e.status === 'Active' &&
      (e.name.toLowerCase().includes('a') || e.email.toLowerCase().includes('a') || e.employeeCode.toLowerCase().includes('a'))
    )).toBeTrue();
  });

  it('openAddModal() should open modal in add mode', () => {
    component.openAddModal();
    expect(component.showModal()).toBeTrue();
    expect(component.isEditMode).toBeFalse();
    expect(component.currentEmp.status).toBe('Active');
  });

  it('openEditModal() should open modal in edit mode with employee data', () => {
    const emp = MOCK_EMPLOYEES[0];
    component.openEditModal(emp);
    expect(component.showModal()).toBeTrue();
    expect(component.isEditMode).toBeTrue();
    expect(component.currentEmp.name).toBe(emp.name);
  });

  it('saveEmployee() in add mode should add employee to list and show toast', () => {
    const initialCount = component.employees().length;
    component.isEditMode = false;
    component.currentEmp = {
      id: 'test-id',
      employeeCode: 'EMP-NEW',
      name: 'New Employee',
      email: 'new@erp.com',
      phone: '',
      position: 'Tester',
      departmentName: 'QA',
      salary: 50000,
      joiningDate: '2026-01-01',
      status: 'Active',
      avatar: '',
      managerName: '',
      location: 'Cairo HQ'
    };
    component.saveEmployee();
    expect(component.employees().length).toBe(initialCount + 1);
    expect(toastSpy.success).toHaveBeenCalledWith('New employee registered successfully.');
    expect(component.showModal()).toBeFalse();
  });

  it('saveEmployee() in edit mode should update employee and show toast', () => {
    const emp = MOCK_EMPLOYEES[0];
    component.isEditMode = true;
    component.currentEmp = { ...emp, position: 'Updated Position' };
    component.saveEmployee();
    const updated = component.employees().find(e => e.id === emp.id);
    expect(updated?.position).toBe('Updated Position');
    expect(toastSpy.success).toHaveBeenCalledWith('Employee profile updated successfully.');
  });

  it('deleteEmployee() should not delete when dialog returns false', fakeAsync(async () => {
    dialogSpy.confirm.and.returnValue(Promise.resolve(false));
    const initialCount = component.employees().length;
    const empId = component.employees()[0].id;
    await component.deleteEmployee(empId);
    tick();
    expect(component.employees().length).toBe(initialCount);
  }));

  it('deleteEmployee() should delete employee and show toast when dialog returns true', fakeAsync(async () => {
    dialogSpy.confirm.and.returnValue(Promise.resolve(true));
    const initialCount = component.employees().length;
    const empId = component.employees()[0].id;
    await component.deleteEmployee(empId);
    tick();
    expect(component.employees().length).toBe(initialCount - 1);
    expect(component.employees().find(e => e.id === empId)).toBeUndefined();
    expect(toastSpy.success).toHaveBeenCalledWith('Employee record deleted.');
  }));

  it('deleteEmployee() should call dialog.confirm with danger type', fakeAsync(async () => {
    dialogSpy.confirm.and.returnValue(Promise.resolve(false));
    await component.deleteEmployee(MOCK_EMPLOYEES[0].id);
    tick();
    expect(dialogSpy.confirm).toHaveBeenCalledWith(
      jasmine.objectContaining({ type: 'danger' })
    );
  }));

  it('exportCsv() should show success toast', () => {
    component.exportCsv();
    expect(toastSpy.success).toHaveBeenCalledWith('Employee roster exported to CSV file.');
  });
});
