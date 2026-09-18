export interface UserProfile {
  id: string;
  name: string;
  email: string;
  role: 'Admin' | 'HR Manager' | 'Inventory Manager' | 'Employee';
  avatar: string;
  permissions: string[];
  tenantId?: string;
  tenantName?: string;
  tenantLogo?: string;
}

export interface Employee {
  id: string;
  employeeCode: string;
  avatar: string;
  name: string;
  email: string;
  phone: string;
  departmentId?: string;
  departmentName: string;
  branchId?: string;
  branchName?: string;
  position: string;
  managerName?: string;
  joiningDate: string;
  status: 'Active' | 'Inactive' | 'On Leave' | 'Suspended' | 'Terminated';
  salary?: number;
  location?: string;
  leaveBalance?: number;

  // Master Data Fields
  nationalId?: string;
  passportNumber?: string;
  nationality?: string;
  dateOfBirth?: string;
  gender?: 'Male' | 'Female';
  maritalStatus?: 'Single' | 'Married' | 'Divorced' | 'Widowed';
  emergencyContactName?: string;
  emergencyContactPhone?: string;
  emergencyContactRelation?: string;
  bankName?: string;
  bankAccountNumber?: string;
  iban?: string;
  swiftCode?: string;
  employmentType?: 'FullTime' | 'PartTime' | 'Contractor' | 'Intern' | 'Remote';
  probationEndDate?: string;
  contractEndDate?: string;
  jobGradeId?: string;
  jobGradeName?: string;
  costCenterCode?: string;
  noticePeriodDays?: number;
}

export interface EmployeeContract {
  id: string;
  contractNumber: string;
  employeeId: string;
  employeeName: string;
  contractType: 'Permanent' | 'FixedTerm' | 'Probation' | 'Contractor';
  startDate: string;
  endDate?: string;
  probationEndDate?: string;
  basicSalary: number;
  housingAllowance: number;
  transportationAllowance: number;
  otherAllowances: number;
  totalGrossSalary: number;
  workingHoursPerWeek: number;
  noticePeriodDays: number;
  status: 'Active' | 'Expired' | 'Terminated' | 'Draft' | 'Renewed';
  signedAt?: string;
  signedDocumentUrl?: string;
  notes?: string;
  daysUntilExpiration?: number;
}

export interface HrAction {
  id: string;
  actionCode: string;
  employeeId: string;
  employeeName: string;
  actionType: 'Hire' | 'Promotion' | 'Transfer' | 'SalaryChange' | 'DepartmentChange' | 'ManagerChange' | 'Suspension' | 'Termination' | 'Resignation';
  effectiveDate: string;
  status: 'Pending' | 'Approved' | 'Rejected' | 'Implemented';
  previousValuesJson: string;
  newValuesJson: string;
  requestedBy: string;
  approvedBy?: string;
  approvalDate?: string;
  remarks?: string;
  creationTime: string;
}

export interface EmployeeDocument {
  id: string;
  employeeId: string;
  employeeName: string;
  documentType: 'NationalId' | 'Passport' | 'Contract' | 'Medical' | 'Certificate' | 'Degree' | 'Tax' | 'Visa' | 'Warning' | 'Other';
  documentTitle: string;
  documentNumber?: string;
  fileUrl: string;
  fileName: string;
  fileSize: number;
  issueDate?: string;
  expiryDate?: string;
  isVerified: boolean;
  verifiedBy?: string;
  verificationDate?: string;
  notes?: string;
  isExpired?: boolean;
  daysUntilExpiration?: number;
}

export interface JobGrade {
  id: string;
  gradeCode: string;
  gradeName: string;
  level: string;
  minSalary: number;
  maxSalary: number;
  description: string;
  isActive: boolean;
}

export interface JobPosition {
  id: string;
  code: string;
  title: string;
  departmentId?: string;
  departmentName?: string;
  jobGradeId?: string;
  jobGradeName?: string;
  description: string;
  requirements: string;
  minSalary: number;
  maxSalary: number;
  isActive: boolean;
}

export interface Department {
  id: string;
  name: string;
  code: string;
  managerName: string;
  employeeCount: number;
  budget: number;
  description: string;
}

export interface AttendanceRecord {
  id: string;
  employeeId: string;
  employeeName: string;
  avatar: string;
  departmentName: string;
  date: string;
  checkIn: string;
  checkOut: string;
  workingHours: number;
  overtimeHours: number;
  status: 'Present' | 'Absent' | 'Late' | 'On Leave' | 'Remote';
}

