import { Injectable } from '@angular/core';
import { ErpApiService, toDateString, AbpEntity } from './erp-api.service';
import {
  Employee, Department, LeaveRequest, AttendanceRecord, EmployeeContract, HrAction,
  EmployeeDocument, JobGrade, JobPosition, JobRequisition, Candidate, OnboardingTask,
  OffboardingRequest, EmployeeLoan, LoanInstallment, PerformanceReview, PerformanceGoal,
  TrainingCourse, TrainingEnrollment, EmployeeCertification, BenefitPlan, EmployeeBenefit,
  WorkShift, ShiftAssignment, TeamSummary
} from '../../models/erp-models';
import { environment } from '../../../../environments/environment';

const DEFAULT_AVATAR = 'https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=150';

interface EmployeeDto extends AbpEntity {
  employeeCode: string; name: string; email: string; phone: string; position: string;
  departmentId?: string; departmentName: string; branchId?: string; branchName?: string;
  salary: number; joiningDate: string; status: string;
  avatar?: string; managerName?: string; location?: string; leaveBalance?: number;
  nationalId?: string; passportNumber?: string; nationality?: string;
  dateOfBirth?: string; gender?: string; maritalStatus?: string;
  emergencyContactName?: string; emergencyContactPhone?: string; emergencyContactRelation?: string;
  bankName?: string; bankAccountNumber?: string; iban?: string; swiftCode?: string;
  employmentType?: string; probationEndDate?: string; contractEndDate?: string;
  jobGradeId?: string; jobGradeName?: string; costCenterCode?: string; noticePeriodDays?: number;
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

interface AttendanceRecordDto extends AbpEntity {
  employeeId: string; employeeName: string; departmentName: string;
  date: string; checkIn: string; checkOut: string;
  workingHours: number; overtimeHours: number; status: AttendanceRecord['status'];
}

function mapEmployee(e: EmployeeDto): Employee {
  return {
    id: e.id,
    employeeCode: e.employeeCode,
    name: e.name,
    email: e.email,
    phone: e.phone,
    position: e.position,
    departmentId: e.departmentId || '',
    departmentName: e.departmentName,
    branchId: e.branchId,
    branchName: e.branchName,
    salary: e.salary,
    joiningDate: toDateString(e.joiningDate),
    status: (e.status || 'Active') as Employee['status'],
    avatar: e.avatar || DEFAULT_AVATAR,
    managerName: e.managerName,
    location: e.location || 'Cairo HQ',
    leaveBalance: e.leaveBalance ?? 21,
    nationalId: e.nationalId,
    passportNumber: e.passportNumber,
    nationality: e.nationality || 'Egyptian',
    dateOfBirth: e.dateOfBirth ? toDateString(e.dateOfBirth) : undefined,
    gender: (e.gender || 'Male') as Employee['gender'],
    maritalStatus: (e.maritalStatus || 'Single') as Employee['maritalStatus'],
    emergencyContactName: e.emergencyContactName,
    emergencyContactPhone: e.emergencyContactPhone,
    emergencyContactRelation: e.emergencyContactRelation,
    bankName: e.bankName,
    bankAccountNumber: e.bankAccountNumber,
    iban: e.iban,
    swiftCode: e.swiftCode,
    employmentType: (e.employmentType || 'FullTime') as Employee['employmentType'],
    probationEndDate: e.probationEndDate ? toDateString(e.probationEndDate) : undefined,
    contractEndDate: e.contractEndDate ? toDateString(e.contractEndDate) : undefined,
    jobGradeId: e.jobGradeId,
    jobGradeName: e.jobGradeName,
    costCenterCode: e.costCenterCode,
    noticePeriodDays: e.noticePeriodDays ?? 30
  };
}

@Injectable({ providedIn: 'root' })
export class HrApiService extends ErpApiService {
  protected override apiPrefix(): string {
    return `${environment.apis.default.url}/api/hr`;
  }

  // ─── Employee Master Data ───
  getEmployees(): Promise<Employee[]> {
    return this.getList<EmployeeDto>('employee').then(items => items.map(mapEmployee));
  }

