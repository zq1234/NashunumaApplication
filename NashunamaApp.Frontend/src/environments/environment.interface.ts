// src/environments/environment.interface.ts
export interface Environment {
  production: boolean;
  apiUrl: string;
  apiVersion: string;
  appName: string;
  appVersion: string;
  
  auth: {
    tokenKey: string;
    userKey: string;
    refreshTokenKey: string;
    tokenExpiryKey: string;
    loginUrl: string;
    logoutUrl: string;
    registerUrl: string;
    changePasswordUrl: string;
    resetPasswordUrl: string;
    validateTokenUrl: string;
    refreshTokenUrl: string;
  };
  
  // Generic API endpoints
  api: {
    [key: string]: string; // Dynamic endpoints
  };
  
  features: {
    enableSwagger: boolean;
    enableDebug: boolean;
    enableLogging: boolean;
    enableAnalytics: boolean;
  };
  
  pagination: {
    defaultPageSize: number;
    pageSizeOptions: number[];
  };
  
  timeout: {
    apiTimeout: number;
    retryAttempts: number;
  };
  
  storage: {
    prefix: string;
  };
}