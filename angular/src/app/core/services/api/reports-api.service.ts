import { Injectable } from '@angular/core';
import { ErpApiService, toDateString, AbpEntity } from './erp-api.service';
import { ReportDefinition } from '../../models/erp-models';

interface ReportDefinitionDto extends AbpEntity {
  title: string;
  category: ReportDefinition['category'];
  description: string;
  lastGenerated: string | null;
  recordCount: number;
  dataSourceCode: string;
  isEnabled: boolean;
}

/** A materialised report: columns + rows, ready to render or export as CSV. */
export interface ReportRunResult {
  title: string;
  category: string;
  columns: string[];
  rows: { cells: string[] }[];
  generatedAt: string;
  recordCount: number;
}

interface ReportRunResultDto {
  title: string;
  category: string;
  columns: string[];
  rows: { cells: string[] }[];
  generatedAt: string;
  recordCount: number;
}

@Injectable({ providedIn: 'root' })
export class ReportsApiService extends ErpApiService {
  getReports(category?: string): Promise<ReportDefinition[]> {
    const route = category ? `report-definition?category=${encodeURIComponent(category)}` : 'report-definition';
    return this.getList<ReportDefinitionDto>(route).then(items =>
      items.map(r => ({
        id: r.id,
        title: r.title,
        category: r.category,
        description: r.description,
        lastGenerated: toDateString(r.lastGenerated ?? ''),
        recordCount: r.recordCount
      })) as ReportDefinition[]
    );
  }

  /** Executes the report's data source server-side and returns live rows. */
  runReport(id: string): Promise<ReportRunResult> {
    return this.post<ReportRunResultDto>(`report-definition/${id}/run`, {}).then(r => ({
      title: r.title,
      category: r.category,
      columns: r.columns ?? [],
      rows: r.rows ?? [],
      generatedAt: r.generatedAt,
      recordCount: r.recordCount
    }));
  }
}
