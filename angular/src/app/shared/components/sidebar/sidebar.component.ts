import { Component, inject } from '@angular/core';
import { RouterModule } from '@angular/router';
import { LocalizationPipe } from '@abp/ng.core';
import { StateService } from '../../../core/services/state.service';
import { PERMISSIONS } from '../../../core/models/permissions';

interface NavItem {
  label: string;
  key: string;
  link: string;
  icon: string;
  badge?: string;
  permission?: string;
}

interface NavGroup {
  label: string;
  key: string;
  icon: string;
  expanded?: boolean;
  permission?: string;
  items?: NavItem[];
  link?: string;
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [RouterModule, LocalizationPipe],
  templateUrl: './sidebar.component.html'
})
export class SidebarComponent {
  state = inject(StateService);

  menu: NavGroup[] = [
    { label: 'Executive Dashboard', key: 'Menu:ExecutiveDashboard', icon: 'pi-home', link: '/dashboard', permission: PERMISSIONS.DashboardView },
    {
      label: 'SaaS & Subscriptions', key: 'Menu:SaasSubscriptions',
      icon: 'pi-star',
      expanded: true,
      items: [
        { label: 'Tenant Subscription', key: 'Menu:TenantSubscription', link: '/saas/subscription', icon: 'pi-credit-card', badge: 'Tier' },
        { label: 'Feature Usage & Limits', key: 'Menu:FeatureUsageLimits', link: '/saas/usage', icon: 'pi-chart-pie', badge: 'Limits' },
        { label: 'Admin SaaS Plans', key: 'Menu:AdminSaasPlans', link: '/saas/plans', icon: 'pi-sliders-h', badge: 'Admin' }
      ]
    },
    {
      label: 'CRM & Customers', key: 'Menu:CrmCustomers',
      icon: 'pi-address-book',
      expanded: true,
      items: [
        { label: 'CRM Leads', key: 'Menu:CrmLeads', link: '/sales/crm/leads', icon: 'pi-user-plus', badge: 'CRM' },
        { label: 'Customer Roster', key: 'Menu:CustomerRoster', link: '/sales/customers', icon: 'pi-users', permission: PERMISSIONS.Customers },
        { label: 'Sales Deal Pipeline', key: 'Menu:SalesPipeline', link: '/sales/pipeline', icon: 'pi-chart-bar' }
      ]
    },
    {
      label: 'Sales & Fulfillment', key: 'Menu:SalesFulfillment',
      icon: 'pi-shopping-cart',
      expanded: true,
      items: [
        { label: 'Sales Dashboard', key: 'Menu:SalesDashboard', link: '/sales/dashboard', icon: 'pi-chart-line' },
        { label: 'Sales Quotations', key: 'Menu:SalesQuotations', link: '/sales/quotations', icon: 'pi-file' },
        { label: 'Sales Orders', key: 'Menu:SalesOrders', link: '/sales/orders', icon: 'pi-shopping-bag', badge: 'Orders' },
        { label: 'Delivery Notes', key: 'Menu:DeliveryNotes', link: '/sales/delivery-notes', icon: 'pi-truck' },
        { label: 'Billing & Invoices', key: 'Menu:BillingInvoices', link: '/sales/invoices', icon: 'pi-file-pdf', permission: PERMISSIONS.Invoices }
      ]
    },
    {
      label: 'Procurement & Vendors', key: 'Menu:ProcurementVendors',
      icon: 'pi-briefcase',
      expanded: true,
      items: [
        { label: 'Suppliers & Vendors', key: 'Menu:SuppliersVendors', link: '/inventory/suppliers', icon: 'pi-building', badge: 'Vendors' },
        { label: 'Purchase Requests & RFQ', key: 'Menu:PurchaseRequests', link: '/inventory/purchase-requests', icon: 'pi-file-edit' },
        { label: 'Purchase Orders', key: 'Menu:PurchaseOrders', link: '/inventory/purchase-orders', icon: 'pi-shopping-cart' },
        { label: 'Goods Receipts & QC', key: 'Menu:GoodsReceiptsQc', link: '/inventory/goods-receipts', icon: 'pi-check-square', badge: 'GRN' }
      ]
    },
    {
      label: 'Inventory & Operations', key: 'Menu:InventoryOperations',
      icon: 'pi-box',
      expanded: true,
      items: [
        { label: 'Products Catalog', key: 'Menu:ProductsCatalog', link: '/inventory/products', icon: 'pi-list' },
        { label: 'Warehouses', key: 'Menu:Warehouses', link: '/inventory/warehouses', icon: 'pi-building' },
        { label: 'Stock Transfers', key: 'Menu:StockTransfers', link: '/inventory/transfers', icon: 'pi-sync' },
        { label: 'Stock Operations & Audits', key: 'Menu:StockOperations', link: '/inventory/stock-operations', icon: 'pi-history', badge: 'Stock' },
        { label: 'Barcode Scanner', key: 'Menu:BarcodeScanner', link: '/inventory/barcode-scanner', icon: 'pi-qrcode', badge: 'PWA' }
      ]
    },
    {
      label: 'Finance & Accounting', key: 'Menu:FinanceAccounting',
      icon: 'pi-wallet',
      expanded: true,
      items: [
        { label: 'Ledger, P&L & Aging', key: 'Menu:LedgerPnlAging', link: '/finance/dashboard', icon: 'pi-chart-line', badge: 'GL' },
        { label: 'Expense Reimbursements', key: 'Menu:ExpenseReimbursements', link: '/finance/expenses', icon: 'pi-dollar', badge: 'Exp' }
      ]
    },
    {
      label: 'Projects & Operations', key: 'Menu:ProjectsOperations',
      icon: 'pi-folder',
      expanded: true,
      items: [
        { label: 'Projects & Timesheets', key: 'Menu:ProjectsTimesheets', link: '/projects', icon: 'pi-list-check', badge: 'Prj' },
        { label: 'Manufacturing & BOM', key: 'Menu:ManufacturingBom', link: '/manufacturing', icon: 'pi-cog', badge: 'MRP' },
        { label: 'Assets & Maintenance', key: 'Menu:AssetsMaintenance', link: '/assets', icon: 'pi-wrench', badge: 'EAM' }
      ]
    },
    {
      label: 'HR & Workforce', key: 'Menu:HrWorkforce',
      icon: 'pi-users',
      expanded: true,
      items: [
        { label: 'Employees', key: 'Menu:Employees', link: '/hr/employees', icon: 'pi-user' },
        { label: 'Departments', key: 'Menu:Departments', link: '/hr/departments', icon: 'pi-building' },
        { label: 'Job Positions', key: 'Menu:JobPositions', link: '/hr/positions', icon: 'pi-id-card' },
        { label: 'Contracts', key: 'Menu:Contracts', link: '/hr/contracts', icon: 'pi-file' },
        { label: 'Attendance', key: 'Menu:Attendance', link: '/hr/attendance', icon: 'pi-calendar' },
        { label: 'Leave Management', key: 'Menu:LeaveManagement', link: '/hr/leave', icon: 'pi-briefcase' },
        { label: 'Payroll & Payslips', key: 'Menu:PayrollPayslips', link: '/hr/payroll', icon: 'pi-dollar' },
        { label: 'Recruitment Kanban', key: 'Menu:RecruitmentKanban', link: '/hr/recruitment', icon: 'pi-user-plus' }
      ]
    },
    {
      label: 'Organization Setup', key: 'Menu:OrganizationSetup',
      icon: 'pi-building',
      expanded: true,
      items: [
        { label: 'Companies & Branches', key: 'Menu:CompaniesBranches', link: '/settings/company', icon: 'pi-sitemap' },
        { label: 'Currencies & Tax', key: 'Menu:CurrenciesTax', link: '/settings/currency-tax', icon: 'pi-dollar' },
        { label: 'Payment Terms', key: 'Menu:PaymentTerms', link: '/settings/payment-terms', icon: 'pi-percentage' }
      ]
    },
    {
      label: 'Workflow Engine', key: 'Menu:WorkflowEngine',
      icon: 'pi-sitemap',
      expanded: true,
      items: [
        { label: 'Workflow Designer', key: 'Menu:WorkflowDesigner', link: '/workflow/designer', icon: 'pi-palette' },
        { label: 'Approval Center', key: 'Menu:ApprovalCenter', link: '/workflow/approvals', icon: 'pi-inbox', badge: 'New' },
        { label: 'My Tasks Inbox', key: 'Menu:MyTasksInbox', link: '/workflow/tasks', icon: 'pi-check-square', badge: '3' },
        { label: 'Execution History', key: 'Menu:ExecutionHistory', link: '/workflow/history', icon: 'pi-history' }
      ]
    },
    { label: 'AI Assistant', key: 'Menu:AIAssistant', icon: 'pi-sparkles', link: '/ai-assistant' },
    { label: 'Reports Center', key: 'Menu:ReportsCenter', icon: 'pi-chart-bar', link: '/reports' },
    { label: 'Notifications', key: 'Menu:Notifications', icon: 'pi-bell', link: '/notifications' },
    { label: 'Chat', key: 'Menu:Chat', icon: 'pi-comments', link: '/chat' },
    { label: 'Documents', key: 'Menu:Documents', icon: 'pi-folder-open', link: '/documents' },
    {
      label: 'Settings & Security', key: 'Menu:SettingsSecurity',
      icon: 'pi-cog',
      expanded: true,
      items: [
        { label: 'System Settings', key: 'Menu:SystemSettings', link: '/settings', icon: 'pi-sliders-h', permission: PERMISSIONS.Settings },
        { label: 'User Accounts', key: 'Menu:UserAccounts', link: '/settings/users', icon: 'pi-user-edit', badge: 'RBAC', permission: PERMISSIONS.Users },
        { label: 'Roles & Permissions', key: 'Menu:RolesPermissions', link: '/settings/roles', icon: 'pi-shield', badge: 'RBAC', permission: PERMISSIONS.Roles },
        { label: 'Audit Trail Logs', key: 'Menu:AuditTrailLogs', link: '/settings/audit-trail', icon: 'pi-history' },
        { label: 'API & Integrations', key: 'Menu:ApiIntegrations', link: '/settings/integrations', icon: 'pi-cloud', badge: 'API' }
      ]
    }
  ];

  isItemVisible(permission?: string): boolean {
    if (!permission) return true;
    return this.state.hasPermission(permission);
  }

  isGroupVisible(group: NavGroup): boolean {
    if (group.permission && !this.state.hasPermission(group.permission)) {
      return false;
    }
    if (group.items) {
      return group.items.some(item => this.isItemVisible(item.permission));
    }
    return true;
  }
}
