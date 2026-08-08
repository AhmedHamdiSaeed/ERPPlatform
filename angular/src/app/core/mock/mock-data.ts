import {
  UserProfile, Employee, Department, AttendanceRecord, LeaveRequest, Candidate,
  Product, Warehouse, StockTransfer, PurchaseOrder, WorkflowDefinition,
  WorkflowTask, WorkflowExecutionLog, NotificationItem, ReportDefinition
} from '../models/erp-models';

export const CURRENT_USER: UserProfile = {
  id: 'usr-001',
  name: 'Ahmed Hamdi',
  email: 'ahmed.hamdi@erpplatform.com',
  role: 'Admin',
  avatar: 'https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=150&auto=format&fit=crop&q=80',
  permissions: ['*']
};

export const MOCK_DEPARTMENTS: Department[] = [
  { id: 'dept-1', name: 'Information Technology', code: 'IT', managerName: 'Ahmed Hamdi', employeeCount: 42, budget: 350000, description: 'Software engineering, infrastructure & IT operations.' },
  { id: 'dept-2', name: 'Human Resources', code: 'HR', managerName: 'Sara Mahmoud', employeeCount: 18, budget: 120000, description: 'Talent acquisition, employee relations, and compliance.' },
  { id: 'dept-3', name: 'Finance & Accounting', code: 'FIN', managerName: 'Khaled Hassan', employeeCount: 24, budget: 200000, description: 'Financial management, payroll, auditing, and tax.' },
  { id: 'dept-4', name: 'Sales & Marketing', code: 'SLS', managerName: 'Nour El-Din', employeeCount: 56, budget: 450000, description: 'Business development, brand promotion, and sales operations.' },
  { id: 'dept-5', name: 'Supply Chain & Logistics', code: 'SCM', managerName: 'Omar Farouk', employeeCount: 65, budget: 600000, description: 'Inventory, procurement, warehouse, and fulfillment.' },
  { id: 'dept-6', name: 'Quality Assurance', code: 'QA', managerName: 'Mona Zaki', employeeCount: 15, budget: 90000, description: 'Product testing, standards compliance, and audit.' },
  { id: 'dept-7', name: 'Customer Support', code: 'CS', managerName: 'Youssef Ali', employeeCount: 25, budget: 110000, description: 'Client support, helpdesk, and account management.' }
];

