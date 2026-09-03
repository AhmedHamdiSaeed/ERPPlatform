import { Component, inject, signal, OnInit } from '@angular/core';
import { FormsModule } from '@angular/forms';
import { HttpClient } from '@angular/common/http';
import { firstValueFrom } from 'rxjs';
import { ToastService } from '../../../core/services/toast.service';
import { StateService } from '../../../core/services/state.service';
import { environment } from '../../../../environments/environment';
import { RoleScopeApiService, SaveRolePageScopeDto } from '../../../core/services/api/role-scope-api.service';
import { HrApiService } from '../../../core/services/api/hr-api.service';
import { OrgApiService, Branch } from '../../../core/services/api/org-api.service';
import {
  PERMISSION_MODULES,
  SCOPE_OPTIONS,
  DataScopeType,
  ModuleDefinition,
  PageDefinition
} from '../../../core/models/permission-catalog';
import { Employee, Department } from '../../../core/models/erp-models';

export interface SystemRole {
  id: string;
  name: string;
  isDefault?: boolean;
  isPublic?: boolean;
  description?: string;
  userCount?: number;
}

/** The four capability toggles shown for every page. */
export type PageAction = 'View' | 'Create' | 'Edit' | 'Delete';

export interface PageScopeState {
  scopeType: DataScopeType;
  targetIds: string[];
}

const DEFAULT_SCOPE: PageScopeState = { scopeType: DataScopeType.All, targetIds: [] };

@Component({
  selector: 'app-roles-permissions',
  standalone: true,
  imports: [FormsModule],
  templateUrl: './roles-permissions.component.html'
})
export class RolesPermissionsComponent implements OnInit {
  private http = inject(HttpClient);
  private toast = inject(ToastService);
  private state = inject(StateService);
  private scopeApi = inject(RoleScopeApiService);
  private hrApi = inject(HrApiService);
  private orgApi = inject(OrgApiService);

  readonly modules = PERMISSION_MODULES;
  readonly scopeOptions = SCOPE_OPTIONS;
  readonly actions: PageAction[] = ['View', 'Create', 'Edit', 'Delete'];

  roles = signal<SystemRole[]>([]);
  selectedRole = signal<SystemRole | null>(null);

  /** permission name -> granted */
  granted = signal<Record<string, boolean>>({});
  /** permission names the backend actually defines; toggles are disabled for the rest. */
  private knownPermissions = signal<Set<string>>(new Set());

  /** pageKey -> configured data scope */
  scopes = signal<Record<string, PageScopeState>>({});

  expandedModules = signal<Record<string, boolean>>({});
  expandedPages = signal<Record<string, boolean>>({});

  branches = signal<Branch[]>([]);
  departments = signal<Department[]>([]);
  employees = signal<Employee[]>([]);

  showCreateModal = signal<boolean>(false);
  newRoleName = '';
  loadingRoles = signal<boolean>(false);
  loadingPermissions = signal<boolean>(false);
  saving = signal<boolean>(false);

  async ngOnInit() {
    await this.loadRoles();
    await this.loadLookupData();
  }

  // ---------------------------------------------------------------- data load

  async loadRoles() {
    this.loadingRoles.set(true);
    try {
      const res = await firstValueFrom(
        this.http.get<{ items: SystemRole[] }>(`${environment.apis.default.url}/api/identity/roles`)
      );
      this.roles.set(res.items ?? []);
      if (this.roles().length > 0 && !this.selectedRole()) {
        await this.selectRole(this.roles()[0]);
      }
    } catch (err) {
      console.error('Failed to load roles', err);
      this.toast.error('Could not load roles from the server.');
    } finally {
      this.loadingRoles.set(false);
    }
  }

  /** Branch / department / employee pickers used by the "specific ..." scopes. */
  async loadLookupData() {
    try {
      const [branches, departments, employees] = await Promise.all([
        this.orgApi.getBranches().catch(() => [] as Branch[]),
        this.hrApi.getDepartments().catch(() => [] as Department[]),
        this.hrApi.getEmployees().catch(() => [] as Employee[])
      ]);
      this.branches.set(branches);
      this.departments.set(departments);
      this.employees.set(employees);
    } catch (err) {
      console.error('Failed to load scope lookup data', err);
    }
  }

