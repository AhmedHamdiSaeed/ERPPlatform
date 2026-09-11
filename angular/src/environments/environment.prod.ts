import { Environment } from '@abp/ng.core';

// Same-origin production config. The SPA is served from the SAME origin as the API
// (https://erpplatform.runasp.net), so the SPA's own origin, the issuer, and the API base URL
// are all identical. No CORS, no redirect-URI, and no separate-host configuration is needed.
const baseUrl = 'https://erpplatform.runasp.net';

export const environment = {
  production: true,
  application: {
    baseUrl,
    name: 'ERPPlatform',
    logoUrl: '',
  },
  oAuthConfig: {
    issuer: baseUrl + '/',
    redirectUri: baseUrl,
    clientId: 'ERPPlatform_App',
    responseType: 'code',
    scope: 'offline_access ERPPlatform',
    requireHttps: true
  },
  apis: {
    default: {
      url: baseUrl,
      rootNamespace: 'ERPPlatform',
    },
  },
} as Environment;
