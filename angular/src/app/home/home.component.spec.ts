import { CoreTestingModule } from "@abp/ng.core/testing";
import { ThemeSharedTestingModule } from "@abp/ng.theme.shared/testing";
import { ComponentFixture, TestBed, waitForAsync } from "@angular/core/testing";
import { NgxValidateCoreModule } from "@ngx-validate/core";
import { HomeComponent } from "./home.component";
import { OAuthService } from 'angular-oauth2-oidc';
import { AuthService } from '@abp/ng.core';

describe("HomeComponent", () => {
  let fixture: ComponentFixture<HomeComponent>;
  let mockOAuthService: jasmine.SpyObj<OAuthService>;
  let mockAuthService: { isAuthenticated: boolean; navigateToLogin: jasmine.Spy };

  beforeEach(
    waitForAsync(() => {
      mockOAuthService = jasmine.createSpyObj('OAuthService', ['hasValidAccessToken']);
      mockAuthService = {
        isAuthenticated: true,
        navigateToLogin: jasmine.createSpy('navigateToLogin')
      };

      TestBed.configureTestingModule({
        declarations: [],
        imports: [
          CoreTestingModule.withConfig(),
          ThemeSharedTestingModule.withConfig(),
          NgxValidateCoreModule,
          HomeComponent
        ],
        providers: [
          {
            provide: OAuthService,
            useValue: mockOAuthService
          },
          {
            provide: AuthService,
            useValue: mockAuthService
          }
        ],
      }).compileComponents();
    })
  );

  beforeEach(() => {
    fixture = TestBed.createComponent(HomeComponent);
    fixture.detectChanges();
  });

  it("should be initiated", () => {
    expect(fixture.componentInstance).toBeTruthy();
  });

  it("hasLoggedIn getter should return isAuthenticated from AuthService", () => {
    mockAuthService.isAuthenticated = true;
    expect(fixture.componentInstance.hasLoggedIn).toBeTrue();
  });

  it("login() method should trigger AuthService navigateToLogin", () => {
    fixture.componentInstance.login();
    expect(mockAuthService.navigateToLogin).toHaveBeenCalled();
  });
});