export interface LeaveRequest {
  id: string;
  employeeId: string;
  employeeName: string;
  avatar: string;
  leaveType: 'Annual' | 'Sick' | 'Maternity' | 'Unpaid' | 'Casual';
  startDate: string;
  endDate: string;
  daysCount: number;
  reason: string;
  attachmentName?: string;
  status: 'Pending' | 'Approved' | 'Rejected';
  appliedDate: string;
}

export interface JobRequisition {
  id: string;
  requisitionCode: string;
  title: string;
  departmentId?: string;
  departmentName: string;
  vacanciesCount: number;
  employmentType: string;
  minSalary: number;
  maxSalary: number;
  experienceLevel: string;
  jobDescription: string;
  requirements: string;
  hiringManager: string;
  status: 'Draft' | 'Open' | 'InProgress' | 'Filled' | 'Cancelled';
  targetStartDate: string;
  candidatesCount?: number;
}

export interface Candidate {
  id: string;
  name: string;
  email: string;
  phone: string;
  appliedPosition: string;
  jobRequisitionId?: string;
  experienceYears: number;
  stage: 'Applied' | 'Screening' | 'Interview' | 'Technical' | 'Offer' | 'Hired' | 'Rejected';
  rating: number;
  cvUrl?: string;
  skills?: string[];
  skillsJson?: string;
  appliedDate: string;
  expectedSalary?: string;
  noticePeriod?: string;
  notes?: string;
  convertedEmployeeId?: string;
}

export interface Interview {
  id: string;
  candidateId: string;
  candidateName: string;
  interviewType: 'Screening' | 'Technical' | 'HR' | 'Managerial' | 'Final';
  scheduledTime: string;
  interviewerName: string;
  meetingLink?: string;
  score: number;
  status: 'Scheduled' | 'Completed' | 'Cancelled' | 'Rescheduled';
  recommendation: 'Advance' | 'StrongHire' | 'Hire' | 'Hold' | 'Reject' | 'Pending';
  feedbackNotes?: string;
}

export interface OfferLetter {
  id: string;
  offerCode: string;
  candidateId: string;
  candidateName: string;
  positionTitle: string;
  departmentName: string;
  offeredBasicSalary: number;
  housingAllowance: number;
  transportAllowance: number;
  totalMonthlyPackage: number;
  proposedStartDate: string;
  expiryDate: string;
  status: 'Draft' | 'Sent' | 'Accepted' | 'Declined' | 'Expired';
  acceptedAt?: string;
  signedDocumentUrl?: string;
  notes?: string;
}

export interface OnboardingTask {
  id: string;
  employeeId: string;
  employeeName: string;
  title: string;
  category: 'IT' | 'HR' | 'Finance' | 'Admin' | 'Department';
  assignedTo: string;
  dueDate: string;
  status: 'Pending' | 'InProgress' | 'Completed' | 'Blocked';
  completedAt?: string;
  notes?: string;
}

export interface OffboardingRequest {
  id: string;
  requestNumber: string;
  employeeId: string;
  employeeName: string;
  departmentName: string;
  resignationDate: string;
  lastWorkingDay: string;
  reason: string;
  status: 'Submitted' | 'ClearanceInProgress' | 'ReadyForFinalSettlement' | 'FinanceApproved' | 'Finalized' | 'Cancelled';
  itClearanceStatus: 'Pending' | 'Cleared';
  adminClearanceStatus: 'Pending' | 'Cleared';
  financeClearanceStatus: 'Pending' | 'Cleared';
  outstandingLoanBalance: number;
  accruedLeavePayout: number;
  endOfServiceGratuity: number;
  netFinalSettlement: number;
  exitInterviewNotes?: string;
}

export interface EmployeeLoan {
  id: string;
  loanNumber: string;
  employeeId: string;
  employeeName: string;
  loanType: 'Personal' | 'Advance' | 'Emergency' | 'Housing';
  principalAmount: number;
  totalInstallments: number;
  monthlyInstallment: number;
  totalPaidAmount: number;
  remainingBalance: number;
  startDeductionPeriod: string;
  status: 'PendingApproval' | 'Approved' | 'Active' | 'FullyRepaid' | 'Rejected';
  approvedBy?: string;
  purpose?: string;
  installments?: LoanInstallment[];
}

export interface LoanInstallment {
  id: string;
  loanId: string;
  employeeId: string;
  period: string;
  installmentNumber: number;
  amount: number;
  dueDate: string;
  isDeducted: boolean;
  deductedAt?: string;
}