export const MOCK_EMPLOYEES: Employee[] = [
  { id: 'emp-101', employeeCode: 'EMP-0101', avatar: 'https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=150', name: 'Ahmed Hamdi', email: 'ahmed.hamdi@erpplatform.com', phone: '+20 100 123 4567', departmentId: 'dept-1', departmentName: 'Information Technology', position: 'Senior Software Architect', managerName: 'Executive Director', joiningDate: '2021-03-15', status: 'Active', salary: 14500, location: 'Cairo HQ' },
  { id: 'emp-102', employeeCode: 'EMP-0102', avatar: 'https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=150', name: 'Sara Mahmoud', email: 'sara.mahmoud@erpplatform.com', phone: '+20 101 987 6543', departmentId: 'dept-2', departmentName: 'Human Resources', position: 'HR Manager', managerName: 'VP People', joiningDate: '2020-01-10', status: 'Active', salary: 11200, location: 'Cairo HQ' },
  { id: 'emp-103', employeeCode: 'EMP-0103', avatar: 'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=150', name: 'Khaled Hassan', email: 'khaled.hassan@erpplatform.com', phone: '+20 102 555 4433', departmentId: 'dept-3', departmentName: 'Finance & Accounting', position: 'Chief Accountant', managerName: 'CFO', joiningDate: '2019-06-01', status: 'Active', salary: 13000, location: 'Cairo HQ' },
  { id: 'emp-104', employeeCode: 'EMP-0104', avatar: 'https://images.unsplash.com/photo-1573496359142-b8d87734a5a2?w=150', name: 'Nour El-Din', email: 'nour.eldin@erpplatform.com', phone: '+20 106 333 2211', departmentId: 'dept-4', departmentName: 'Sales & Marketing', position: 'VP of Global Sales', managerName: 'CEO', joiningDate: '2018-11-20', status: 'Active', salary: 16000, location: 'Alexandria Hub' },
  { id: 'emp-105', employeeCode: 'EMP-0105', avatar: 'https://images.unsplash.com/photo-1500648767791-00dcc994a43e?w=150', name: 'Omar Farouk', email: 'omar.farouk@erpplatform.com', phone: '+20 111 888 9900', departmentId: 'dept-5', departmentName: 'Supply Chain & Logistics', position: 'Logistics Lead', managerName: 'COO', joiningDate: '2022-04-12', status: 'Active', salary: 9800, location: 'Central Warehouse' },
  { id: 'emp-106', employeeCode: 'EMP-0106', avatar: 'https://images.unsplash.com/photo-1580489944761-15a19d654956?w=150', name: 'Mona Zaki', email: 'mona.zaki@erpplatform.com', phone: '+20 109 444 7766', departmentId: 'dept-6', departmentName: 'Quality Assurance', position: 'QA Lead', managerName: 'Ahmed Hamdi', joiningDate: '2022-09-01', status: 'Active', salary: 9200, location: 'Cairo HQ' },
  { id: 'emp-107', employeeCode: 'EMP-0107', avatar: 'https://images.unsplash.com/photo-1519085360753-af0119f7cbe7?w=150', name: 'Youssef Ali', email: 'youssef.ali@erpplatform.com', phone: '+20 114 222 8811', departmentId: 'dept-7', departmentName: 'Customer Support', position: 'Support Lead', managerName: 'Sara Mahmoud', joiningDate: '2023-01-15', status: 'On Leave', salary: 7500, location: 'Remote' }
];

export const MOCK_ATTENDANCE: AttendanceRecord[] = [
  { id: 'att-1', employeeId: 'emp-101', employeeName: 'Ahmed Hamdi', avatar: 'https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=150', departmentName: 'IT', date: '2026-08-08', checkIn: '08:45 AM', checkOut: '05:15 PM', workingHours: 8.5, overtimeHours: 0.5, status: 'Present' },
  { id: 'att-2', employeeId: 'emp-102', employeeName: 'Sara Mahmoud', avatar: 'https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=150', departmentName: 'HR', date: '2026-08-08', checkIn: '09:00 AM', checkOut: '05:00 PM', workingHours: 8.0, overtimeHours: 0.0, status: 'Present' },
  { id: 'att-3', employeeId: 'emp-103', employeeName: 'Khaled Hassan', avatar: 'https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=150', departmentName: 'Finance', date: '2026-08-08', checkIn: '09:25 AM', checkOut: '05:30 PM', workingHours: 8.08, overtimeHours: 0.0, status: 'Late' },
  { id: 'att-4', employeeId: 'emp-104', employeeName: 'Nour El-Din', avatar: 'https://images.unsplash.com/photo-1573496359142-b8d87734a5a2?w=150', departmentName: 'Sales', date: '2026-08-08', checkIn: '08:30 AM', checkOut: '06:00 PM', workingHours: 9.5, overtimeHours: 1.5, status: 'Present' },
  { id: 'att-5', employeeId: 'emp-105', employeeName: 'Omar Farouk', avatar: 'https://images.unsplash.com/photo-1500648767791-00dcc994a43e?w=150', departmentName: 'Logistics', date: '2026-08-08', checkIn: '08:00 AM', checkOut: '04:00 PM', workingHours: 8.0, overtimeHours: 0.0, status: 'Remote' },
  { id: 'att-6', employeeId: 'emp-107', employeeName: 'Youssef Ali', avatar: 'https://images.unsplash.com/photo-1519085360753-af0119f7cbe7?w=150', departmentName: 'Support', date: '2026-08-08', checkIn: '-', checkOut: '-', workingHours: 0, overtimeHours: 0, status: 'On Leave' }
];

