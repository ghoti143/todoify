import type { Task } from "../types";

export const mockTask = (overrides: Partial<Task> = {}): Task => ({
  id: "task-1",
  title: "Test task",
  description: "A test description",
  isComplete: false,
  priority: "Medium",
  dueDate: undefined,
  createdAt: "2024-01-01T00:00:00Z",
  updatedAt: "2024-01-01T00:00:00Z",
  ...overrides,
});

export const mockTasks = (): Task[] => [
  mockTask({ id: "task-1", title: "First task", priority: "High" }),
  mockTask({
    id: "task-2",
    title: "Second task",
    priority: "Low",
    isComplete: true,
  }),
  mockTask({
    id: "task-3",
    title: "Third task",
    priority: "Medium",
    dueDate: "2020-01-01",
  }),
];
