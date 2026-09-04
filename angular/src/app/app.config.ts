import { ApplicationConfig, importProvidersFrom, inject, provideAppInitializer } from '@angular/core';
import { provideRouter } from '@angular/router';
import { provideAnimations } from '@angular/platform-browser/animations';
import { provideHttpClient, withInterceptors } from '@angular/common/http';
import { CoreModule } from '@abp/ng.core';
import { ThemeSharedModule } from '@abp/ng.theme.shared';
import { appRoutes } from './app.routes';
import { authInterceptorFn } from './core/interceptors/auth.interceptor';
import { SessionTimeoutService } from './core/services/session-timeout.service';
import { TranslationService } from './core/services/translation.service';
import { environment } from '../environments/environment';

export const appConfig: ApplicationConfig = {
  providers: [
    importProvidersFrom(
      CoreModule.forRoot({
        environment,
        registerLocaleFn: () => Promise.resolve()
      })
    ),
    importProvidersFrom(ThemeSharedModule.forRoot()),
    provideRouter(appRoutes),
    provideAnimations(),
    provideHttpClient(withInterceptors([authInterceptorFn])),
    // Restores the 3h session clock, watches user activity and keeps tabs in sync.
    provideAppInitializer(() => inject(SessionTimeoutService).init()),
    // Starts the intelligent DOM auto-translator across all pages and tables.
    provideAppInitializer(() => inject(TranslationService).init()),
  ],
};
