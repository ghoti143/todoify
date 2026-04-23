import { api } from "./client";
import type { Task, CreateTaskRequest } from "../types";

export const taskApi = {
  getAll: (params?: Record<string, string>) =>
    api.get<Task[]>("/tasks", { params }).then((r) => r.data),
  getById: (id: string) => api.get<Task>(`/tasks/${id}`).then((r) => r.data),
  create: (data: CreateTaskRequest) =>
    api.post<Task>("/tasks", data).then((r) => r.data),
  update: (id: string, data: Partial<CreateTaskRequest>) =>
    api.put<Task>(`/tasks/${id}`, data).then((r) => r.data),
  toggleComplete: (id: string) =>
    api.patch<Task>(`/tasks/${id}/complete`).then((r) => r.data),
  delete: (id: string) => api.delete(`/tasks/${id}`),
};
