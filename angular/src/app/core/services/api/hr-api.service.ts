import { Injectable } from '@angular/core';
import { ErpApiService, toDateString, AbpEntity } from './erp-api.service';
import { Employee, Department, LeaveRequest } from '../../models/erp-models';
import { environment } from '../../../../environments/environment';

interface EmployeeDto extends AbpEntity {
  employeeCode: string; name: string; email: string; phone: string; position: string;
  departmentName: string; salary: number; joiningDate: string; status: string;
  avatar?: string; managerName?: string; location?: string;
}

interface DepartmentDto extends AbpEntity {
  code: string; name: string; description: string; managerName: string;
  employeeCount: number; budget: number;
}

interface LeaveRequestDto extends AbpEntity {
  employeeId: string; employeeName: string; leaveType: string;
  startDate: string; endDate: string; daysCount: number;
  reason: string; status: string; appliedDate: string;
}

@Injectable({ providedIn: 'root' })
export class HrApiService extends ErpApiService {
  protected override apiPrefix(): string {
    return `${environment.apis.default.url}/api/hr`;
  }

  getEmployees(): Promise<Employee[]> {
    return this.getList<EmployeeDto>('employee').then(items =>
      items.map(e => ({
        id: e.id,
        employeeCode: e.employeeCode,
        name: e.name,
        email: e.email,
        phone: e.phone,
        position: e.position,
        departmentId: '',
        departmentName: e.departmentName,
        salary: e.salary,
        joiningDate: toDateString(e.joiningDate),
        status: e.status,
        avatar: e.avatar || 'https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=150',
        managerName: e.managerName,
        location: e.location
      })) as Employee[]
    );
  }

  createEmployee(emp: Partial<Employee>): Promise<Employee> {
    const body = {
      employeeCode: emp.employeeCode, name: emp.name, email: emp.email, phone: emp.phone ?? '',
      position: emp.position ?? '', departmentName: emp.departmentName ?? '',
      salary: emp.salary ?? 0, status: emp.status ?? 'Active'
    };
    return this.post<EmployeeDto>('employee', body).then(() => this.getEmployees().then(list => list[0]));
  }

  updateEmployee(id: string, emp: Partial<Employee>): Promise<void> {
    const body = {
      employeeCode: emp.employeeCode, name: emp.name, email: emp.email, phone: emp.phone ?? '',
      position: emp.position ?? '', departmentName: emp.departmentName ?? '',
      salary: emp.salary ?? 0, status: emp.status ?? 'Active'
    };
    return this.put<void>(`employee/${id}`, body);
  }

  deleteEmployee(id: string): Promise<void> {
    return this.delete(`employee/${id}`);
  }

  getDepartments(): Promise<Department[]> {
    return this.getList<DepartmentDto>('department').then(items =>
      items.map(d => ({
        id: d.id, code: d.code, name: d.name, description: d.description,
        managerName: d.managerName, employeeCount: d.employeeCount, budget: d.budget
      })) as Department[]
    );
  }

  createDepartment(dept: Partial<Department>): Promise<void> {
    return this.post('department', dept);
  }

  updateDepartment(id: string, dept: Partial<Department>): Promise<void> {
    return this.put(`department/${id}`, dept);
  }

  deleteDepartment(id: string): Promise<void> {
    return this.delete(`department/${id}`);
  }

  getLeaveRequests(): Promise<LeaveRequest[]> {
    return this.getList<LeaveRequestDto>('leave-request').then(items =>
      items.map(l => ({
        id: l.id,
        employeeId: l.employeeId,
        employeeName: l.employeeName,
        avatar: 'https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=150',
        leaveType: l.leaveType,
        startDate: toDateString(l.startDate),
        endDate: toDateString(l.endDate),
        daysCount: l.daysCount,
        reason: l.reason,
        status: l.status,
        appliedDate: toDateString(l.appliedDate)
      })) as LeaveRequest[]
    );
  }

  createLeaveRequest(leave: Partial<LeaveRequest>): Promise<void> {
    return this.post('leave-request', leave);
  }

  approveLeaveRequest(id: string): Promise<void> {
    return this.post(`leave-request/${id}/approve`, {});
  }

  rejectLeaveRequest(id: string): Promise<void> {
    return this.post(`leave-request/${id}/reject`, {});
  }

  deleteLeaveRequest(id: string): Promise<void> {
    return this.delete(`leave-request/${id}`);
  }
}
