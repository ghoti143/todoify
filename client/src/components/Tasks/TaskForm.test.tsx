import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, it, expect, vi, beforeEach } from "vitest";
import { TaskForm } from "./TaskForm";
import { createQueryWrapper } from "../../test/wrapper";
import { mockTask } from "../../test/fixtures";
import type { Task } from "../../types";

vi.mock("../../api/tasks", () => ({
  taskApi: {
    create: vi.fn(),
    update: vi.fn(),
  },
}));

import { taskApi } from "../../api/tasks";

const renderForm = (task?: Task, onDone = vi.fn()) => {
  const wrapper = createQueryWrapper();
  return render(<TaskForm task={task} onDone={onDone} />, { wrapper });
};

describe("TaskForm — create mode", () => {
  beforeEach(() => vi.clearAllMocks());

  it('renders "New task" heading in create mode', () => {
    renderForm();
    expect(screen.getByText("New task")).toBeInTheDocument();
  });

  it('submit button reads "Create task"', () => {
    renderForm();
    expect(
      screen.getByRole("button", { name: /create task/i }),
    ).toBeInTheDocument();
  });

  it("submit is disabled when title is empty", () => {
    renderForm();
    expect(screen.getByRole("button", { name: /create task/i })).toBeDisabled();
  });

  it("calls taskApi.create with form values on submit", async () => {
    (taskApi.create as ReturnType<typeof vi.fn>).mockResolvedValue(mockTask());
    renderForm();

    await userEvent.type(
      screen.getByPlaceholderText("What needs doing?"),
      "My new task",
    );
    await userEvent.type(
      screen.getByPlaceholderText("Optional details…"),
      "Some details",
    );
    await userEvent.click(screen.getByRole("button", { name: /create task/i }));

    expect(taskApi.create).toHaveBeenCalledWith(
      expect.objectContaining({
        title: "My new task",
        description: "Some details",
      }),
    );
  });

  it("keeps form open and shows error on create failure", async () => {
    (taskApi.create as ReturnType<typeof vi.fn>).mockRejectedValue(
      new Error("Server error"),
    );
    const onDone = vi.fn();
    renderForm(undefined, onDone);

    await userEvent.type(
      screen.getByPlaceholderText("What needs doing?"),
      "My task",
    );
    await userEvent.click(screen.getByRole("button", { name: /create task/i }));

    expect(await screen.findByText("Server error")).toBeInTheDocument();
    expect(onDone).not.toHaveBeenCalled();
  });

  it("calls onDone when cancel is clicked", async () => {
    const onDone = vi.fn();
    renderForm(undefined, onDone);
    await userEvent.click(screen.getByRole("button", { name: /cancel/i }));
    expect(onDone).toHaveBeenCalledOnce();
  });

  it("calls onDone when close button is clicked", async () => {
    const onDone = vi.fn();
    renderForm(undefined, onDone);
    await userEvent.click(screen.getByLabelText("Close"));
    expect(onDone).toHaveBeenCalledOnce();
  });
});

describe("TaskForm — edit mode", () => {
  beforeEach(() => vi.clearAllMocks());

  const task = mockTask({
    title: "Existing task",
    description: "Old description",
    priority: "High",
  });

  it('renders "Edit task" heading in edit mode', () => {
    renderForm(task);
    expect(screen.getByText("Edit task")).toBeInTheDocument();
  });

  it("pre-fills fields with existing task values", () => {
    renderForm(task);
    expect(screen.getByDisplayValue("Existing task")).toBeInTheDocument();
    expect(screen.getByDisplayValue("Old description")).toBeInTheDocument();
    expect(screen.getByDisplayValue("High")).toBeInTheDocument();
  });

  it('submit button reads "Save changes"', () => {
    renderForm(task);
    expect(
      screen.getByRole("button", { name: /save changes/i }),
    ).toBeInTheDocument();
  });

  it("calls taskApi.update with task id and updated values", async () => {
    (taskApi.update as ReturnType<typeof vi.fn>).mockResolvedValue(task);
    renderForm(task);

    const titleInput = screen.getByDisplayValue("Existing task");
    await userEvent.clear(titleInput);
    await userEvent.type(titleInput, "Updated task");
    await userEvent.click(
      screen.getByRole("button", { name: /save changes/i }),
    );

    expect(taskApi.update).toHaveBeenCalledWith(
      "task-1",
      expect.objectContaining({ title: "Updated task" }),
    );
  });

  it("keeps form open and shows error on update failure", async () => {
    (taskApi.update as ReturnType<typeof vi.fn>).mockRejectedValue(
      new Error("Update failed"),
    );
    const onDone = vi.fn();
    renderForm(task, onDone);

    await userEvent.click(
      screen.getByRole("button", { name: /save changes/i }),
    );

    expect(await screen.findByText("Update failed")).toBeInTheDocument();
    expect(onDone).not.toHaveBeenCalled();
  });
});