export const MOCK_LEAVE_REQUESTS: LeaveRequest[] = [
  { id: 'lv-101', employeeId: 'emp-101', employeeName: 'Ahmed Hamdi', avatar: 'https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=150', leaveType: 'Annual', startDate: '2026-08-15', endDate: '2026-08-20', daysCount: 5, reason: 'Summer family vacation', status: 'Pending', appliedDate: '2026-08-07' },
  { id: 'lv-102', employeeId: 'emp-106', employeeName: 'Mona Zaki', avatar: 'https://images.unsplash.com/photo-1580489944761-15a19d654956?w=150', leaveType: 'Sick', startDate: '2026-08-10', endDate: '2026-08-11', daysCount: 2, reason: 'Medical appointment & recovery', status: 'Approved', appliedDate: '2026-08-06' },
  { id: 'lv-103', employeeId: 'emp-107', employeeName: 'Youssef Ali', avatar: 'https://images.unsplash.com/photo-1519085360753-af0119f7cbe7?w=150', leaveType: 'Annual', startDate: '2026-08-01', endDate: '2026-08-10', daysCount: 8, reason: 'Personal leave', status: 'Approved', appliedDate: '2026-07-25' }
];

export const MOCK_CANDIDATES: Candidate[] = [
  { id: 'cand-1', name: 'Tarek Mansour', email: 'tarek@example.com', phone: '+20 100 888 7766', appliedPosition: 'Senior Angular Developer', experienceYears: 6, stage: 'Technical', rating: 4.8, skills: ['Angular', 'TypeScript', 'RxJS', 'PrimeNG', 'Tailwind'], appliedDate: '2026-08-02', notes: 'Excellent architectural skills.' },
  { id: 'cand-2', name: 'Hoda Soliman', email: 'hoda@example.com', phone: '+20 102 999 1122', appliedPosition: '.NET Backend Architect', experienceYears: 8, stage: 'Interview', rating: 4.5, skills: ['C#', 'ABP Framework', 'EF Core', 'SQL Server', 'Docker'], appliedDate: '2026-08-04', notes: 'Strong understanding of DDD.' },
  { id: 'cand-3', name: 'Mahmoud Fawzy', email: 'mahmoud@example.com', phone: '+20 105 777 3344', appliedPosition: 'UI/UX Designer', experienceYears: 4, stage: 'Screening', rating: 4.0, skills: ['Figma', 'Design Systems', 'Tailwind', 'Prototyping'], appliedDate: '2026-08-06' },
  { id: 'cand-4', name: 'Reem El-Sayed', email: 'reem@example.com', phone: '+20 111 444 5566', appliedPosition: 'DevOps Specialist', experienceYears: 5, stage: 'Offer', rating: 4.9, skills: ['Kubernetes', 'CI/CD', 'Azure', 'Terraform'], appliedDate: '2026-07-28', notes: 'Offer letter dispatched.' }
];

export const MOCK_PRODUCTS: Product[] = [
  { id: 'prod-1', sku: 'PRD-ELC-001', name: 'Dell UltraSharp 27" 4K Monitor', category: 'Electronics', price: 650, stock: 45, reorderLevel: 10, unit: 'pcs', warehouseName: 'Main Warehouse', status: 'In Stock', supplierName: 'TechSupply Co.' },
  { id: 'prod-2', sku: 'PRD-ELC-002', name: 'Logitech MX Master 3S Mouse', category: 'Electronics', price: 120, stock: 8, reorderLevel: 15, unit: 'pcs', warehouseName: 'Main Warehouse', status: 'Low Stock', supplierName: 'TechSupply Co.' },
  { id: 'prod-3', sku: 'PRD-OFC-101', name: 'Ergonomic Executive Mesh Chair', category: 'Office Furniture', price: 340, stock: 120, reorderLevel: 20, unit: 'pcs', warehouseName: 'Secondary Warehouse', status: 'In Stock', supplierName: 'FurniCorp Ltd.' },
  { id: 'prod-4', sku: 'PRD-IND-305', name: 'Industrial Hydraulic Pump Motor 5HP', category: 'Industrial Equipment', price: 2100, stock: 4, reorderLevel: 5, unit: 'units', warehouseName: 'Central Logistics Hub', status: 'Low Stock', supplierName: 'HeavyMachinery Inc.' },
  { id: 'prod-5', sku: 'PRD-ELC-009', name: 'Cisco Catalyst 48-Port Switch', category: 'Networking', price: 1850, stock: 0, reorderLevel: 2, unit: 'units', warehouseName: 'Main Warehouse', status: 'Out of Stock', supplierName: 'NetWorks Direct' }
];

