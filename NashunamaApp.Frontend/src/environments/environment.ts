// src/environments/environment.ts
import { Environment } from './environment.interface';

export const environment: Environment = {
  production: false,
  apiUrl: 'https://localhost:7058',
  apiVersion: 'v1',
  appName: 'Nashunuma App',
  appVersion: '1.0.0-dev',

  auth: {
    tokenKey: 'auth_token',
    userKey: 'user_info',
    refreshTokenKey: 'refresh_token',
    tokenExpiryKey: 'token_expiry',
    loginUrl: '/api/Auth/login',
    logoutUrl: '/api/Auth/logout',
    registerUrl: '/api/Auth/register',
    changePasswordUrl: '/api/Auth/change-password',
    resetPasswordUrl: '/api/Auth/reset-password',
    validateTokenUrl: '/api/Auth/validate-token',
    refreshTokenUrl: '/api/Auth/refresh-token'
  },

  // Generic API endpoints
  api: {
    // User Management
    'user.getUsers': '/api/UserManagement/users',
    'user.getByUsername': '/api/UserManagement/users/{username}',
    'user.transferLocation': '/api/UserManagement/users/{username}/location',
    'user.toggleStatus': '/api/UserManagement/users/{username}/status',
    'user.blockUser': '/api/UserManagement/users/{username}/block',
     'user.unblockUser': '/api/UserManagement/users/{username}/unblock',
    'user.getByLocation': '/api/UserManagement/users/by-location',
    'user.getStatistics': '/api/UserManagement/statistics/location',
    
    
    // Food Stock
    'foodStock.getList': '/api/FoodStock',
    'foodStock.getById': '/api/FoodStock/{id}',
    'foodStock.create': '/api/FoodStock',
    'foodStock.update': '/api/FoodStock/{id}',
    'foodStock.delete': '/api/FoodStock/{id}',
    'foodStock.export': '/api/FoodStock/export',
    'foodStock.getSummary': '/api/FoodStock/summary',
    
    // Location Management
    'location.getProvinces': '/api/Location/provinces',
    'location.getDistrictsByProvince': '/api/Location/districts/{provinceName}',
    'location.getTehsilsByDistrict': '/api/Location/tehsils/{districtName}',
    'location.getUcsByTehsil': '/api/Location/ucs/{tehsilName}',
    'location.getHierarchy': '/api/Location/hierarchy',
    'location.getProvinceByName': '/api/Location/province/{provinceName}',
    'location.getDistrictByName': '/api/Location/district/{districtName}',
    'location.getTehsilByName': '/api/Location/tehsil/{tehsilName}',
    
    // Add more as needed
  },

  features: {
    enableSwagger: true,
    enableDebug: true,
    enableLogging: true,
    enableAnalytics: false
  },

  pagination: {
    defaultPageSize: 10,
    pageSizeOptions: [5, 10, 25, 50, 100]
  },

  timeout: {
    apiTimeout: 30000,
    retryAttempts: 3
  },

  storage: {
    prefix: 'nashunuma_'
  }
};