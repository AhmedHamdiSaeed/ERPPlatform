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
    { label: 'Dashboard', icon: 'pi-home', link: '/dashboard' },
    {
      label: 'Finance & Ledger',
      icon: 'pi-wallet',
      expanded: true,
      items: [
        { label: 'General Ledger & COA', link: '/finance/dashboard', icon: 'pi-chart-line', badge: 'New' }
      ]
    },
    {
      label: 'HR & Payroll',
      icon: 'pi-users',
      expanded: true,
      items: [
        { label: 'Employees', link: '/hr/employees', icon: 'pi-user' },
        { label: 'Departments', link: '/hr/departments', icon: 'pi-building' },
        { label: 'Attendance', link: '/hr/attendance', icon: 'pi-calendar' },
        { label: 'Leave Management', link: '/hr/leave', icon: 'pi-briefcase' },
        { label: 'Payroll & Payslips', link: '/hr/payroll', icon: 'pi-dollar', badge: 'New' },
        { label: 'Recruitment Kanban', link: '/hr/recruitment', icon: 'pi-user-plus' }
      ]
    },
    {
      label: 'Inventory & Logistics',
      icon: 'pi-box',
      expanded: true,
      items: [
        { label: 'Products', link: '/inventory/products', icon: 'pi-list' },
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
        { label: 'Billing & Invoices', link: '/sales/invoices', icon: 'pi-file-pdf' },
        { label: 'Sales Deal Pipeline', link: '/sales/pipeline', icon: 'pi-chart-bar', badge: 'New' }
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
        { label: 'Audit Trail Logs', link: '/settings/audit-trail', icon: 'pi-history', badge: 'Audit' }
      ]
    }
  ];
}
