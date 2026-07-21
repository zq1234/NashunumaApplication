// src/app/core/models/api-response.model.ts

/**
 * Generic API Response Wrapper
 */
export interface ApiResponse<T> {
  isSuccess: boolean;
  message: string;
  data: T;
  errors: string[] | null;
  statusCode: number;
}

/**
 * Paginated Response Wrapper
 */
export interface PaginatedResponse<T> {
  items: T[];
  pageNumber: number;
  pageSize: number;
  totalCount: number;
  totalPages: number;
  hasPrevious: boolean;
  hasNext: boolean;
}

/**
 * API Error Response
 */
export interface ApiErrorResponse {
  isSuccess: false;
  message: string;
  errors: string[];
  statusCode: number;
  data: null;
}

/**
 * API Validation Error Response
 */
export interface ApiValidationError {
  field: string;
  message: string;
}

/**
 * API Response with Validation Errors
 */
export interface ApiValidationResponse<T> extends ApiResponse<T> {
  validationErrors?: ApiValidationError[];
}

/**
 * Empty API Response for operations that return no data
 */
export interface ApiEmptyResponse extends ApiResponse<null> {}

/**
 * API Response with file data (for downloads)
 */
export interface ApiFileResponse {
  data: Blob;
  fileName: string;
  contentType: string;
}

// ============================================
// Helper Functions for API Responses
// ============================================

export class ApiResponseHelper {
  /**
   * Check if response is successful
   */
  static isSuccess<T>(response: ApiResponse<T>): boolean {
    return response.isSuccess && response.statusCode >= 200 && response.statusCode < 300;
  }

  /**
   * Get error message from response
   */
  static getErrorMessage<T>(response: ApiResponse<T>): string {
    if (response.errors && response.errors.length > 0) {
      return response.errors.join(', ');
    }
    return response.message || 'An error occurred';
  }

  /**
   * Check if response has validation errors
   */
  static hasValidationErrors<T>(response: ApiValidationResponse<T>): boolean {
    return !!(response.validationErrors && response.validationErrors.length > 0);
  }

  /**
   * Get validation errors as object
   */
  static getValidationErrors<T>(response: ApiValidationResponse<T>): { [key: string]: string[] } {
    const errors: { [key: string]: string[] } = {};
    if (response.validationErrors) {
      response.validationErrors.forEach(error => {
        if (!errors[error.field]) {
          errors[error.field] = [];
        }
        errors[error.field].push(error.message);
      });
    }
    return errors;
  }

  /**
   * Create a success response
   */
  static createSuccess<T>(data: T, message: string = 'Success'): ApiResponse<T> {
    return {
      isSuccess: true,
      message,
      data,
      errors: null,
      statusCode: 200
    };
  }

  /**
   * Create an error response
   */
  static createError<T>(message: string, errors?: string[], statusCode: number = 400): ApiResponse<T> {
    return {
      isSuccess: false,
      message,
      data: null as any,
      errors: errors || [message],
      statusCode
    };
  }
}

/**
 * API Response Status Codes
 */
export const ApiStatusCodes = {
  OK: 200,
  CREATED: 201,
  ACCEPTED: 202,
  NO_CONTENT: 204,
  BAD_REQUEST: 400,
  UNAUTHORIZED: 401,
  FORBIDDEN: 403,
  NOT_FOUND: 404,
  CONFLICT: 409,
  INTERNAL_SERVER_ERROR: 500,
  SERVICE_UNAVAILABLE: 503
} as const;

export type ApiStatusCode = typeof ApiStatusCodes[keyof typeof ApiStatusCodes];