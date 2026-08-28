import { Routes } from '@angular/router';
import { LayoutShellComponent } from './layout/layout-shell.component';
import { authGuard } from './core/guards/auth.guard';

export const appRoutes: Routes = [
  // Auth routes (no layout shell)
  {
    path: 'auth/login',
    loadComponent: () => import('./features/auth/login/login.component').then(m => m.LoginComponent)
  },
  {
    path: 'auth/forgot-password',
    loadComponent: () => import('./features/auth/forgot-password/forgot-password.component').then(m => m.ForgotPasswordComponent)
  },
  {
    path: 'auth/reset-password',
    loadComponent: () => import('./features/auth/reset-password/reset-password.component').then(m => m.ResetPasswordComponent)
  },

  // ERP Application routes (wrapped in layout shell & protected by authGuard)
  {
    path: '',
    component: LayoutShellComponent,
    canActivate: [authGuard],
    children: [
      // Dashboard
      {
        path: 'dashboard',
        loadComponent: () => import('./features/dashboard/dashboard.component').then(m => m.DashboardComponent)
      },

      // Profile & Change Password
      {
        path: 'profile',
        loadComponent: () => import('./features/auth/profile/user-profile.component').then(m => m.UserProfileComponent)
      },
      {
        path: 'auth/change-password',
        loadComponent: () => import('./features/auth/change-password/change-password.component').then(m => m.ChangePasswordComponent)
      },

      // ─── Finance Module ───
      {
        path: 'finance/dashboard',
        loadComponent: () => import('./features/finance/finance-dashboard.component').then(m => m.FinanceDashboardComponent)
      },

      // ─── HR Module ───
      {
        path: 'hr/employees',
        loadComponent: () => import('./features/hr/employees/employee-list.component').then(m => m.EmployeeListComponent)
      },
      {
        path: 'hr/employees/:id',
        loadComponent: () => import('./features/hr/employees/employee-detail.component').then(m => m.EmployeeDetailComponent)
      },
      {
        path: 'hr/departments',
        loadComponent: () => import('./features/hr/departments/department-list.component').then(m => m.DepartmentListComponent)
      },
      {
        path: 'hr/positions',
        loadComponent: () => import('./features/hr/positions/positions.component').then(m => m.PositionsComponent)
      },
      {
        path: 'hr/contracts',
        loadComponent: () => import('./features/hr/contracts/contracts.component').then(m => m.ContractsComponent)
      },
      {
        path: 'hr/attendance',
        loadComponent: () => import('./features/hr/attendance/attendance-list.component').then(m => m.AttendanceListComponent)
      },
      {
        path: 'hr/leave',
        loadComponent: () => import('./features/hr/leave/leave-management.component').then(m => m.LeaveManagementComponent)
      },
      {
        path: 'hr/payroll',
        loadComponent: () => import('./features/hr/payroll/payroll-management.component').then(m => m.PayrollManagementComponent)
      },
      {
        path: 'hr/recruitment',
        loadComponent: () => import('./features/hr/recruitment/recruitment-kanban.component').then(m => m.RecruitmentKanbanComponent)
      },

      // ─── Inventory Module ───
      {
        path: 'inventory/products',
        loadComponent: () => import('./features/inventory/products/product-list.component').then(m => m.ProductListComponent)
      },
      {
        path: 'inventory/warehouses',
        loadComponent: () => import('./features/inventory/warehouses/warehouse-list.component').then(m => m.WarehouseListComponent)
      },
      {
        path: 'inventory/transfers',
        loadComponent: () => import('./features/inventory/transfers/stock-transfer-list.component').then(m => m.StockTransferListComponent)
      },
      {
        path: 'inventory/purchase-orders',
        loadComponent: () => import('./features/inventory/purchase-orders/purchase-order-list.component').then(m => m.PurchaseOrderListComponent)
      },
      {
        path: 'inventory/barcode-scanner',
        loadComponent: () => import('./shared/components/barcode-scanner/barcode-scanner.component').then(m => m.BarcodeScannerComponent)
      },

      // ─── Sales & CRM ───
      {
        path: 'sales/dashboard',
        loadComponent: () => import('./features/sales/sales-dashboard/sales-dashboard.component').then(m => m.SalesDashboardComponent)
      },
      {
        path: 'sales/invoices',
        loadComponent: () => import('./features/sales/sales-invoices.component').then(m => m.SalesInvoicesComponent)
      },
      {
        path: 'sales/quotations',
        loadComponent: () => import('./features/sales/quotations/sales-quotations.component').then(m => m.SalesQuotationsComponent)
      },
      {
        path: 'sales/pipeline',
        loadComponent: () => import('./features/sales/pipeline/sales-pipeline-kanban.component').then(m => m.SalesPipelineKanbanComponent)
      },

      // ─── Workflow Engine ───
      {
        path: 'workflow/designer',
        loadComponent: () => import('./features/workflow/designer/workflow-designer.component').then(m => m.WorkflowDesignerComponent)
      },
      {
        path: 'workflow/tasks',
        loadComponent: () => import('./features/workflow/tasks/my-tasks.component').then(m => m.MyTasksComponent)
      },
      {
        path: 'workflow/history',
        loadComponent: () => import('./features/workflow/history/execution-history.component').then(m => m.ExecutionHistoryComponent)
      },

      // ─── AI Assistant ───
      {
        path: 'ai-assistant',
        loadComponent: () => import('./features/ai-assistant/ai-assistant-page.component').then(m => m.AiAssistantPageComponent)
      },

      // ─── Reports ───
      {
        path: 'reports',
        loadComponent: () => import('./features/reports/report-center.component').then(m => m.ReportCenterComponent)
      },

      // ─── Notifications ───
      {
        path: 'notifications',
        loadComponent: () => import('./features/notifications/notifications.component').then(m => m.NotificationsComponent)
      },

      // ─── Chat ───
      {
        path: 'chat',
        loadComponent: () => import('./features/chat/chat.component').then(m => m.ChatComponent)
      },

      // ─── Settings, Security & Org ───
      {
        path: 'settings',
        loadComponent: () => import('./features/settings/settings.component').then(m => m.SettingsComponent)
      },
      {
        path: 'settings/company',
        loadComponent: () => import('./features/settings/company/company-management.component').then(m => m.CompanyManagementComponent)
      },
      {
        path: 'settings/currency-tax',
        loadComponent: () => import('./features/settings/currency-tax/currency-tax.component').then(m => m.CurrencyTaxComponent)
      },
      {
        path: 'settings/payment-terms',
        loadComponent: () => import('./features/settings/payment-terms/payment-terms.component').then(m => m.PaymentTermsComponent)
      },
      {
        path: 'settings/users',
        loadComponent: () => import('./features/settings/users/user-management.component').then(m => m.UserManagementComponent)
      },
      {
        path: 'settings/roles',
        loadComponent: () => import('./features/settings/roles/roles-permissions.component').then(m => m.RolesPermissionsComponent)
      },
      {
        path: 'settings/audit-trail',
        loadComponent: () => import('./features/settings/audit-trail/audit-trail.component').then(m => m.AuditTrailComponent)
      },

      // ─── Document Manager ───
      {
        path: 'documents',
        loadComponent: () => import('./features/documents/document-manager.component').then(m => m.DocumentManagerComponent)
      },

      // Default redirect
      {
        path: '',
        redirectTo: 'dashboard',
        pathMatch: 'full'
      }
    ]
  },

  // Catch-all redirect
  {
    path: '**',
    redirectTo: 'dashboard'
  }
];