  getEmployeesPaged(
    skipCount: number = 0,
    maxResultCount: number = 50,
    filter?: string,
    status?: string,
    departmentName?: string,
    employmentType?: string
  ): Promise<{ totalCount: number; items: Employee[] }> {
    let params = `skipCount=${skipCount}&maxResultCount=${maxResultCount}`;
    if (filter) params += `&filter=${encodeURIComponent(filter)}`;
    if (status && status !== 'ALL') params += `&status=${encodeURIComponent(status)}`;
    if (departmentName && departmentName !== 'ALL') params += `&departmentName=${encodeURIComponent(departmentName)}`;
    if (employmentType && employmentType !== 'ALL') params += `&employmentType=${encodeURIComponent(employmentType)}`;

    return this.get<{ totalCount: number; items: EmployeeDto[] }>(`employee?${params}`)
      .then(res => ({
        totalCount: res.totalCount || 0,
        items: (res.items || []).map(mapEmployee)
      }));
  }

  getEmployee(id: string): Promise<Employee> {
    return this.get<EmployeeDto>(`employee/${id}`).then(mapEmployee);
  }

  createEmployee(emp: Partial<Employee>): Promise<Employee> {
    return this.post<EmployeeDto>('employee', emp).then(res => mapEmployee(res));
  }

  updateEmployee(id: string, emp: Partial<Employee>): Promise<void> {
    return this.put<void>(`employee/${id}`, emp);
  }

  deleteEmployee(id: string): Promise<void> {
    return this.delete(`employee/${id}`);
  }

  // ─── Contracts Lifecycle ───
  getContracts(): Promise<EmployeeContract[]> {
    return this.getList<EmployeeContract>('contract');
  }

  getContractsByEmployee(employeeId: string): Promise<EmployeeContract[]> {
    return this.get<EmployeeContract[]>(`contract/contracts-by-employee-id?employeeId=${encodeURIComponent(employeeId)}`);
  }

  getExpiringContracts(daysThreshold: number = 30): Promise<EmployeeContract[]> {
    return this.get<EmployeeContract[]>(`contract/expiring-contracts?daysThreshold=${daysThreshold}`);
  }

  createContract(contract: Partial<EmployeeContract>): Promise<EmployeeContract> {
    return this.post<EmployeeContract>('contract', contract);
  }

  updateContract(id: string, contract: Partial<EmployeeContract>): Promise<void> {
    return this.put<void>(`contract/${id}`, contract);
  }

  renewContract(id: string, renewData: { newStartDate: string; newEndDate?: string; newBasicSalary?: number; notes?: string }): Promise<EmployeeContract> {
    return this.post<EmployeeContract>(`contract/${id}/renew`, renewData);
  }

  terminateContract(id: string, reason: string): Promise<EmployeeContract> {
    return this.post<EmployeeContract>(`contract/${id}/terminate?reason=${encodeURIComponent(reason)}`, {});
  }

  // ─── HR Actions & Career Timeline ───
  getHrActions(employeeId?: string): Promise<HrAction[]> {
    if (employeeId) {
      return this.get<HrAction[]>(`hr-action/actions-by-employee-id?employeeId=${encodeURIComponent(employeeId)}`);
    }
    return this.getList<HrAction>('hr-action');
  }

  createHrAction(action: {
    employeeId: string;
    actionType: string;
    effectiveDate: string;
    remarks?: string;
    newPosition?: string;
    newDepartmentName?: string;
    newSalary?: number;
    newManagerName?: string;
    newLocation?: string;
    newStatus?: string;
    newJobGradeName?: string;
  }): Promise<HrAction> {
    return this.post<HrAction>('hr-action', action);
  }

  approveHrAction(id: string, approvedBy: string = 'HR Admin'): Promise<HrAction> {
    return this.post<HrAction>(`hr-action/${id}/approve?approvedBy=${encodeURIComponent(approvedBy)}`, {});
  }

  rejectHrAction(id: string, rejectedBy: string = 'HR Admin', reason: string = 'Policy constraint'): Promise<HrAction> {
    return this.post<HrAction>(`hr-action/${id}/reject?rejectedBy=${encodeURIComponent(rejectedBy)}&reason=${encodeURIComponent(reason)}`, {});
  }

  // ─── Employee Document Vault ───
  getEmployeeDocuments(employeeId?: string): Promise<EmployeeDocument[]> {
    if (employeeId) {
      return this.get<EmployeeDocument[]>(`employee-document/documents-by-employee-id?employeeId=${encodeURIComponent(employeeId)}`);
    }
    return this.getList<EmployeeDocument>('employee-document');
  }

