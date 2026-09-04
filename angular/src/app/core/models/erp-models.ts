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
  departmentId: string;
  departmentName: string;
  position: string;
  managerName?: string;
  joiningDate: string;
  status: 'Active' | 'Inactive' | 'On Leave';
  salary?: number;
  location?: string;
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

export interface Candidate {
  id: string;
  name: string;
  email: string;
  phone: string;
  appliedPosition: string;
  experienceYears: number;
  stage: 'Applied' | 'Screening' | 'Interview' | 'Technical' | 'Offer' | 'Hired';
  rating: number;
  cvUrl?: string;
  skills: string[];
  appliedDate: string;
  notes?: string;
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