export interface Product {
  id: string;
  sku: string;
  name: string;
  category: string;
  price: number;
  stock: number;
  reorderLevel: number;
  unit: string;
  warehouseName: string;
  status: 'In Stock' | 'Low Stock' | 'Out of Stock';
  supplierName: string;
}

export interface Warehouse {
  id: string;
  name: string;
  code: string;
  location: string;
  manager: string;
  totalProductsCount: number;
  totalStockValue: number;
  capacityPercentage: number;
}

export interface StockTransfer {
  id: string;
  transferCode: string;
  sourceWarehouse: string;
  destinationWarehouse: string;
  productName: string;
  quantity: number;
  requestedBy: string;
  date: string;
  status: 'Draft' | 'Pending Approval' | 'Approved' | 'In Transit' | 'Completed' | 'Rejected';
}

export interface PurchaseOrderItem {
  productName: string;
  quantity: number;
  unitPrice: number;
  totalPrice: number;
}

export interface PurchaseOrder {
  id: string;
  poNumber: string;
  supplierName: string;
  orderDate: string;
  deliveryDate: string;
  items: PurchaseOrderItem[];
  subtotal: number;
  tax: number;
  discount: number;
  grandTotal: number;
  createdBy: string;
  status: 'Draft' | 'Pending Approval' | 'Approved' | 'Received' | 'Cancelled';
}

export type WorkflowNodeType = 
  | 'trigger' 
  | 'action' 
  | 'condition' 
  | 'approval' 
  | 'notification' 
  | 'delay' 
  | 'webhook' 
  | 'api_request' 
  | 'ai' 
  | 'end';

export interface WorkflowNode {
  id: string;
  type: WorkflowNodeType;
  title: string;
  subtitle?: string;
  x: number;
  y: number;
  config?: Record<string, any>;
}

export interface WorkflowConnection {
  id: string;
  sourceId: string;
  targetId: string;
  label?: string;
}

export interface WorkflowDefinition {
  id: string;
  name: string;
  description: string;
  version: string;
  triggerType: 'Manual' | 'Schedule' | 'Entity Created' | 'Entity Updated' | 'Webhook';
  status: 'Published' | 'Draft' | 'Archived';
  createdBy: string;
  createdDate: string;
  nodes: WorkflowNode[];
  connections: WorkflowConnection[];
}

export interface WorkflowTask {
  id: string;
  taskNumber: string;
  workflowName: string;
  requestedBy: string;
  requestedByAvatar: string;
  type: 'Leave Request' | 'Purchase Order' | 'Stock Transfer' | 'Expense Approval';
  details: string;
  createdDate: string;
  status: 'Waiting Approval' | 'Approved' | 'Rejected' | 'Changes Requested';
  comments?: string[];
}

export interface WorkflowExecutionLog {
  id: string;
  executionCode: string;
  workflowName: string;
  triggeredBy: string;
  startTime: string;
  duration: string;
  status: 'Running' | 'Completed' | 'Failed' | 'Cancelled';
  steps: {
    stepName: string;
    timestamp: string;
    status: 'Passed' | 'Failed' | 'Running' | 'Skipped';
    details?: string;
  }[];
}

export interface NotificationItem {
  id: string;
  type: 'Workflow Approval' | 'System' | 'HR' | 'Inventory' | 'AI' | 'Security';
  title: string;
  message: string;
  timestamp: string;
  read: boolean;
  link?: string;
}

export interface ReportDefinition {
  id: string;
  title: string;
  category: 'HR' | 'Inventory' | 'Workflow' | 'Financial' | 'System';
  description: string;
  lastGenerated: string;
  recordCount: number;
}

// Finance & Accounting Models
export interface Account {
  id: string;
  code: string;
  name: string;
  type: 'Asset' | 'Liability' | 'Equity' | 'Revenue' | 'Expense';
  balance: number;
  currency: string;
  parentCode?: string;
  isActive: boolean;
}

export interface JournalEntry {
  id: string;
  entryNumber: string;
  entryDate: string;
  description: string;
  totalDebit: number;
  totalCredit: number;
  status: 'Draft' | 'Posted';
  createdBy: string;
}

// Payroll Models
export interface PayrollRun {
  id: string;
  period: string;
  totalEmployees: number;
  totalGrossSalary: number;
  totalDeductions: number;
  totalNetSalary: number;
  status: 'Draft' | 'Processed' | 'Approved';
  processedDate: string;
}

