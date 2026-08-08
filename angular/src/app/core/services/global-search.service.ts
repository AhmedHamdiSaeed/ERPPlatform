import { Injectable } from '@angular/core';
import { 
  MOCK_EMPLOYEES, MOCK_PRODUCTS, MOCK_WORKFLOWS, 
  MOCK_DEPARTMENTS, MOCK_PURCHASE_ORDERS, MOCK_REPORTS 
} from '../mock/mock-data';

export interface SearchResultGroup {
  category: string;
  items: {
    title: string;
    subtitle: string;
    link: string;
    icon: string;
    badge?: string;
  }[];
}

@Injectable({
  providedIn: 'root'
})
export class GlobalSearchService {

  search(query: string): SearchResultGroup[] {
    const q = query.trim().toLowerCase();
    if (!q) return [];

    const results: SearchResultGroup[] = [];

    // Employees
    const empMatches = MOCK_EMPLOYEES.filter(e => 
      e.name.toLowerCase().includes(q) || 
      e.position.toLowerCase().includes(q) || 
      e.departmentName.toLowerCase().includes(q) ||
      e.email.toLowerCase().includes(q)
    ).map(e => ({
      title: e.name,
      subtitle: `${e.position} • ${e.departmentName}`,
      link: `/hr/employees/${e.id}`,
      icon: 'pi-user',
      badge: e.status
    }));

    if (empMatches.length) {
      results.push({ category: 'Employees', items: empMatches });
    }

    // Products
    const prodMatches = MOCK_PRODUCTS.filter(p => 
      p.name.toLowerCase().includes(q) || 
      p.sku.toLowerCase().includes(q) || 
      p.category.toLowerCase().includes(q)
    ).map(p => ({
      title: p.name,
      subtitle: `SKU: ${p.sku} • Stock: ${p.stock} ${p.unit} ($${p.price})`,
      link: '/inventory/products',
      icon: 'pi-box',
      badge: p.status
    }));

    if (prodMatches.length) {
      results.push({ category: 'Products & Inventory', items: prodMatches });
    }

    // Workflows
    const wfMatches = MOCK_WORKFLOWS.filter(w => 
      w.name.toLowerCase().includes(q) || 
      w.description.toLowerCase().includes(q)
    ).map(w => ({
      title: w.name,
      subtitle: w.description,
      link: '/workflow/designer',
      icon: 'pi-sitemap',
      badge: w.version
    }));

    if (wfMatches.length) {
      results.push({ category: 'Workflows', items: wfMatches });
    }

    // Departments
    const deptMatches = MOCK_DEPARTMENTS.filter(d => 
      d.name.toLowerCase().includes(q) || 
      d.code.toLowerCase().includes(q)
    ).map(d => ({
      title: d.name,
      subtitle: `Code: ${d.code} • Manager: ${d.managerName}`,
      link: '/hr/departments',
      icon: 'pi-building'
    }));

    if (deptMatches.length) {
      results.push({ category: 'Departments', items: deptMatches });
    }

    // Purchase Orders
    const poMatches = MOCK_PURCHASE_ORDERS.filter(po => 
      po.poNumber.toLowerCase().includes(q) || 
      po.supplierName.toLowerCase().includes(q)
    ).map(po => ({
      title: `${po.poNumber} - ${po.supplierName}`,
      subtitle: `Grand Total: $${po.grandTotal.toLocaleString()} • Date: ${po.orderDate}`,
      link: '/inventory/purchase-orders',
      icon: 'pi-file',
      badge: po.status
    }));

    if (poMatches.length) {
      results.push({ category: 'Purchase Orders', items: poMatches });
    }

    // Reports
    const repMatches = MOCK_REPORTS.filter(r => 
      r.title.toLowerCase().includes(q) || 
      r.description.toLowerCase().includes(q)
    ).map(r => ({
      title: r.title,
      subtitle: r.description,
      link: '/reports',
      icon: 'pi-chart-line',
      badge: r.category
    }));

    if (repMatches.length) {
      results.push({ category: 'Reports', items: repMatches });
    }

    return results;
  }
}
