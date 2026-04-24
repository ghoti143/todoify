import { render, screen } from "@testing-library/react";
import userEvent from "@testing-library/user-event";
import { describe, it, expect, vi, beforeEach } from "vitest";
import { TaskCard } from "./TaskCard";
import { createQueryWrapper } from "../../test/wrapper";
import { mockTask } from "../../test/fixtures";

vi.mock("../../api/tasks", () => ({
  taskApi: {
    toggleComplete: vi.fn(),
    delete: vi.fn(),
  },
}));

import { taskApi } from "../../api/tasks";

const renderCard = (overrides = {}, onEdit = vi.fn()) => {
  const wrapper = createQueryWrapper();
  return render(<TaskCard task={mockTask(overrides)} onEdit={onEdit} />, {
    wrapper,
  });
};

describe("TaskCard", () => {
  beforeEach(() => {
    vi.clearAllMocks();
  });

  it("renders task title, priority and description", () => {
    renderCard();
    expect(screen.getByText("Test task")).toBeInTheDocument();
    expect(screen.getByText("Medium")).toBeInTheDocument();
    expect(screen.getByText("A test description")).toBeInTheDocument();
  });

  it("renders overdue date with warning when past due", () => {
    renderCard({ dueDate: "2020-01-01", isComplete: false });
    expect(screen.getByText(/jan 1/i)).toBeInTheDocument();
    expect(screen.getByText(/⚠/)).toBeInTheDocument();
  });

  it("does not show overdue warning on completed tasks", () => {
    renderCard({ dueDate: "2020-01-01", isComplete: true });
    expect(screen.queryByText(/⚠/)).not.toBeInTheDocument();
  });

  it("does not render description when absent", () => {
    renderCard({ description: undefined });
    expect(screen.queryByText("A test description")).not.toBeInTheDocument();
  });

  it("calls toggleComplete when checkbox is clicked", async () => {
    (taskApi.toggleComplete as ReturnType<typeof vi.fn>).mockResolvedValue({});
    renderCard();
    await userEvent.click(screen.getByLabelText("Mark complete"));
    expect(taskApi.toggleComplete).toHaveBeenCalledWith("task-1");
  });

  it("shows confirm buttons after delete is clicked", async () => {
    renderCard();
    await userEvent.click(screen.getByLabelText("Delete task"));
    expect(screen.getByRole("button", { name: /yes/i })).toBeInTheDocument();
    expect(screen.getByRole("button", { name: /no/i })).toBeInTheDocument();
  });

  it("cancels delete when No is clicked", async () => {
    renderCard();
    await userEvent.click(screen.getByLabelText("Delete task"));
    await userEvent.click(screen.getByRole("button", { name: /no/i }));
    expect(screen.getByLabelText("Delete task")).toBeInTheDocument();
    expect(
      screen.queryByRole("button", { name: /yes/i }),
    ).not.toBeInTheDocument();
  });

  it("calls delete when Yes is confirmed", async () => {
    (taskApi.delete as ReturnType<typeof vi.fn>).mockResolvedValue(undefined);
    renderCard();
    await userEvent.click(screen.getByLabelText("Delete task"));
    await userEvent.click(screen.getByRole("button", { name: /yes/i }));
    expect(taskApi.delete).toHaveBeenCalledWith("task-1");
  });

  it("calls onEdit with the task when edit is clicked", async () => {
    const onEdit = vi.fn();
    const task = mockTask();
    const wrapper = createQueryWrapper();
    render(<TaskCard task={task} onEdit={onEdit} />, { wrapper });

    await userEvent.click(screen.getByLabelText("Edit task"));
    expect(onEdit).toHaveBeenCalledWith(task);
  });

  it("applies reduced opacity when task is complete", () => {
    const { container } = renderCard({ isComplete: true });
    expect(container.firstChild).toHaveClass("opacity-50");
  });
});