  async selectRole(role: SystemRole) {
    this.selectedRole.set(role);
    this.expandedModules.set({});
    this.expandedPages.set({});
    await Promise.all([this.loadPermissions(role.name), this.loadScopes(role.name)]);
  }

  async loadPermissions(roleName: string) {
    this.loadingPermissions.set(true);
    try {
      const url = `${environment.apis.default.url}/api/permission-management/permissions?providerName=R&providerKey=${encodeURIComponent(roleName)}`;
      const res = await firstValueFrom(this.http.get<any>(url));

      const granted: Record<string, boolean> = {};
      const known = new Set<string>();

      for (const group of res?.groups ?? []) {
        for (const p of group.permissions ?? []) {
          if (!p?.name) continue;
          known.add(p.name);
          granted[p.name] = !!p.isGranted;
        }
      }

      this.knownPermissions.set(known);
      this.granted.set(granted);
    } catch (err) {
      console.error('Failed to load permissions', err);
      this.toast.error('Could not load permissions for this role.');
    } finally {
      this.loadingPermissions.set(false);
    }
  }

  async loadScopes(roleName: string) {
    try {
      const list = await this.scopeApi.getScopes(roleName);
      const map: Record<string, PageScopeState> = {};
      for (const item of list) {
        map[item.pageKey] = { scopeType: item.scopeType, targetIds: item.targetIds ?? [] };
      }
      this.scopes.set(map);
    } catch (err) {
      console.error('Failed to load role page scopes', err);
      this.scopes.set({});
    }
  }

  // ------------------------------------------------------------- permissions

  /** Builds the permission name for a page action, e.g. "ERPPlatform.Employees.Create". */
  permissionName(page: PageDefinition, action: PageAction): string {
    return action === 'View' ? page.key : `${page.key}.${action}`;
  }

  isGranted(page: PageDefinition, action: PageAction): boolean {
    return !!this.granted()[this.permissionName(page, action)];
  }

  /** Toggles are only interactive when the backend defines the permission. */
  isDefined(page: PageDefinition, action: PageAction): boolean {
    return this.knownPermissions().has(this.permissionName(page, action));
  }

  togglePermission(page: PageDefinition, action: PageAction) {
    if (!this.isDefined(page, action)) return;
    const name = this.permissionName(page, action);
    const current = { ...this.granted() };
    current[name] = !current[name];
    this.granted.set(current);
  }

  /** Counts how many of the page's four actions are turned on (for the module summary). */
  grantedActionCount(page: PageDefinition): number {
    return this.actions.filter(a => this.isGranted(page, a)).length;
  }

  // ------------------------------------------------------------------ scopes

  scopeFor(pageKey: string): PageScopeState {
    return this.scopes()[pageKey] ?? DEFAULT_SCOPE;
  }

  setScopeType(pageKey: string, type: DataScopeType) {
    const current = { ...this.scopes() };
    // Switching scope type clears stale targets so ids never leak between scope kinds.
    current[pageKey] = { scopeType: type, targetIds: [] };
    this.scopes.set(current);
  }

  /** Options for the multi-select, driven by the selected scope type. */
  targetOptionsFor(pageKey: string): { id: string; label: string }[] {
    switch (this.scopeFor(pageKey).scopeType) {
      case DataScopeType.SpecificBranch:
        return this.branches().map(b => ({ id: b.id, label: b.name }));
      case DataScopeType.SpecificDepartment:
        return this.departments().map(d => ({ id: d.id, label: d.name }));
      case DataScopeType.SpecificEmployees:
        return this.employees().map(e => ({ id: e.id, label: `${e.name} (${e.employeeCode})` }));
      default:
        return [];
    }
  }

  isTargetSelected(pageKey: string, id: string): boolean {
    return this.scopeFor(pageKey).targetIds.includes(id);
  }