export interface Payslip {
  id: string;
  employeeId: string;
  employeeName: string;
  period: string;
  baseSalary: number;
  allowances: number;
  deductions: number;
  netSalary: number;
  status: 'Paid' | 'Pending';
}

// CRM Models
export interface Deal {
  id: string;
  title: string;
  customerName: string;
  value: number;
  stage: 'Prospecting' | 'Qualification' | 'Proposal' | 'Negotiation' | 'Closed Won' | 'Closed Lost';
  probability: number;
  expectedCloseDate: string;
  ownerName: string;
}

// Audit Trail Model
export interface AuditLogEntry {
  id: string;
  entityName: string;
  entityId: string;
  action: 'Created' | 'Updated' | 'Deleted';
  userName: string;
  timestamp: string;
  changesJson: string;
}

// ─── Performance & KPIs Models ───
export interface PerformanceReview {
  id: string;
  employeeId: string;
  employeeName: string;
  reviewCycle: string;
  periodStart: string;
  periodEnd: string;
  reviewerName: string;
  selfRating: number;
  managerRating: number;
  finalRating: number;
  status: 'Draft' | 'SelfAssessment' | 'ManagerReview' | 'Completed';
  goalsAchievedPercentage: number;
  strengths: string;
  areasForImprovement: string;
  promotionRecommended: boolean;
  managerFeedback: string;
  completedAt?: string;
}

export interface PerformanceGoal {
  id: string;
  employeeId: string;
  employeeName: string;
  title: string;
  description: string;
  category: 'Strategic' | 'Operational' | 'Learning' | 'KPI';
  weight: number;
  targetValue: number;
  currentValue: number;
  metricUnit: string;
  dueDate: string;
  status: 'NotStarted' | 'InProgress' | 'Achieved' | 'Behind';
  score?: number;
}

// ─── Learning & Development (L&D) Models ───
export interface TrainingCourse {
  id: string;
  courseCode: string;
  title: string;
  description: string;
  category: 'Technical' | 'Compliance' | 'Leadership' | 'SoftSkills';
  trainerName: string;
  durationHours: number;
  costPerAttendee: number;
  maxAttendees: number;
  deliveryMethod: 'Online' | 'Classroom' | 'Hybrid';
  status: 'Active' | 'Upcoming' | 'Completed' | 'Archived';
  passingScore: number;
}

export interface TrainingEnrollment {
  id: string;
  courseId: string;
  courseTitle: string;
  employeeId: string;
  employeeName: string;
  enrollmentDate: string;
  completionDate?: string;
  status: 'Enrolled' | 'InProgress' | 'Completed' | 'Failed' | 'Cancelled';
  score: number;
  certificateIssued: boolean;
  feedback?: string;
}

export interface EmployeeCertification {
  id: string;
  employeeId: string;
  employeeName: string;
  certificationName: string;
  issuingOrganization: string;
  issueDate: string;
  expiryDate?: string;
  credentialId: string;
  certificateUrl?: string;
  status: 'Active' | 'ExpiringSoon' | 'Expired';
}

// ─── Benefits & Corporate Insurance Models ───
export interface BenefitPlan {
  id: string;
  planCode: string;
  planName: string;
  category: 'MedicalInsurance' | 'LifeInsurance' | 'Retirement' | 'GymWellness' | 'Allowance';
  providerName: string;
  coverageDetails: string;
  employerContributionMonthly: number;
  employeeContributionMonthly: number;
  isActive: boolean;
}

export interface EmployeeBenefit {
  id: string;
  employeeId: string;
  employeeName: string;
  benefitPlanId: string;
  planName: string;
  category: string;
  enrollmentDate: string;
  coverageAmount: number;
  employerContribution: number;
  employeeDeduction: number;
  status: 'Active' | 'Terminated' | 'Suspended';
}

// ─── Work Shifts Models ───
export interface WorkShift {
  id: string;
  shiftCode: string;
  shiftName: string;
  startTime: string;
  endTime: string;
  breakMinutes: number;
  isNightShift: boolean;
  isActive: boolean;
}

export interface ShiftAssignment {
  id: string;
  employeeId: string;
  employeeName: string;
  workShiftId: string;
  shiftName: string;
  startDate: string;
  endDate?: string;
  notes?: string;
}

// ─── Self-Service (MSS / ESS) Models ───
export interface TeamSummary {
  totalDirectReports: number;
  presentToday: number;
  onLeaveToday: number;
  pendingLeaveApprovals: number;
  pendingActionApprovals: number;
  teamMembers: Employee[];
}

