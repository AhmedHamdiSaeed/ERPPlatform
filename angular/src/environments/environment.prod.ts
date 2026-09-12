import { Environment } from '@abp/ng.core';

// Split-host production config.
// - The SPA is served from Vercel (spaUrl).
// - The API / OpenIddict issuer lives on runasp.net (apiUrl).
// - CORS is configured on the backend to allow the Vercel origin.
const spaUrl = typeof window !== 'undefined' ? window.location.origin : 'https://erpplatform-one.vercel.app';
const apiUrl = 'https://erpplatform.runasp.net';

export const environment = {
  production: true,
  application: {
    baseUrl: spaUrl,
    name: 'ERPPlatform',
    logoUrl: '',
  },
  oAuthConfig: {
    issuer: apiUrl + '/',
    redirectUri: spaUrl,
    clientId: 'ERPPlatform_App',
    responseType: 'code',
    scope: 'offline_access ERPPlatform',
    requireHttps: true,
  },
  apis: {
    default: {
      url: apiUrl,
      rootNamespace: 'ERPPlatform',
    },
    abp: {
      url: apiUrl,
      rootNamespace: 'ERPPlatform',
    },
  },
} as Environment;
