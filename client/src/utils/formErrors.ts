// utils/formErrors.ts
import axios from "axios";

// Type for your RFC 9110 error response
export interface ValidationErrorResponse {
  title?: string;
  status?: number;
  errors?: Record<string, string[]>;
}

/**
 * Extract field errors from an Axios error (RFC 9110 Problem Details format)
 */
export const getFieldErrorsFromAxiosError = (
  err: unknown,
): Record<string, string[]> | null => {
  if (!axios.isAxiosError(err)) return null;
  const data = err.response?.data as Partial<ValidationErrorResponse>;
  return data?.errors && typeof data.errors === "object" ? data.errors : null;
};

/**
 * Get the error message for a field (case-insensitive key matching)
 */
export const getFieldError = (
  fieldErrors: Record<string, string[]>,
  fieldName: string, // camelCase, e.g. "title", "dueDate"
): string | null => {
  const key = Object.keys(fieldErrors).find(
    (k) => k.toLowerCase() === fieldName.toLowerCase(),
  );
  return key ? (fieldErrors[key]?.[0] ?? null) : null;
};

/**
 * Create an onChange handler that updates state AND clears the field error
 */
export const createFieldChangeHandler = <T extends string | number>(
  fieldName: string, // camelCase
  setter: (value: T) => void,
  setFieldErrors: React.Dispatch<
    React.SetStateAction<Record<string, string[]>>
  >,
) => {
  return (value: T) => {
    setter(value);
    // Clear error for this field (case-insensitive)
    setFieldErrors((prev) => {
      const key = Object.keys(prev).find(
        (k) => k.toLowerCase() === fieldName.toLowerCase(),
      );
      if (!key) return prev;

      const next = { ...prev };
      delete next[key];
      return next;
    });
  };
};
