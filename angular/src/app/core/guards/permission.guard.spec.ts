import { TestBed } from '@angular/core';
import { Router } from '@angular/router';
import { permissionGuard } from './permission.guard';
import { StateService } from '../services/state.service';

describe('permissionGuard', () => {
  let stateServiceSpy: jasmine.SpyObj<StateService>;
  let routerSpy: jasmine.SpyObj<Router>;

  beforeEach(() => {
    stateServiceSpy = jasmine.createSpyObj('StateService', ['hasPermission']);
    routerSpy = jasmine.createSpyObj('Router', ['createUrlTree']);

    TestBed.configureTestingModule({
      providers: [
        { provide: StateService, useValue: stateServiceSpy },
        { provide: Router, useValue: routerSpy }
      ]
    });
  });

  it('should allow navigation when permission is granted', () => {
    stateServiceSpy.hasPermission.and.returnValue(true);
    const guard = permissionGuard('ERPPlatform.Customers');

    const result = TestBed.runInInjectionContext(() => guard({} as any, {} as any));

    expect(result).toBeTrue();
    expect(stateServiceSpy.hasPermission).toHaveBeenCalledWith('ERPPlatform.Customers');
  });

  it('should redirect to dashboard when permission is denied', () => {
    stateServiceSpy.hasPermission.and.returnValue(false);
    routerSpy.createUrlTree.and.returnValue('/dashboard' as any);
    const guard = permissionGuard('ERPPlatform.Users');

    const result = TestBed.runInInjectionContext(() => guard({} as any, {} as any));

    expect(routerSpy.createUrlTree).toHaveBeenCalledWith(['/dashboard']);
    expect(result).toBe('/dashboard' as any);
  });
});
