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
      label: 'Organization Setup',
      icon: 'pi-building',
      expanded: true,
      items: [
        { label: 'Companies & Branches', link: '/settings/company', icon: 'pi-sitemap', badge: 'Org' },
        { label: 'Currencies & Tax', link: '/settings/currency-tax', icon: 'pi-dollar' },
        { label: 'Payment Terms', link: '/settings/payment-terms', icon: 'pi-percentage' }
      ]
    },
    {
      label: 'Finance & Ledger',
      icon: 'pi-wallet',
      expanded: true,
      items: [
        { label: 'General Ledger & COA', link: '/finance/dashboard', icon: 'pi-chart-line' }
      ]
    },
    {
      label: 'HR & Workforce',
      icon: 'pi-users',
      expanded: true,
      items: [
        { label: 'Employees', link: '/hr/employees', icon: 'pi-user' },
        { label: 'Departments', link: '/hr/departments', icon: 'pi-building' },
        { label: 'Job Positions', link: '/hr/positions', icon: 'pi-id-card', badge: 'New' },
        { label: 'Contracts', link: '/hr/contracts', icon: 'pi-file', badge: 'New' },
        { label: 'Attendance', link: '/hr/attendance', icon: 'pi-calendar' },
        { label: 'Leave Management', link: '/hr/leave', icon: 'pi-briefcase' },
        { label: 'Payroll & Payslips', link: '/hr/payroll', icon: 'pi-dollar' },
        { label: 'Recruitment Kanban', link: '/hr/recruitment', icon: 'pi-user-plus' }
      ]
    },
    {
      label: 'Inventory & Logistics',
      icon: 'pi-box',
      expanded: true,
      items: [
        { label: 'Products Catalog', link: '/inventory/products', icon: 'pi-list' },
        { label: 'Warehouses', link: '/inventory/warehouses', icon: 'pi-building' },
        { label: 'Stock Transfers', link: '/inventory/transfers', icon: 'pi-sync' },
        { label: 'Purchase Orders', link: '/inventory/purchase-orders', icon: 'pi-shopping-bag' },
        { label: 'Barcode Scanner', link: '/inventory/barcode-scanner', icon: 'pi-qrcode', badge: 'PWA' }
      ]
    },
    {
      label: 'Sales & CRM',
      icon: 'pi-briefcase',
      expanded: true,
      items: [
        { label: 'Sales Dashboard', link: '/sales/dashboard', icon: 'pi-chart-line', badge: 'New' },
        { label: 'Quotations', link: '/sales/quotations', icon: 'pi-file', badge: 'New' },
        { label: 'Billing & Invoices', link: '/sales/invoices', icon: 'pi-file-pdf' },
        { label: 'Sales Pipeline', link: '/sales/pipeline', icon: 'pi-chart-bar' }
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
        { label: 'Audit Trail Logs', link: '/settings/audit-trail', icon: 'pi-history' }
      ]
    }
  ];
}