  getExpiringDocuments(daysThreshold: number = 30): Promise<EmployeeDocument[]> {
    return this.get<EmployeeDocument[]>(`employee-document/expiring-documents?daysThreshold=${daysThreshold}`);
  }

  createEmployeeDocument(doc: Partial<EmployeeDocument>): Promise<EmployeeDocument> {
    return this.post<EmployeeDocument>('employee-document', doc);
  }

  verifyEmployeeDocument(id: string, verifiedBy: string = 'HR Officer'): Promise<EmployeeDocument> {
    return this.post<EmployeeDocument>(`employee-document/${id}/verify?verifiedBy=${encodeURIComponent(verifiedBy)}`, {});
  }

  deleteEmployeeDocument(id: string): Promise<void> {
    return this.delete(`employee-document/${id}`);
  }

  // ─── Job Grades & Positions ───
  getJobGrades(): Promise<JobGrade[]> {
    return this.getList<JobGrade>('job-grade');
  }

  getJobPositions(): Promise<JobPosition[]> {
    return this.getList<JobPosition>('job-position');
  }

  // ─── Departments ───
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

  // ─── Leave Requests ───
  getLeaveRequests(): Promise<LeaveRequest[]> {
    return this.getList<LeaveRequestDto>('leave-request').then(items =>
      items.map(l => ({
        id: l.id,
        employeeId: l.employeeId,
        employeeName: l.employeeName,
        avatar: DEFAULT_AVATAR,
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

  // ─── Attendance ───
  getAttendance(employeeId?: string): Promise<AttendanceRecord[]> {
    const route = employeeId ? `attendance?employeeId=${encodeURIComponent(employeeId)}` : 'attendance';
    return this.getList<AttendanceRecordDto>(route).then(items =>
      items.map(a => ({
        id: a.id,
        employeeId: a.employeeId,
        employeeName: a.employeeName,
        avatar: DEFAULT_AVATAR,
        departmentName: a.departmentName,
        date: toDateString(a.date),
        checkIn: a.checkIn || '-',
        checkOut: a.checkOut || '-',
        workingHours: a.workingHours,
        overtimeHours: a.overtimeHours,
        status: a.status
      })) as AttendanceRecord[]
    );
  }

  checkIn(employeeId: string, employeeName: string, departmentName: string): Promise<void> {
    const params = new URLSearchParams({
      employeeName,
      departmentName
    });
    return this.post(`attendance/check-in/${encodeURIComponent(employeeId)}?${params.toString()}`, {});
  }

  // ─── Recruitment & ATS ───
  getRequisitions(): Promise<JobRequisition[]> {
    return this.get<JobRequisition[]>('recruitment/requisitions');
  }

  createRequisition(req: Partial<JobRequisition>): Promise<JobRequisition> {
    return this.post<JobRequisition>('recruitment/requisition', req);
  }

  getCandidates(): Promise<Candidate[]> {
    return this.getList<Candidate>('recruitment');
  }

  createCandidate(cand: Partial<Candidate>): Promise<Candidate> {
    return this.post<Candidate>('recruitment', cand);
  }

  updateCandidateStage(id: string, newStage: string): Promise<Candidate> {
    return this.post<Candidate>(`recruitment/${id}/stage?newStage=${encodeURIComponent(newStage)}`, {});
  }

  convertCandidateToEmployee(candidateId: string, input: {
    employeeCode?: string;
    departmentId?: string;
    departmentName: string;
    agreedBasicSalary: number;
    housingAllowance: number;
    transportAllowance: number;
    joiningDate: string;
    employmentType: string;
    jobGradeName?: string;
  }): Promise<Employee> {
    return this.post<Employee>(`recruitment/${candidateId}/convert-to-employee`, input);
  }

  // ─── Onboarding & Offboarding Lifecycle ───
  getOnboardingTasks(employeeId: string): Promise<OnboardingTask[]> {
    return this.get<OnboardingTask[]>(`lifecycle-onboarding/tasks-by-employee-id?employeeId=${encodeURIComponent(employeeId)}`);
  }

  createOnboardingTask(task: Partial<OnboardingTask>): Promise<OnboardingTask> {
    return this.post<OnboardingTask>('lifecycle-onboarding/task', task);
  }

  completeOnboardingTask(taskId: string): Promise<OnboardingTask> {
    return this.post<OnboardingTask>(`lifecycle-onboarding/tasks/${taskId}/complete`, {});
  }

  generateDefaultOnboardingTasks(employeeId: string): Promise<OnboardingTask[]> {
    return this.post<OnboardingTask[]>(`lifecycle-onboarding/generate-default-onboarding-checklist?employeeId=${encodeURIComponent(employeeId)}`, {});
  }

  getOffboardingRequests(): Promise<OffboardingRequest[]> {
    return this.get<OffboardingRequest[]>('lifecycle-onboarding/offboarding-requests');
  }

  submitOffboardingRequest(request: { employeeId: string; resignationDate: string; lastWorkingDay: string; reason: string; exitInterviewNotes?: string }): Promise<OffboardingRequest> {
    return this.post<OffboardingRequest>('lifecycle-onboarding/offboarding-request', request);
  }

  updateClearanceStatus(id: string, department: string, status: string): Promise<OffboardingRequest> {
    return this.post<OffboardingRequest>(`lifecycle-onboarding/offboarding/${id}/clearance?department=${encodeURIComponent(department)}&status=${encodeURIComponent(status)}`, {});
  }

  finalizeOffboarding(id: string): Promise<OffboardingRequest> {
    return this.post<OffboardingRequest>(`lifecycle-onboarding/offboarding/${id}/finalize`, {});
  }

  // ─── Employee Loans & Financial Requests ───
  getEmployeeLoans(employeeId?: string): Promise<EmployeeLoan[]> {
    if (employeeId) {
      return this.get<EmployeeLoan[]>(`employee-loan/loans-by-employee-id?employeeId=${encodeURIComponent(employeeId)}`);
    }
    return this.getList<EmployeeLoan>('employee-loan');
  }

  requestEmployeeLoan(loan: { employeeId: string; loanType: string; principalAmount: number; totalInstallments: number; startDeductionPeriod: string; purpose?: string }): Promise<EmployeeLoan> {
    return this.post<EmployeeLoan>('employee-loan', loan);
  }

  approveEmployeeLoan(loanId: string, approvedBy: string = 'Finance Director'): Promise<EmployeeLoan> {
    return this.post<EmployeeLoan>(`employee-loan/${loanId}/approve?approvedBy=${encodeURIComponent(approvedBy)}`, {});
  }

  rejectEmployeeLoan(loanId: string, reason: string): Promise<EmployeeLoan> {
    return this.post<EmployeeLoan>(`employee-loan/${loanId}/reject?reason=${encodeURIComponent(reason)}`, {});
  }

  // ─── Performance & KPIs ───
  getPerformanceReviews(employeeId?: string): Promise<PerformanceReview[]> {
    const url = employeeId ? `performance/reviews?employeeId=${encodeURIComponent(employeeId)}` : 'performance/reviews';
    return this.get<PerformanceReview[]>(url);
  }

  createPerformanceReview(input: { employeeId: string; reviewCycle: string; periodStart: string; periodEnd: string; reviewerName?: string }): Promise<PerformanceReview> {
    return this.post<PerformanceReview>('performance/review', input);
  }

  submitPerformanceEvaluation(reviewId: string, input: {
    selfRating: number;
    managerRating: number;
    finalRating?: number;
    goalsAchievedPercentage: number;
    strengths: string;
    areasForImprovement: string;
    promotionRecommended: boolean;
    managerFeedback: string;
  }): Promise<PerformanceReview> {
    return this.post<PerformanceReview>(`performance/review/${reviewId}/evaluation`, input);
  }

  getPerformanceGoals(employeeId?: string): Promise<PerformanceGoal[]> {
    const url = employeeId ? `performance/goals?employeeId=${encodeURIComponent(employeeId)}` : 'performance/goals';
    return this.get<PerformanceGoal[]>(url);
  }

  createPerformanceGoal(input: {
    employeeId: string;
    title: string;
    description: string;
    category: string;
    weight: number;
    targetValue: number;
    currentValue?: number;
    metricUnit: string;
    dueDate: string;
  }): Promise<PerformanceGoal> {
    return this.post<PerformanceGoal>('performance/goal', input);
  }

  updateGoalProgress(goalId: string, currentValue: number, status: string): Promise<PerformanceGoal> {
    return this.post<PerformanceGoal>(`performance/goal/${goalId}/progress?currentValue=${currentValue}&status=${encodeURIComponent(status)}`, {});
  }

  // ─── Learning & Development (Training) ───
  getTrainingCourses(): Promise<TrainingCourse[]> {
    return this.get<TrainingCourse[]>('training/courses');
  }

  createTrainingCourse(course: Partial<TrainingCourse>): Promise<TrainingCourse> {
    return this.post<TrainingCourse>('training/course', course);
  }

  getTrainingEnrollments(employeeId?: string, courseId?: string): Promise<TrainingEnrollment[]> {
    let url = 'training/enrollments';
    const params: string[] = [];
    if (employeeId) params.push(`employeeId=${encodeURIComponent(employeeId)}`);
    if (courseId) params.push(`courseId=${encodeURIComponent(courseId)}`);
    if (params.length > 0) url += `?${params.join('&')}`;
    return this.get<TrainingEnrollment[]>(url);
  }

  enrollEmployee(input: { courseId: string; employeeId: string }): Promise<TrainingEnrollment> {
    return this.post<TrainingEnrollment>('training/enroll', input);
  }

  completeEnrollment(enrollmentId: string, score: number, feedback: string): Promise<TrainingEnrollment> {
    return this.post<TrainingEnrollment>(`training/enrollment/${enrollmentId}/complete?score=${score}&feedback=${encodeURIComponent(feedback)}`, {});
  }

  getCertifications(employeeId?: string): Promise<EmployeeCertification[]> {
    const url = employeeId ? `training/certifications?employeeId=${encodeURIComponent(employeeId)}` : 'training/certifications';
    return this.get<EmployeeCertification[]>(url);
  }

  addCertification(cert: Partial<EmployeeCertification>): Promise<EmployeeCertification> {
    return this.post<EmployeeCertification>('training/certification', cert);
  }

  // ─── Benefits & Corporate Insurance ───
  getBenefitPlans(): Promise<BenefitPlan[]> {
    return this.get<BenefitPlan[]>('benefit/plans');
  }

  createBenefitPlan(plan: Partial<BenefitPlan>): Promise<BenefitPlan> {
    return this.post<BenefitPlan>('benefit/plan', plan);
  }

  getEmployeeBenefits(employeeId?: string): Promise<EmployeeBenefit[]> {
    const url = employeeId ? `benefit/employee-benefits?employeeId=${encodeURIComponent(employeeId)}` : 'benefit/employee-benefits';
    return this.get<EmployeeBenefit[]>(url);
  }

  enrollEmployeeBenefit(input: { employeeId: string; benefitPlanId: string; coverageAmount: number }): Promise<EmployeeBenefit> {
    return this.post<EmployeeBenefit>('benefit/enroll', input);
  }

  terminateBenefit(benefitId: string): Promise<EmployeeBenefit> {
    return this.post<EmployeeBenefit>(`benefit/${benefitId}/terminate`, {});
  }

  // ─── Work Shifts & Scheduling ───
  getWorkShifts(): Promise<WorkShift[]> {
    return this.get<WorkShift[]>('shift/shifts');
  }

  createWorkShift(shift: Partial<WorkShift>): Promise<WorkShift> {
    return this.post<WorkShift>('shift/shift', shift);
  }

  getShiftAssignments(employeeId?: string): Promise<ShiftAssignment[]> {
    const url = employeeId ? `shift/assignments?employeeId=${encodeURIComponent(employeeId)}` : 'shift/assignments';
    return this.get<ShiftAssignment[]>(url);
  }

  assignShift(input: { employeeId: string; workShiftId: string; startDate: string; endDate?: string; notes?: string }): Promise<ShiftAssignment> {
    return this.post<ShiftAssignment>('shift/assign', input);
  }

  // ─── Self-Service (MSS / ESS) ───
  getManagerTeamSummary(managerName: string): Promise<TeamSummary> {
    return this.get<TeamSummary>(`self-service/team-summary?managerName=${encodeURIComponent(managerName)}`);
  }
}
