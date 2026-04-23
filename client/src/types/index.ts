export type Priority = "Low" | "Medium" | "High";

export interface Task {
  id: string;
  title: string;
  description?: string;
  isComplete: boolean;
  priority: Priority;
  dueDate?: string; // "YYYY-MM-DD"
  createdAt: string;
  updatedAt: string;
}

export interface AuthResponse {
  token: string;
  email: string;
  displayName?: string;
  expiresAt: string;
}

export interface CreateTaskRequest {
  title: string;
  description?: string;
  priority: Priority;
  dueDate?: string;
}