export const MOCK_WAREHOUSES: Warehouse[] = [
  { id: 'wh-1', name: 'Main Warehouse', code: 'WH-MAIN', location: 'Cairo Industrial Zone', manager: 'Omar Farouk', totalProductsCount: 1250, totalStockValue: 85400, capacityPercentage: 78 },
  { id: 'wh-2', name: 'Secondary Warehouse', code: 'WH-SEC', location: '6th October Logistics Park', manager: 'Magdy Zaky', totalProductsCount: 750, totalStockValue: 40200, capacityPercentage: 62 },
  { id: 'wh-3', name: 'Central Logistics Hub', code: 'WH-HUB', location: 'Alexandria Port Freeport', manager: 'Karim Nabil', totalProductsCount: 320, totalStockValue: 195000, capacityPercentage: 90 }
];

export const MOCK_STOCK_TRANSFERS: StockTransfer[] = [
  { id: 'st-101', transferCode: 'TRF-2026-001', sourceWarehouse: 'Main Warehouse', destinationWarehouse: 'Secondary Warehouse', productName: 'Logitech MX Master 3S Mouse', quantity: 20, requestedBy: 'Omar Farouk', date: '2026-08-07', status: 'Approved' },
  { id: 'st-102', transferCode: 'TRF-2026-002', sourceWarehouse: 'Central Logistics Hub', destinationWarehouse: 'Main Warehouse', productName: 'Cisco Catalyst 48-Port Switch', quantity: 5, requestedBy: 'Karim Nabil', date: '2026-08-08', status: 'In Transit' }
];

export const MOCK_PURCHASE_ORDERS: PurchaseOrder[] = [
  {
    id: 'po-501',
    poNumber: 'PO-2026-8801',
    supplierName: 'TechSupply Co.',
    orderDate: '2026-08-05',
    deliveryDate: '2026-08-12',
    items: [
      { productName: 'Dell UltraSharp 27" 4K Monitor', quantity: 20, unitPrice: 600, totalPrice: 12000 },
      { productName: 'Logitech MX Master 3S Mouse', quantity: 50, unitPrice: 100, totalPrice: 5000 }
    ],
    subtotal: 17000,
    tax: 2380,
    discount: 500,
    grandTotal: 18880,
    createdBy: 'Omar Farouk',
    status: 'Pending Approval'
  },
  {
    id: 'po-502',
    poNumber: 'PO-2026-8802',
    supplierName: 'HeavyMachinery Inc.',
    orderDate: '2026-08-01',
    deliveryDate: '2026-08-15',
    items: [
      { productName: 'Industrial Hydraulic Pump Motor 5HP', quantity: 10, unitPrice: 2000, totalPrice: 20000 }
    ],
    subtotal: 20000,
    tax: 2800,
    discount: 1000,
    grandTotal: 21800,
    createdBy: 'Omar Farouk',
    status: 'Approved'
  }
];

