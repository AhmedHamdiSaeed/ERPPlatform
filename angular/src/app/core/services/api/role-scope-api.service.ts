import { Injectable } from '@angular/core';
import { ErpApiService } from './erp-api.service';
import { DataScopeType } from '../../models/permission-catalog';

export interface RolePageScopeDto {
  id: string;
  roleName: string;
  pageKey: string;
  scopeType: DataScopeType;
  targetIds: string[];
}

export interface SaveRolePageScopeDto {
  pageKey: string;
  scopeType: DataScopeType;
  targetIds: string[];
}

@Injectable({ providedIn: 'root' })
export class RoleScopeApiService extends ErpApiService {
  /** GET /api/app/role-page-scope?roleName=... */
  getScopes(roleName: string): Promise<RolePageScopeDto[]> {
    return this.getList<RolePageScopeDto>(`role-page-scope?roleName=${encodeURIComponent(roleName)}`);
  }

  /** POST /api/app/role-page-scope/save */
  saveScopes(roleName: string, scopes: SaveRolePageScopeDto[]): Promise<{ items: RolePageScopeDto[] }> {
    return this.post<{ items: RolePageScopeDto[] }>('role-page-scope/save', { roleName, scopes });
  }
}
