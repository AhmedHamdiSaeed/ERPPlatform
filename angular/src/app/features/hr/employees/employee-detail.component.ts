import { Component, inject, signal } from '@angular/core';
import { TranslatePipe } from '../../../shared/pipes/translate.pipe';
import { CommonModule } from '@angular/common';
import { FormsModule } from '@angular/forms';
import { ActivatedRoute, RouterModule } from '@angular/router';
import { Employee, AttendanceRecord, LeaveRequest, EmployeeContract, HrAction, EmployeeDocument, Department } from '../../../core/models/erp-models';
import { HrApiService } from '../../../core/services/api/hr-api.service';
import { ToastService } from '../../../core/services/toast.service';

@Component({
  selector: 'app-employee-detail',
  standalone: true,
  imports: [CommonModule, RouterModule, FormsModule, TranslatePipe],
  templateUrl: './employee-detail.component.html'
})
export class EmployeeDetailComponent {
  private route = inject(ActivatedRoute);
  private hrApi = inject(HrApiService);
  private toast = inject(ToastService);

  employee = signal<Employee | undefined>(undefined);
  activeTab = signal('HR:TabOverview');
  loading = signal(false);

  // Sub-entity signals
  attendance = signal<AttendanceRecord[]>([]);
  leaveRequests = signal<LeaveRequest[]>([]);
  contracts = signal<EmployeeContract[]>([]);
  hrActions = signal<HrAction[]>([]);
  documents = signal<EmployeeDocument[]>([]);
  departments = signal<Department[]>([]);

  // Modals state
  showEditModal = signal(false);
  editEmp: Partial<Employee> = {};

  showHrActionModal = signal(false);
  newAction: {
    actionType: string;
    effectiveDate: string;
    remarks: string;
    newPosition?: string;
    newDepartmentName?: string;
    newSalary?: number;
    newManagerName?: string;
    newLocation?: string;
    newStatus?: string;
    newJobGradeName?: string;
  } = {
    actionType: 'Promotion',
    effectiveDate: new Date().toISOString().substring(0, 10),
    remarks: ''
  };

  showContractModal = signal(false);
  newContract: Partial<EmployeeContract> = {
    contractType: 'Permanent',
    startDate: new Date().toISOString().substring(0, 10),
    basicSalary: 0,
    housingAllowance: 0,
    transportationAllowance: 0,
    otherAllowances: 0,
    workingHoursPerWeek: 40,
    noticePeriodDays: 30,
    status: 'Active'
  };

  showRenewContractModal = signal(false);
  selectedContractId: string = '';
  renewData = {
    newStartDate: new Date().toISOString().substring(0, 10),
    newEndDate: '',
    newBasicSalary: 0,
    notes: ''
  };

  showDocumentModal = signal(false);
  newDoc: Partial<EmployeeDocument> = {
    documentType: 'NationalId',
    documentTitle: '',
    documentNumber: '',
    fileUrl: '',
    notes: ''
  };

  tabs = [
    'HR:TabOverview',
    'HR:TabPersonalInfo',
    'HR:TabEmployment',
    'HR:Contracts',
    'HR:HrActions',
    'HR:Attendance',
    'HR:Leave',
    'HR:TabDocuments'
  ];

  constructor() {
    const id = this.route.snapshot.paramMap.get('id');
    if (id) {
      this.load(id);
    } else {
      this.toast.error('No employee id provided.');
    }
  }

  async load(id: string) {
    this.loading.set(true);
    try {
      const [emp, att, leave, ctrs, acts, docs, depts] = await Promise.all([
        this.hrApi.getEmployee(id),
        this.hrApi.getAttendance(id),
        this.hrApi.getLeaveRequests(),
        this.hrApi.getContractsByEmployee(id),
        this.hrApi.getHrActions(id),
        this.hrApi.getEmployeeDocuments(id),
        this.hrApi.getDepartments()
      ]);

      this.employee.set(emp);
      this.attendance.set(att);
      this.leaveRequests.set(leave.filter(l => l.employeeId === id));
      this.contracts.set(ctrs);
      this.hrActions.set(acts);
      this.documents.set(docs);
      this.departments.set(depts);
    } catch (e) {
      console.error('Failed to load employee details', e);
      this.toast.error('Could not load the employee from the server.');
    } finally {
      this.loading.set(false);
    }
  }

  openEditModal() {
    const emp = this.employee();
    if (!emp) return;
    this.editEmp = { ...emp };
    this.showEditModal.set(true);
  }

