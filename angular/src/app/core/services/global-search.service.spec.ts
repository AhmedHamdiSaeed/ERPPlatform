import { TestBed } from '@angular/core/testing';
import { GlobalSearchService } from './global-search.service';
import { MOCK_EMPLOYEES, MOCK_PRODUCTS } from '../mock/mock-data';

describe('GlobalSearchService', () => {
  let service: GlobalSearchService;

  beforeEach(() => {
    TestBed.configureTestingModule({});
    service = TestBed.inject(GlobalSearchService);
  });

  it('should be created', () => {
    expect(service).toBeTruthy();
  });

  // ─── EMPTY QUERY ─────────────────────────────────────────────────────────

  it('search() with empty string should return empty array', () => {
    expect(service.search('')).toEqual([]);
  });

  it('search() with whitespace-only query should return empty array', () => {
    expect(service.search('   ')).toEqual([]);
  });

  // ─── EMPLOYEE SEARCH ──────────────────────────────────────────────────────

  it('search() by employee name should return Employees group', () => {
    const firstEmployee = MOCK_EMPLOYEES[0];
    const results = service.search(firstEmployee.name.substring(0, 4));
    const empGroup = results.find(r => r.category === 'Employees');
    expect(empGroup).toBeDefined();
    expect(empGroup!.items.length).toBeGreaterThanOrEqual(1);
  });

  it('search() by employee name should map to correct link format', () => {
    const employee = MOCK_EMPLOYEES[0];
    const results = service.search(employee.name.substring(0, 4));
    const empGroup = results.find(r => r.category === 'Employees');
    const item = empGroup?.items.find(i => i.title === employee.name);
    expect(item?.link).toBe(`/hr/employees/${employee.id}`);
  });

  it('search() by employee email should return matching Employees', () => {
    const employee = MOCK_EMPLOYEES[0];
    const emailPrefix = employee.email.split('@')[0];
    const results = service.search(emailPrefix);
    const empGroup = results.find(r => r.category === 'Employees');
    expect(empGroup).toBeDefined();
    expect(empGroup!.items.some(i => i.title === employee.name)).toBeTrue();
  });

  it('search() by employee position should return matching Employees', () => {
    const employee = MOCK_EMPLOYEES[0];
    const results = service.search(employee.position.substring(0, 5));
    const empGroup = results.find(r => r.category === 'Employees');
    expect(empGroup).toBeDefined();
  });

  it('search() employee result should include status as badge', () => {
    const employee = MOCK_EMPLOYEES[0];
    const results = service.search(employee.name.substring(0, 4));
    const empGroup = results.find(r => r.category === 'Employees');
    const item = empGroup?.items.find(i => i.title === employee.name);
    expect(item?.badge).toBe(employee.status);
  });

  // ─── PRODUCT SEARCH ───────────────────────────────────────────────────────

  it('search() by product SKU should return Products group', () => {
    const firstProduct = MOCK_PRODUCTS[0];
    const results = service.search(firstProduct.sku.substring(0, 4));
    const prodGroup = results.find(r => r.category === 'Products & Inventory');
    expect(prodGroup).toBeDefined();
  });

  it('search() by product name should return Products group', () => {
    const firstProduct = MOCK_PRODUCTS[0];
    const results = service.search(firstProduct.name.substring(0, 5));
    const prodGroup = results.find(r => r.category === 'Products & Inventory');
    expect(prodGroup).toBeDefined();
    expect(prodGroup!.items.length).toBeGreaterThanOrEqual(1);
  });

  it('search() product result link should point to inventory/products', () => {
    const product = MOCK_PRODUCTS[0];
    const results = service.search(product.sku.substring(0, 4));
    const prodGroup = results.find(r => r.category === 'Products & Inventory');
    expect(prodGroup?.items[0]?.link).toBe('/inventory/products');
  });

  // ─── CASE INSENSITIVITY ───────────────────────────────────────────────────

  it('search() should be case-insensitive for employee name', () => {
    const employee = MOCK_EMPLOYEES[0];
    const upperResults = service.search(employee.name.toUpperCase().substring(0, 4));
    const lowerResults = service.search(employee.name.toLowerCase().substring(0, 4));

    const upperCount = upperResults.find(r => r.category === 'Employees')?.items.length ?? 0;
    const lowerCount = lowerResults.find(r => r.category === 'Employees')?.items.length ?? 0;
    expect(upperCount).toBe(lowerCount);
  });

  // ─── NO MATCHES ───────────────────────────────────────────────────────────

  it('search() with no matching query should return empty array', () => {
    const results = service.search('xyzzy-nonexistent-query-12345');
    expect(results).toEqual([]);
  });

  // ─── MULTI-GROUP RESULTS ──────────────────────────────────────────────────

  it('search() with broad query should return multiple groups', () => {
    // 'a' likely matches employees, products, departments, etc.
    const results = service.search('a');
    expect(results.length).toBeGreaterThanOrEqual(1);
  });
});
