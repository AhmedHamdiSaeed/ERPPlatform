import { Environment } from '@abp/ng.core';

const baseUrl = 'http://localhost:4200';

export const environment = {
  production: false,
  application: {
    baseUrl,
    name: 'ERPPlatform',
    logoUrl: '',
  },
  oAuthConfig: {
    issuer: 'https://localhost:44364/',
    redirectUri: baseUrl,
    clientId: 'ERPPlatform_App',
    responseType: 'code',
    scope: 'offline_access ERPPlatform',
    requireHttps: true,
  },
  apis: {
    default: {
      url: 'https://localhost:44364',
      rootNamespace: 'ERPPlatform',
    },
  },
} as Environment;