  async saveEmployee() {
    const emp = this.employee();
    if (!emp?.id) return;
    try {
      await this.hrApi.updateEmployee(emp.id, this.editEmp);
      this.toast.success('Employee profile updated successfully.');
      this.showEditModal.set(false);
      await this.load(emp.id);
    } catch (e) {
      console.error('Failed to save employee', e);
      this.toast.error('Failed to save the employee record.');
    }
  }

  // ─── HR Action ───
  openHrActionModal() {
    const emp = this.employee();
    if (!emp) return;
    this.newAction = {
      actionType: 'Promotion',
      effectiveDate: new Date().toISOString().substring(0, 10),
      remarks: '',
      newPosition: emp.position,
      newDepartmentName: emp.departmentName,
      newSalary: emp.salary,
      newManagerName: emp.managerName,
      newLocation: emp.location,
      newStatus: emp.status,
      newJobGradeName: emp.jobGradeName
    };
    this.showHrActionModal.set(true);
  }

  async submitHrAction() {
    const emp = this.employee();
    if (!emp?.id) return;
    try {
      await this.hrApi.createHrAction({
        employeeId: emp.id,
        ...this.newAction
      });
      this.toast.success('HR action executed and employee record updated.');
      this.showHrActionModal.set(false);
      await this.load(emp.id);
    } catch (e) {
      console.error('Failed to record HR action', e);
      this.toast.error('Failed to execute HR action.');
    }
  }

  // ─── Contracts ───
  openNewContractModal() {
    const emp = this.employee();
    if (!emp) return;
    this.newContract = {
      employeeId: emp.id,
      employeeName: emp.name,
      contractType: 'Permanent',
      startDate: new Date().toISOString().substring(0, 10),
      basicSalary: emp.salary ?? 0,
      housingAllowance: 0,
      transportationAllowance: 0,
      otherAllowances: 0,
      workingHoursPerWeek: 40,
      noticePeriodDays: 30,
      status: 'Active'
    };
    this.showContractModal.set(true);
  }

  async saveContract() {
    const emp = this.employee();
    if (!emp?.id) return;
    try {
      this.newContract.employeeId = emp.id;
      this.newContract.employeeName = emp.name;
      await this.hrApi.createContract(this.newContract);
      this.toast.success('Contract added successfully.');
      this.showContractModal.set(false);
      await this.load(emp.id);
    } catch (e) {
      console.error('Failed to create contract', e);
      this.toast.error('Failed to create contract.');
    }
  }

  openRenewModal(contract: EmployeeContract) {
    this.selectedContractId = contract.id;
    this.renewData = {
      newStartDate: new Date().toISOString().substring(0, 10),
      newEndDate: '',
      newBasicSalary: contract.basicSalary,
      notes: ''
    };
    this.showRenewContractModal.set(true);
  }

  async submitContractRenewal() {
    const emp = this.employee();
    if (!emp?.id || !this.selectedContractId) return;
    try {
      await this.hrApi.renewContract(this.selectedContractId, this.renewData);
      this.toast.success('Contract renewed successfully.');
      this.showRenewContractModal.set(false);
      await this.load(emp.id);
    } catch (e) {
      console.error('Failed to renew contract', e);
      this.toast.error('Failed to renew contract.');
    }
  }

  // ─── Documents ───
  openDocumentModal() {
    const emp = this.employee();
    if (!emp) return;
    this.newDoc = {
      employeeId: emp.id,
      employeeName: emp.name,
      documentType: 'NationalId',
      documentTitle: '',
      documentNumber: '',
      fileUrl: '',
      notes: ''
    };
    this.showDocumentModal.set(true);
  }

  async saveDocument() {
    const emp = this.employee();
    if (!emp?.id) return;
    try {
      this.newDoc.employeeId = emp.id;
      this.newDoc.employeeName = emp.name;
      await this.hrApi.createEmployeeDocument(this.newDoc);
      this.toast.success('Document uploaded to employee vault.');
      this.showDocumentModal.set(false);
      await this.load(emp.id);
    } catch (e) {
      console.error('Failed to upload document', e);
      this.toast.error('Failed to upload document.');
    }
  }

  async verifyDocument(doc: EmployeeDocument) {
    try {
      await this.hrApi.verifyEmployeeDocument(doc.id, 'HR Specialist');
      this.toast.success('Document marked as verified.');
      const emp = this.employee();
      if (emp?.id) await this.load(emp.id);
    } catch (e) {
      console.error('Failed to verify document', e);
      this.toast.error('Failed to verify document.');
    }
  }

  sendEmail() {
    const emp = this.employee();
    if (emp?.email) {
      window.location.href = `mailto:${emp.email}`;
    }
  }
}

