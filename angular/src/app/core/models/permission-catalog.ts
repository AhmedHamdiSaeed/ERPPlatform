/**
 * Module -> Page catalogue used by the Roles & Permissions screen.
 *
 * The backend defines a single flat permission group, so the module structure lives here
 * (frontend grouping). Page keys are the identifiers stored in RolePageScopes.PageKey and
 * must stay in sync with DataScopePageKeys on the server.
 */

export enum DataScopeType {
  /** No row restriction - sees every record. */
  All = 0,
  /** Records belonging to the current user's department. */
  MyDepartment = 1,
  /** Records belonging to the current user's branch. */
  MyBranch = 2,
  /** Records in the explicitly selected branches. */
  SpecificBranch = 3,
  /** Records in the explicitly selected departments. */
  SpecificDepartment = 4,
  /** The explicitly selected employee records. */
  SpecificEmployees = 5
}

export interface ScopeOption {
  value: DataScopeType;
  label: string;
  /** When true the admin must pick concrete branches / departments / employees. */
  requiresTargets: boolean;
}

export const SCOPE_OPTIONS: ScopeOption[] = [
  { value: DataScopeType.All, label: 'All employees', requiresTargets: false },
  { value: DataScopeType.MyDepartment, label: 'My department', requiresTargets: false },
  { value: DataScopeType.MyBranch, label: 'My branch', requiresTargets: false },
  { value: DataScopeType.SpecificBranch, label: 'Specific branch', requiresTargets: true },
  { value: DataScopeType.SpecificDepartment, label: 'Specific department', requiresTargets: true },
  { value: DataScopeType.SpecificEmployees, label: 'Specific employees', requiresTargets: true }
];

export interface PageDefinition {
  /** Permission / scope key, e.g. "ERPPlatform.Employees". */
  key: string;
  name: string;
  /** People-data pages expose a data scope selector. */
  supportsScope: boolean;
}

export interface ModuleDefinition {
  key: string;
  name: string;
  icon: string;
  pages: PageDefinition[];
}

const scoped = (key: string, name: string): PageDefinition => ({ key, name, supportsScope: true });
// Every page exposes a data-scope selector (All / My department / My branch / specific
// branch·department·employees). The scope is generic row-filtering data stored per role+page.
const page = (key: string, name: string): PageDefinition => ({ key, name, supportsScope: true });

export const PERMISSION_MODULES: ModuleDefinition[] = [
  {
    key: 'hr',
    name: 'Human Resources',
    icon: 'pi pi-users',
    pages: [
      scoped('ERPPlatform.Employees', 'Employees'),
      scoped('ERPPlatform.Attendance', 'Attendance'),
      scoped('ERPPlatform.LeaveRequests', 'Leave Requests'),
      scoped('ERPPlatform.Payroll', 'Payroll'),
      scoped('ERPPlatform.Payslips', 'Payslips'),
      page('ERPPlatform.Departments', 'Departments'),
      page('ERPPlatform.Positions', 'Positions'),
      page('ERPPlatform.Contracts', 'Contracts'),
      page('ERPPlatform.Recruitment', 'Recruitment')
    ]
  },
  {
    key: 'sales',
    name: 'Sales & CRM',
    icon: 'pi pi-shopping-cart',
    pages: [
      page('ERPPlatform.Customers', 'Customers'),
      page('ERPPlatform.Leads', 'Leads'),
      page('ERPPlatform.Deals', 'Deals / Pipeline'),
      page('ERPPlatform.SalesOrders', 'Sales Orders'),
      page('ERPPlatform.SalesQuotations', 'Quotations'),
      page('ERPPlatform.Invoices', 'Sales Invoices'),
      page('ERPPlatform.DeliveryNotes', 'Delivery Notes'),
      page('ERPPlatform.Payments', 'Payments')
    ]
  },
  {
    key: 'inventory',
    name: 'Inventory & Purchasing',
    icon: 'pi pi-box',
    pages: [
      page('ERPPlatform.Products', 'Products'),
      page('ERPPlatform.Warehouses', 'Warehouses'),
      page('ERPPlatform.StockTransfers', 'Stock Transfers'),
      page('ERPPlatform.PurchaseOrders', 'Purchase Orders'),
      page('ERPPlatform.PurchaseRequests', 'Purchase Requests'),
      page('ERPPlatform.GoodsReceipts', 'Goods Receipts'),
      page('ERPPlatform.Suppliers', 'Suppliers')
    ]
  },
  {
    key: 'finance',
    name: 'Finance & Accounting',
    icon: 'pi pi-wallet',
    pages: [
      page('ERPPlatform.Accounts', 'Chart of Accounts'),
      page('ERPPlatform.JournalEntries', 'Journal Entries'),
      page('ERPPlatform.ExpenseRequests', 'Expenses')
    ]
  },
  {
    key: 'operations',
    name: 'Projects & Operations',
    icon: 'pi pi-sitemap',
    pages: [
      page('ERPPlatform.Projects', 'Projects'),
      page('ERPPlatform.ManufacturingOrders', 'Manufacturing'),
      page('ERPPlatform.FixedAssets', 'Fixed Assets'),
      page('ERPPlatform.MaintenanceRequests', 'Maintenance')
    ]
  },
  {
    key: 'workflow',
    name: 'Workflow',
    icon: 'pi pi-sync',
    pages: [
      page('ERPPlatform.WorkflowDefinitions', 'Designer'),
      page('ERPPlatform.WorkflowTasks', 'Tasks'),
      page('ERPPlatform.ApprovalCenter', 'Approval Center')
    ]
  },
  {
    key: 'reports',
    name: 'Reports & Documents',
    icon: 'pi pi-chart-bar',
    pages: [
      page('ERPPlatform.ReportDefinitions', 'Report Center'),
      page('ERPPlatform.Documents', 'Documents')
    ]
  },
  {
    key: 'system',
    name: 'System & Settings',
    icon: 'pi pi-cog',
    pages: [
      page('ERPPlatform.Dashboard', 'Dashboard'),
      page('ERPPlatform.Users', 'Users'),
      page('ERPPlatform.Roles', 'Roles'),
      page('ERPPlatform.Companies', 'Company'),
      page('ERPPlatform.Branches', 'Branches'),
      page('ERPPlatform.Currencies', 'Currencies & Tax'),
      page('ERPPlatform.IntegrationConfigs', 'Integrations'),
      page('ERPPlatform.Settings', 'Settings')
    ]
  }
];

/** Flat list of every page that exposes a data scope, for convenience. */
export const SCOPED_PAGES: PageDefinition[] = PERMISSION_MODULES
  .reduce<PageDefinition[]>((acc, m) => acc.concat(m.pages), [])
  .filter(p => p.supportsScope);
