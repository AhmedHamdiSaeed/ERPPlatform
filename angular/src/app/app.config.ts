import { ApplicationConfig, inject, provideAppInitializer } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideAnimations } from '@angular/platform-browser/animations';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { appRoutes } from './app.routes';
import { authInterceptorFn } from './core/interceptors/auth.interceptor';
import { SessionTimeoutService } from './core/services/session-timeout.service';

export const appConfig: ApplicationConfig = {
  providers: [
    provideRouter(appRoutes),
    provideAnimations(),
    provideHttpClient(withInterceptors([authInterceptorFn])),
    // Restores the 3h session clock, watches user activity and keeps tabs in sync.
    provideAppInitializer(() => inject(SessionTimeoutService).init()),
  ],
};