  toggleTarget(pageKey: string, id: string) {
    const state = this.scopeFor(pageKey);
    const next = state.targetIds.includes(id)
      ? state.targetIds.filter(x => x !== id)
      : [...state.targetIds, id];

    const current = { ...this.scopes() };
    current[pageKey] = { scopeType: state.scopeType, targetIds: next };
    this.scopes.set(current);
  }

  scopeLabel(pageKey: string): string {
    const option = this.scopeOptions.find(o => o.value === this.scopeFor(pageKey).scopeType);
    return option?.label ?? 'All employees';
  }

  // ------------------------------------------------------------ accordion UI

  toggleModule(key: string) {
    const current = { ...this.expandedModules() };
    current[key] = !current[key];
    this.expandedModules.set(current);
  }

  isModuleExpanded(key: string): boolean {
    return !!this.expandedModules()[key];
  }

  togglePage(key: string) {
    const current = { ...this.expandedPages() };
    current[key] = !current[key];
    this.expandedPages.set(current);
  }

  isPageExpanded(key: string): boolean {
    return !!this.expandedPages()[key];
  }

  expandAll() {
    const modules: Record<string, boolean> = {};
    const pages: Record<string, boolean> = {};
    for (const m of this.modules) {
      modules[m.key] = true;
      for (const p of m.pages) pages[p.key] = true;
    }
    this.expandedModules.set(modules);
    this.expandedPages.set(pages);
  }

  collapseAll() {
    this.expandedModules.set({});
    this.expandedPages.set({});
  }

  moduleGrantedCount(module: ModuleDefinition): number {
    return module.pages.reduce((sum, p) => sum + this.grantedActionCount(p), 0);
  }

  moduleTotalCount(module: ModuleDefinition): number {
    return module.pages.length * this.actions.length;
  }

  // ------------------------------------------------------------------- save

  async save() {
    const role = this.selectedRole();
    if (!role) return;

    this.saving.set(true);
    try {
      await this.savePermissions(role.name);
      await this.saveScopes(role.name);
      this.toast.success(`Access policy for "${role.name}" saved.`);
      await this.state.loadAppConfig();
    } catch (err) {
      console.error('Failed to save role policy', err);
      this.toast.error('Could not save the access policy.');
    } finally {
      this.saving.set(false);
    }
  }

  private async savePermissions(roleName: string) {
    const permissions: { name: string; isGranted: boolean }[] = [];
    const known = this.knownPermissions();

    for (const module of this.modules) {
      for (const page of module.pages) {
        for (const action of this.actions) {
          const name = this.permissionName(page, action);
          // Skip permissions the backend does not define - they would be silently dropped.
          if (!known.has(name)) continue;
          permissions.push({ name, isGranted: !!this.granted()[name] });
        }
      }
    }

    if (permissions.length === 0) return;

    const url = `${environment.apis.default.url}/api/permission-management/permissions?providerName=R&providerKey=${encodeURIComponent(roleName)}`;
    await firstValueFrom(this.http.put(url, { permissions }));
  }

  private async saveScopes(roleName: string) {
    const scopes: SaveRolePageScopeDto[] = [];

    for (const module of this.modules) {
      for (const page of module.pages) {
        if (!page.supportsScope) continue;
        const state = this.scopeFor(page.key);
        scopes.push({
          pageKey: page.key,
          scopeType: state.scopeType,
          targetIds: state.targetIds
        });
      }
    }

    await this.scopeApi.saveScopes(roleName, scopes);
  }

  // ------------------------------------------------------------- create role

  openCreateModal() {
    this.newRoleName = '';
    this.showCreateModal.set(true);
  }

  async createRole() {
    if (!this.newRoleName.trim()) {
      this.toast.warning('Please enter a valid role name.');
      return;
    }

    try {
      const payload = { name: this.newRoleName.trim(), isDefault: false, isPublic: true };
      const created = await firstValueFrom(
        this.http.post<SystemRole>(`${environment.apis.default.url}/api/identity/roles`, payload)
      );
      this.toast.success(`Role "${created.name}" created.`);
      this.showCreateModal.set(false);
      await this.loadRoles();
      await this.selectRole(created);
    } catch (err) {
      console.error('Failed to create role', err);
      this.toast.error('Could not create the role.');
    }
  }
}
