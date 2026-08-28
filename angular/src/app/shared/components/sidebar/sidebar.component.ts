import { Component, inject } from '@angular/core';
import { RouterModule } from '@angular/router';
import { StateService } from '../../../core/services/state.service';

interface NavGroup {
  label: string;
  icon: string;
  expanded?: boolean;
  items?: { label: string; link: string; icon: string; badge?: string }[];
  link?: string;
}

@Component({
  selector: 'app-sidebar',
  standalone: true,
  imports: [RouterModule],
  templateUrl: './sidebar.component.html'
})
export class SidebarComponent {
  state = inject(StateService);

  menu: NavGroup[] = [
    { label: 'Executive Dashboard', icon: 'pi-home', link: '/dashboard' },
    {
      label: 'SaaS & Subscriptions',
      icon: 'pi-star',
      expanded: true,
      items: [
        { label: 'Tenant Subscription', link: '/saas/subscription', icon: 'pi-credit-card', badge: 'Tier' },
        { label: 'Feature Usage & Limits', link: '/saas/usage', icon: 'pi-chart-pie', badge: 'Limits' },
        { label: 'Admin SaaS Plans', link: '/saas/plans', icon: 'pi-sliders-h', badge: 'Admin' }
      ]
    },
    {
      label: 'CRM & Customers',
      icon: 'pi-address-book',
      expanded: true,
      items: [
        { label: 'CRM Leads', link: '/sales/crm/leads', icon: 'pi-user-plus', badge: 'CRM' },
        { label: 'Customer Roster', link: '/sales/customers', icon: 'pi-users' },
        { label: 'Sales Deal Pipeline', link: '/sales/pipeline', icon: 'pi-chart-bar' }
      ]
    },
    {
      label: 'Sales & Fulfillment',
      icon: 'pi-shopping-cart',
      expanded: true,
      items: [
        { label: 'Sales Dashboard', link: '/sales/dashboard', icon: 'pi-chart-line' },
        { label: 'Sales Quotations', link: '/sales/quotations', icon: 'pi-file' },
        { label: 'Sales Orders', link: '/sales/orders', icon: 'pi-shopping-bag', badge: 'Orders' },
        { label: 'Delivery Notes', link: '/sales/delivery-notes', icon: 'pi-truck' },
        { label: 'Billing & Invoices', link: '/sales/invoices', icon: 'pi-file-pdf' }
      ]
    },
    {
      label: 'Procurement & Vendors',
      icon: 'pi-briefcase',
      expanded: true,
      items: [
        { label: 'Suppliers & Vendors', link: '/inventory/suppliers', icon: 'pi-building', badge: 'Vendors' },
        { label: 'Purchase Requests & RFQ', link: '/inventory/purchase-requests', icon: 'pi-file-edit' },
        { label: 'Purchase Orders', link: '/inventory/purchase-orders', icon: 'pi-shopping-cart' },
        { label: 'Goods Receipts & QC', link: '/inventory/goods-receipts', icon: 'pi-check-square', badge: 'GRN' }
      ]
    },
    {
      label: 'Inventory & Operations',
      icon: 'pi-box',
      expanded: true,
      items: [
        { label: 'Products Catalog', link: '/inventory/products', icon: 'pi-list' },
        { label: 'Warehouses', link: '/inventory/warehouses', icon: 'pi-building' },
        { label: 'Stock Transfers', link: '/inventory/transfers', icon: 'pi-sync' },
        { label: 'Stock Operations & Audits', link: '/inventory/stock-operations', icon: 'pi-history', badge: 'Stock' },
        { label: 'Barcode Scanner', link: '/inventory/barcode-scanner', icon: 'pi-qrcode', badge: 'PWA' }
      ]
    },
    {
      label: 'Finance & Accounting',
      icon: 'pi-wallet',
      expanded: true,
      items: [
        { label: 'Ledger, P&L & Aging', link: '/finance/dashboard', icon: 'pi-chart-line', badge: 'GL' },
        { label: 'Expense Reimbursements', link: '/finance/expenses', icon: 'pi-dollar', badge: 'Exp' }
      ]
    },
    {
      label: 'Projects & Operations',
      icon: 'pi-folder',
      expanded: true,
      items: [
        { label: 'Projects & Timesheets', link: '/projects', icon: 'pi-list-check', badge: 'Prj' },
        { label: 'Manufacturing & BOM', link: '/manufacturing', icon: 'pi-cog', badge: 'MRP' },
        { label: 'Assets & Maintenance', link: '/assets', icon: 'pi-wrench', badge: 'EAM' }
      ]
    },
    {
      label: 'HR & Workforce',
      icon: 'pi-users',
      expanded: true,
      items: [
        { label: 'Employees', link: '/hr/employees', icon: 'pi-user' },
        { label: 'Departments', link: '/hr/departments', icon: 'pi-building' },
        { label: 'Job Positions', link: '/hr/positions', icon: 'pi-id-card' },
        { label: 'Contracts', link: '/hr/contracts', icon: 'pi-file' },
        { label: 'Attendance', link: '/hr/attendance', icon: 'pi-calendar' },
        { label: 'Leave Management', link: '/hr/leave', icon: 'pi-briefcase' },
        { label: 'Payroll & Payslips', link: '/hr/payroll', icon: 'pi-dollar' },
        { label: 'Recruitment Kanban', link: '/hr/recruitment', icon: 'pi-user-plus' }
      ]
    },
    {
      label: 'Organization Setup',
      icon: 'pi-building',
      expanded: true,
      items: [
        { label: 'Companies & Branches', link: '/settings/company', icon: 'pi-sitemap' },
        { label: 'Currencies & Tax', link: '/settings/currency-tax', icon: 'pi-dollar' },
        { label: 'Payment Terms', link: '/settings/payment-terms', icon: 'pi-percentage' }
      ]
    },
    {
      label: 'Workflow Engine',
      icon: 'pi-sitemap',
      expanded: true,
      items: [
        { label: 'Workflow Designer', link: '/workflow/designer', icon: 'pi-palette' },
        { label: 'My Tasks Inbox', link: '/workflow/tasks', icon: 'pi-check-square', badge: '3' },
        { label: 'Execution History', link: '/workflow/history', icon: 'pi-history' }
      ]
    },
    { label: 'AI Assistant', icon: 'pi-sparkles', link: '/ai-assistant' },
    { label: 'Reports Center', icon: 'pi-chart-bar', link: '/reports' },
    { label: 'Notifications', icon: 'pi-bell', link: '/notifications' },
    { label: 'Chat', icon: 'pi-comments', link: '/chat' },
    { label: 'Documents', icon: 'pi-folder-open', link: '/documents' },
    {
      label: 'Settings & Security',
      icon: 'pi-cog',
      expanded: true,
      items: [
        { label: 'System Settings', link: '/settings', icon: 'pi-sliders-h' },
        { label: 'User Accounts', link: '/settings/users', icon: 'pi-user-edit', badge: 'RBAC' },
        { label: 'Roles & Permissions', link: '/settings/roles', icon: 'pi-shield', badge: 'RBAC' },
        { label: 'Audit Trail Logs', link: '/settings/audit-trail', icon: 'pi-history' },
        { label: 'API & Integrations', link: '/settings/integrations', icon: 'pi-cloud', badge: 'API' }
      ]
    }
  ];
}
