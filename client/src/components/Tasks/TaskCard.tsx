import { useState } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { taskApi } from "../../api/tasks";
import type { Task } from "../../types";

interface TaskCardProps {
  task: Task;
  onEdit: (task: Task) => void;
}

const PRIORITY_STYLES: Record<Task["priority"], string> = {
  Low: "bg-slate-800 text-slate-400",
  Medium: "bg-amber-950/50 text-amber-400",
  High: "bg-red-950/50 text-red-400",
};

export const TaskCard = ({ task, onEdit }: TaskCardProps) => {
  const qc = useQueryClient();
  const [confirmDelete, setConfirmDelete] = useState(false);

  const toggleMutation = useMutation({
    mutationFn: () => taskApi.toggleComplete(task.id),
    onMutate: async () => {
      await qc.cancelQueries({ queryKey: ["tasks"] });
      const prev = qc.getQueryData<Task[]>(["tasks"]);
      qc.setQueryData<Task[]>(["tasks"], (old) =>
        old?.map((t) =>
          t.id === task.id ? { ...t, isComplete: !t.isComplete } : t,
        ),
      );
      return { prev };
    },
    onError: (_err, _vars, ctx) => {
      if (ctx?.prev) qc.setQueryData(["tasks"], ctx.prev);
    },
    onSettled: () => qc.invalidateQueries({ queryKey: ["tasks"] }),
  });

  const deleteMutation = useMutation({
    mutationFn: () => taskApi.delete(task.id),
    onSuccess: () => qc.invalidateQueries({ queryKey: ["tasks"] }),
  });

  const dueDate = task.dueDate
    ? new Date(task.dueDate).toLocaleDateString("en-US", {
        month: "short",
        day: "numeric",
      })
    : null;
  const isOverdue =
    task.dueDate && !task.isComplete && new Date(task.dueDate) < new Date();

  return (
    <div
      className={`flex items-start gap-3 px-4 py-3.5 bg-[#0f1117] border border-slate-800 hover:border-slate-700 rounded transition-colors font-mono ${task.isComplete ? "opacity-50" : ""}`}
    >
      {/* Checkbox */}
      <button
        onClick={() => toggleMutation.mutate()}
        disabled={toggleMutation.isPending}
        aria-label={task.isComplete ? "Mark incomplete" : "Mark complete"}
        className={`mt-0.5 w-4.5 h-4.5 flex-shrink-0 flex items-center justify-center rounded border transition-colors ${
          task.isComplete
            ? "bg-amber-400 border-amber-400 text-slate-900"
            : "border-slate-600 hover:border-amber-400 bg-transparent"
        }`}
        style={{ width: 18, height: 18 }}
      >
        {task.isComplete && (
          <span className="text-[10px] font-bold leading-none">✓</span>
        )}
      </button>

      {/* Body */}
      <div className="flex-1 min-w-0">
        <div className="flex items-baseline gap-2 mb-0.5">
          <span
            className={`text-sm ${task.isComplete ? "line-through text-slate-500" : "text-slate-100"}`}
          >
            {task.title}
          </span>
          <span
            className={`flex-shrink-0 text-[10px] font-bold uppercase tracking-wider px-1.5 py-0.5 rounded ${PRIORITY_STYLES[task.priority]}`}
          >
            {task.priority}
          </span>
        </div>

        {task.description && (
          <p className="text-xs text-slate-500 leading-relaxed mb-1">
            {task.description}
          </p>
        )}

        {dueDate && (
          <span
            className={`text-[10px] uppercase tracking-wider ${isOverdue ? "text-red-400" : "text-slate-600"}`}
          >
            {isOverdue ? "⚠ " : ""}
            {dueDate}
          </span>
        )}
      </div>

      {/* Actions */}
      <div className="flex items-center gap-1 flex-shrink-0">
        <button
          onClick={() => onEdit(task)}
          aria-label="Edit task"
          className="w-7 h-7 flex items-center justify-center border border-slate-700 hover:border-amber-400 hover:text-amber-400 text-slate-500 rounded text-sm transition-colors"
        >
          ✎
        </button>

        {confirmDelete ? (
          <>
            <button
              onClick={() => deleteMutation.mutate()}
              disabled={deleteMutation.isPending}
              className="h-7 px-2 flex items-center border border-red-800 hover:border-red-500 text-red-400 rounded text-[10px] font-bold uppercase tracking-wider transition-colors disabled:opacity-50"
            >
              {deleteMutation.isPending ? "…" : "Yes"}
            </button>
            <button
              onClick={() => setConfirmDelete(false)}
              className="h-7 px-2 flex items-center border border-slate-700 hover:border-slate-500 text-slate-500 rounded text-[10px] font-bold uppercase tracking-wider transition-colors"
            >
              No
            </button>
          </>
        ) : (
          <button
            onClick={() => setConfirmDelete(true)}
            aria-label="Delete task"
            className="w-7 h-7 flex items-center justify-center border border-slate-700 hover:border-red-700 hover:text-red-400 text-slate-500 rounded text-sm transition-colors"
          >
            ✕
          </button>
        )}
      </div>
    </div>
  );
};
