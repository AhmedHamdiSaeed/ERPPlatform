import { Component, inject } from '@angular/core';
import { RouterModule } from '@angular/router';
import { LocalizationPipe } from '@abp/ng.core';
import { StateService } from '../../../core/services/state.service';
import { PERMISSIONS } from '../../../core/models/permissions';

interface NavItem {
  label: string;
  key: string;
  arLabel?: string;
  link: string;
  icon: string;
  badge?: string;
  permission?: string;
}

interface NavGroup {
  label: string;
  key: string;
  arLabel?: string;
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
    { label: 'Executive Dashboard', key: 'Menu:ExecutiveDashboard', arLabel: 'لوحة التحكم التنفيذية', icon: 'pi-home', link: '/dashboard', permission: PERMISSIONS.DashboardView },
    {
      label: 'SaaS & Subscriptions', key: 'Menu:SaasSubscriptions', arLabel: 'الاشتراكات والخدمات',
      icon: 'pi-star',
      expanded: true,
      items: [
        { label: 'Tenant Subscription', key: 'Menu:TenantSubscription', arLabel: 'اشتراك المستأجر', link: '/saas/subscription', icon: 'pi-credit-card', badge: 'Tier' },
        { label: 'Feature Usage & Limits', key: 'Menu:FeatureUsageLimits', arLabel: 'استخدام الميزات والحدود', link: '/saas/usage', icon: 'pi-chart-pie', badge: 'Limits' },
        { label: 'Admin SaaS Plans', key: 'Menu:AdminSaasPlans', arLabel: 'خطط النظام', link: '/saas/plans', icon: 'pi-sliders-h', badge: 'Admin' }
      ]
    },
    {
      label: 'CRM & Customers', key: 'Menu:CrmCustomers', arLabel: 'إدارة العملاء',
      icon: 'pi-address-book',
      expanded: true,
      items: [
        { label: 'CRM Leads', key: 'Menu:CrmLeads', arLabel: 'عملاء محتملون', link: '/sales/crm/leads', icon: 'pi-user-plus', badge: 'CRM' },
        { label: 'Customer Roster', key: 'Menu:CustomerRoster', arLabel: 'قائمة العملاء', link: '/sales/customers', icon: 'pi-users', permission: PERMISSIONS.Customers },
        { label: 'Sales Deal Pipeline', key: 'Menu:SalesPipeline', arLabel: 'مسار الصفقات', link: '/sales/pipeline', icon: 'pi-chart-bar' }
      ]
    },
    {
      label: 'Sales & Fulfillment', key: 'Menu:SalesFulfillment', arLabel: 'المبيعات والتنفيذ',
      icon: 'pi-shopping-cart',
      expanded: true,
      items: [
        { label: 'Sales Dashboard', key: 'Menu:SalesDashboard', arLabel: 'لوحة المبيعات', link: '/sales/dashboard', icon: 'pi-chart-line' },
        { label: 'Sales Quotations', key: 'Menu:SalesQuotations', arLabel: 'عروض الأسعار', link: '/sales/quotations', icon: 'pi-file' },
        { label: 'Sales Orders', key: 'Menu:SalesOrders', arLabel: 'أوامر البيع', link: '/sales/orders', icon: 'pi-shopping-bag', badge: 'Orders' },
        { label: 'Delivery Notes', key: 'Menu:DeliveryNotes', arLabel: 'إشعارات التسليم', link: '/sales/delivery-notes', icon: 'pi-truck' },
        { label: 'Billing & Invoices', key: 'Menu:BillingInvoices', arLabel: 'الفواتير والتحصيل', link: '/sales/invoices', icon: 'pi-file-pdf', permission: PERMISSIONS.Invoices }
      ]
    },
    {
      label: 'Procurement & Vendors', key: 'Menu:ProcurementVendors', arLabel: 'المشتريات والموردين',
      icon: 'pi-briefcase',
      expanded: true,
      items: [
        { label: 'Suppliers & Vendors', key: 'Menu:SuppliersVendors', arLabel: 'الموردون', link: '/inventory/suppliers', icon: 'pi-building', badge: 'Vendors' },
        { label: 'Purchase Requests & RFQ', key: 'Menu:PurchaseRequests', arLabel: 'طلبات الشراء', link: '/inventory/purchase-requests', icon: 'pi-file-edit' },
        { label: 'Purchase Orders', key: 'Menu:PurchaseOrders', arLabel: 'أوامر الشراء', link: '/inventory/purchase-orders', icon: 'pi-shopping-cart' },
        { label: 'Goods Receipts & QC', key: 'Menu:GoodsReceiptsQc', arLabel: 'استلام البضائع والجودة', link: '/inventory/goods-receipts', icon: 'pi-check-square', badge: 'GRN' }
      ]
    },
    {
      label: 'Inventory & Operations', key: 'Menu:InventoryOperations', arLabel: 'المخزون والعمليات',
      icon: 'pi-box',
      expanded: true,
      items: [
        { label: 'Products Catalog', key: 'Menu:ProductsCatalog', arLabel: 'كتالوج المنتجات', link: '/inventory/products', icon: 'pi-list' },
        { label: 'Warehouses', key: 'Menu:Warehouses', arLabel: 'المستودعات', link: '/inventory/warehouses', icon: 'pi-building' },
        { label: 'Stock Transfers', key: 'Menu:StockTransfers', arLabel: 'تحويلات المخزون', link: '/inventory/transfers', icon: 'pi-sync' },
        { label: 'Stock Operations & Audits', key: 'Menu:StockOperations', arLabel: 'عمليات وجرد المخزون', link: '/inventory/stock-operations', icon: 'pi-history', badge: 'Stock' },
        { label: 'Barcode Scanner', key: 'Menu:BarcodeScanner', arLabel: 'ماسح الباركود', link: '/inventory/barcode-scanner', icon: 'pi-qrcode', badge: 'PWA' }
      ]
    },
    {
      label: 'Finance & Accounting', key: 'Menu:FinanceAccounting', arLabel: 'المالية والمحاسبة',
      icon: 'pi-wallet',
      expanded: true,
      items: [
        { label: 'Ledger, P&L & Aging', key: 'Menu:LedgerPnlAging', arLabel: 'دفتر الأستاذ والأرباح والخسائر', link: '/finance/dashboard', icon: 'pi-chart-line', badge: 'GL' },
        { label: 'Expense Reimbursements', key: 'Menu:ExpenseReimbursements', arLabel: 'تسديد المصروفات', link: '/finance/expenses', icon: 'pi-dollar', badge: 'Exp' }
      ]
    },
    {
      label: 'Projects & Operations', key: 'Menu:ProjectsOperations', arLabel: 'المشاريع والعمليات',
      icon: 'pi-folder',
      expanded: true,
      items: [
        { label: 'Projects & Timesheets', key: 'Menu:ProjectsTimesheets', arLabel: 'المشاريع وسجلات الوقت', link: '/projects', icon: 'pi-list-check', badge: 'Prj' },
        { label: 'Manufacturing & BOM', key: 'Menu:ManufacturingBom', arLabel: 'التصنيع ومتطلبات الإنتاج', link: '/manufacturing', icon: 'pi-cog', badge: 'MRP' },
        { label: 'Assets & Maintenance', key: 'Menu:AssetsMaintenance', arLabel: 'الأصول والصيانة', link: '/assets', icon: 'pi-wrench', badge: 'EAM' }
      ]
    },
    {
      label: 'HR & Workforce', key: 'Menu:HrWorkforce', arLabel: 'الموارد البشرية',
      icon: 'pi-users',
      expanded: true,
      items: [
        { label: 'Employees', key: 'Menu:Employees', arLabel: 'الموظفون', link: '/hr/employees', icon: 'pi-user' },
        { label: 'Departments', key: 'Menu:Departments', arLabel: 'الأقسام', link: '/hr/departments', icon: 'pi-building' },
        { label: 'Job Positions', key: 'Menu:JobPositions', arLabel: 'المناصب الوظيفية', link: '/hr/positions', icon: 'pi-id-card' },
        { label: 'Contracts', key: 'Menu:Contracts', arLabel: 'العقود', link: '/hr/contracts', icon: 'pi-file' },
        { label: 'Attendance', key: 'Menu:Attendance', arLabel: 'الحضور', link: '/hr/attendance', icon: 'pi-calendar' },
        { label: 'Leave Management', key: 'Menu:LeaveManagement', arLabel: 'إدارة الإجازات', link: '/hr/leave', icon: 'pi-briefcase' },
        { label: 'Payroll & Payslips', key: 'Menu:PayrollPayslips', arLabel: 'الرواتب وكشوف المرتبات', link: '/hr/payroll', icon: 'pi-dollar' },
        { label: 'Recruitment Kanban', key: 'Menu:RecruitmentKanban', arLabel: 'لوحة التوظيف', link: '/hr/recruitment', icon: 'pi-user-plus' }
      ]
    },
    {
      label: 'Organization Setup', key: 'Menu:OrganizationSetup', arLabel: 'إعداد المؤسسة',
      icon: 'pi-building',
      expanded: true,
      items: [
        { label: 'Companies & Branches', key: 'Menu:CompaniesBranches', arLabel: 'الشركات والفروع', link: '/settings/company', icon: 'pi-sitemap' },
        { label: 'Currencies & Tax', key: 'Menu:CurrenciesTax', arLabel: 'العملات والضرائب', link: '/settings/currency-tax', icon: 'pi-dollar' },
        { label: 'Payment Terms', key: 'Menu:PaymentTerms', arLabel: 'شروط الدفع', link: '/settings/payment-terms', icon: 'pi-percentage' }
      ]
    },
    {
      label: 'Workflow Engine', key: 'Menu:WorkflowEngine', arLabel: 'محرك سير العمل',
      icon: 'pi-sitemap',
      expanded: true,
      items: [
        { label: 'Workflow Designer', key: 'Menu:WorkflowDesigner', arLabel: 'مصمم سير العمل', link: '/workflow/designer', icon: 'pi-palette' },
        { label: 'Approval Center', key: 'Menu:ApprovalCenter', arLabel: 'صندوق الموافقات', link: '/workflow/approvals', icon: 'pi-inbox', badge: 'New' },
        { label: 'My Tasks Inbox', key: 'Menu:MyTasksInbox', arLabel: 'صندوق مهامي', link: '/workflow/tasks', icon: 'pi-check-square', badge: '3' },
        { label: 'Execution History', key: 'Menu:ExecutionHistory', arLabel: 'سجل التنفيذ', link: '/workflow/history', icon: 'pi-history' }
      ]
    },
    { label: 'AI Assistant', key: 'Menu:AIAssistant', arLabel: 'المساعد الذكي', icon: 'pi-sparkles', link: '/ai-assistant' },
    { label: 'Reports Center', key: 'Menu:ReportsCenter', arLabel: 'مركز التقارير', icon: 'pi-chart-bar', link: '/reports' },
    { label: 'Notifications', key: 'Menu:Notifications', arLabel: 'الإشعارات', icon: 'pi-bell', link: '/notifications' },
    { label: 'Chat', key: 'Menu:Chat', arLabel: 'المحادثة', icon: 'pi-comments', link: '/chat' },
    { label: 'Documents', key: 'Menu:Documents', arLabel: 'المستندات', icon: 'pi-folder-open', link: '/documents' },
    {
      label: 'Settings & Security', key: 'Menu:SettingsSecurity', arLabel: 'الإعدادات والأمان',
      icon: 'pi-cog',
      expanded: true,
      items: [
        { label: 'System Settings', key: 'Menu:SystemSettings', arLabel: 'إعدادات النظام', link: '/settings', icon: 'pi-sliders-h', permission: PERMISSIONS.Settings },
        { label: 'User Accounts', key: 'Menu:UserAccounts', arLabel: 'حسابات المستخدمين', link: '/settings/users', icon: 'pi-user-edit', badge: 'RBAC', permission: PERMISSIONS.Users },
        { label: 'Roles & Permissions', key: 'Menu:RolesPermissions', arLabel: 'الأدوار والصلاحيات', link: '/settings/roles', icon: 'pi-shield', badge: 'RBAC', permission: PERMISSIONS.Roles },
        { label: 'Audit Trail Logs', key: 'Menu:AuditTrailLogs', arLabel: 'سجلات التدقيق', link: '/settings/audit-trail', icon: 'pi-history' },
        { label: 'API & Integrations', key: 'Menu:ApiIntegrations', arLabel: 'الواجهات والتكاملات', link: '/settings/integrations', icon: 'pi-cloud', badge: 'API' }
      ]
    }
  ];

  getLabel(item: NavGroup | NavItem): string {
    return this.state.isRtl() ? (item.arLabel || item.label) : item.label;
  }

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