export const MOCK_WORKFLOWS: WorkflowDefinition[] = [
  {
    id: 'wf-1',
    name: 'Employee Leave Approval Workflow',
    description: 'Automated multi-level approval process for annual & sick leave requests.',
    version: 'v2.1',
    triggerType: 'Entity Created',
    status: 'Published',
    createdBy: 'Ahmed Hamdi',
    createdDate: '2026-01-15',
    nodes: [
      { id: 'n-1', type: 'trigger', title: 'Leave Requested', subtitle: 'On Leave Created', x: 100, y: 150 },
      { id: 'n-2', type: 'condition', title: 'Check Leave Days', subtitle: 'Days > 5', x: 320, y: 150 },
      { id: 'n-3', type: 'approval', title: 'Manager Approval', subtitle: 'Direct Line Manager', x: 550, y: 80 },
      { id: 'n-4', type: 'approval', title: 'HR + Director Approval', subtitle: 'Higher Level Review', x: 550, y: 240 },
      { id: 'n-5', type: 'notification', title: 'Send Email & SMS', subtitle: 'Notify Employee', x: 800, y: 150 },
      { id: 'n-6', type: 'end', title: 'Process Completed', subtitle: 'Status updated', x: 1020, y: 150 }
    ],
    connections: [
      { id: 'c-1', sourceId: 'n-1', targetId: 'n-2' },
      { id: 'c-2', sourceId: 'n-2', targetId: 'n-3', label: 'No' },
      { id: 'c-3', sourceId: 'n-2', targetId: 'n-4', label: 'Yes' },
      { id: 'c-4', sourceId: 'n-3', targetId: 'n-5' },
      { id: 'c-5', sourceId: 'n-4', targetId: 'n-5' },
      { id: 'c-6', sourceId: 'n-5', targetId: 'n-6' }
    ]
  },
  {
    id: 'wf-2',
    name: 'High Value Purchase Order Approval',
    description: 'Routes POs above $5,000 to CFO and Procurement Committee.',
    version: 'v1.0',
    triggerType: 'Entity Created',
    status: 'Published',
    createdBy: 'Khaled Hassan',
    createdDate: '2026-03-10',
    nodes: [
      { id: 'n-10', type: 'trigger', title: 'PO Created', subtitle: 'Purchase Order Trigger', x: 100, y: 150 },
      { id: 'n-11', type: 'condition', title: 'Total > $5,000', subtitle: 'Amount Check', x: 320, y: 150 },
      { id: 'n-12', type: 'approval', title: 'CFO Approval', subtitle: 'Financial Sign-off', x: 560, y: 150 },
      { id: 'n-13', type: 'end', title: 'Execute Order', subtitle: 'Send to Supplier', x: 800, y: 150 }
    ],
    connections: [
      { id: 'c-10', sourceId: 'n-10', targetId: 'n-11' },
      { id: 'c-11', sourceId: 'n-11', targetId: 'n-12', label: 'Yes' },
      { id: 'c-12', sourceId: 'n-12', targetId: 'n-13' }
    ]
  }
];

export const MOCK_TASKS: WorkflowTask[] = [
  { id: 'task-1', taskNumber: 'TSK-9901', workflowName: 'Employee Leave Approval Workflow', requestedBy: 'Ahmed Hamdi', requestedByAvatar: 'https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=150', type: 'Leave Request', details: 'Annual Leave request for 5 days (Aug 15 - Aug 20)', createdDate: '2026-08-07', status: 'Waiting Approval' },
  { id: 'task-2', taskNumber: 'TSK-9902', workflowName: 'High Value Purchase Order Approval', requestedBy: 'Omar Farouk', requestedByAvatar: 'https://images.unsplash.com/photo-1500648767791-00dcc994a43e?w=150', type: 'Purchase Order', details: 'PO-2026-8801 for TechSupply Co. ($18,880)', createdDate: '2026-08-05', status: 'Waiting Approval' },
  { id: 'task-3', taskNumber: 'TSK-9903', workflowName: 'Stock Transfer Verification', requestedBy: 'Karim Nabil', requestedByAvatar: 'https://images.unsplash.com/photo-1519085360753-af0119f7cbe7?w=150', type: 'Stock Transfer', details: 'TRF-2026-002 Transfer from Central Hub to Main WH', createdDate: '2026-08-08', status: 'Waiting Approval' }
];

