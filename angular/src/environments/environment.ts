import { Environment } from '@abp/ng.core';

const baseUrl = 'http://localhost:4200';
const apiUrl = 'https://localhost:44327';

export const environment = {
  production: false,
  application: {
    baseUrl,
    name: 'ERPPlatform',
    logoUrl: '',
  },
  oAuthConfig: {
    issuer: `${apiUrl}/`,
    redirectUri: baseUrl,
    clientId: 'ERPPlatform_App',
    responseType: 'code',
    scope: 'offline_access ERPPlatform',
    requireHttps: false,
  },
  apis: {
    default: {
      url: apiUrl,
      rootNamespace: 'ERPPlatform',
    },
  },
} as Environment;
