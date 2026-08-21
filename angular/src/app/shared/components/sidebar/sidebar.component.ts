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
      label: 'HR Module',
      icon: 'pi-users',
      expanded: true,
      items: [
        { label: 'Employees', link: '/hr/employees', icon: 'pi-user' },
        { label: 'Departments', link: '/hr/departments', icon: 'pi-building' },
        { label: 'Attendance', link: '/hr/attendance', icon: 'pi-calendar' },
        { label: 'Leave Management', link: '/hr/leave', icon: 'pi-briefcase' },
        { label: 'Recruitment', link: '/hr/recruitment', icon: 'pi-user-plus', badge: 'New' }
      ]
    },
    {
      label: 'Inventory',
      icon: 'pi-box',
      expanded: true,
      items: [
        { label: 'Products', link: '/inventory/products', icon: 'pi-list' },
        { label: 'Warehouses', link: '/inventory/warehouses', icon: 'pi-building' },
        { label: 'Stock Transfers', link: '/inventory/transfers', icon: 'pi-sync' },
        { label: 'Purchase Orders', link: '/inventory/purchase-orders', icon: 'pi-shopping-bag' }
      ]
    },
    {
      label: 'Sales & Financials',
      icon: 'pi-dollar',
      expanded: true,
      items: [
        { label: 'Billing & Invoices', link: '/sales/invoices', icon: 'pi-file-pdf', badge: 'New' }
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
    { label: 'Settings', icon: 'pi-cog', link: '/settings' }
  ];
}