export const MOCK_EXECUTION_LOGS: WorkflowExecutionLog[] = [
  {
    id: 'log-1',
    executionCode: 'EXEC-2026-9041',
    workflowName: 'Employee Leave Approval Workflow',
    triggeredBy: 'Ahmed Hamdi',
    startTime: '2026-08-07 10:30 AM',
    duration: '4m 12s',
    status: 'Completed',
    steps: [
      { stepName: 'Leave Requested Trigger', timestamp: '10:30 AM', status: 'Passed', details: 'Entity LeaveRequest #lv-101 created' },
      { stepName: 'Check Leave Days Condition', timestamp: '10:30 AM', status: 'Passed', details: 'Evaluated: Days (5) <= 5 -> Single Approval' },
      { stepName: 'Manager Approval', timestamp: '10:32 AM', status: 'Passed', details: 'Approved by Sara Mahmoud' },
      { stepName: 'Send Email Notification', timestamp: '10:34 AM', status: 'Passed', details: 'Email dispatched to ahmed.hamdi@erpplatform.com' }
    ]
  },
  {
    id: 'log-2',
    executionCode: 'EXEC-2026-9042',
    workflowName: 'High Value Purchase Order Approval',
    triggeredBy: 'Omar Farouk',
    startTime: '2026-08-05 02:15 PM',
    duration: '1h 10m',
    status: 'Running',
    steps: [
      { stepName: 'PO Created Trigger', timestamp: '02:15 PM', status: 'Passed', details: 'Purchase Order #po-501 created' },
      { stepName: 'Amount Condition', timestamp: '02:15 PM', status: 'Passed', details: '$18,880 > $5,000 threshold' },
      { stepName: 'CFO Approval', timestamp: '02:16 PM', status: 'Running', details: 'Waiting for CFO signature' }
    ]
  }
];

export const MOCK_NOTIFICATIONS: NotificationItem[] = [
  { id: 'notif-1', type: 'Workflow Approval', title: 'New Approval Pending', message: 'Leave Request #lv-101 requires your approval.', timestamp: '10 mins ago', read: false, link: '/workflow/tasks' },
  { id: 'notif-2', type: 'Inventory', title: 'Low Stock Alert', message: 'Logitech MX Master 3S Mouse stock is below reorder level (8 left).', timestamp: '45 mins ago', read: false, link: '/inventory/products' },
  { id: 'notif-3', type: 'AI', title: 'AI Recommendation', message: 'Inventory value decreased by 4.5%. View AI analysis on dashboard.', timestamp: '2 hours ago', read: true, link: '/dashboard' },
  { id: 'notif-4', type: 'HR', title: 'New Job Application', message: 'Tarek Mansour applied for Senior Angular Developer.', timestamp: '1 day ago', read: true, link: '/hr/recruitment' }
];

export const MOCK_REPORTS: ReportDefinition[] = [
  { id: 'rep-1', title: 'Monthly Workforce Analytics & Headcount', category: 'HR', description: 'Comprehensive breakdown of employee headcount, attrition, and department distribution.', lastGenerated: '2026-08-01', recordCount: 245 },
  { id: 'rep-2', title: 'Inventory Valuation & Stock Turnover Rate', category: 'Inventory', description: 'Valuation of stock per warehouse, turnover ratios, and slow-moving items.', lastGenerated: '2026-08-07', recordCount: 1540 },
  { id: 'rep-3', title: 'Workflow SLA & Bottleneck Analysis', category: 'Workflow', description: 'Average approval times, step durations, and execution failure rates.', lastGenerated: '2026-08-05', recordCount: 380 },
  { id: 'rep-4', title: 'Quarterly Procurement & Supplier Expenses', category: 'Financial', description: 'Total purchase order expenditures, supplier performance, and savings.', lastGenerated: '2026-07-31', recordCount: 88 }
];
