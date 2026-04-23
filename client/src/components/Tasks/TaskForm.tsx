import { useState } from "react";
import { useMutation, useQueryClient } from "@tanstack/react-query";
import { taskApi } from "../../api/tasks";
import type { Task, Priority } from "../../types";

interface TaskFormProps {
  task?: Task;
  onDone: () => void;
}

const PRIORITIES: Priority[] = ["Low", "Medium", "High"];

const inputClass =
  "bg-[#1a1d27] border border-slate-700 rounded px-3 py-2.5 text-slate-100 text-sm placeholder-slate-600 outline-none focus:border-amber-400 transition-colors font-mono w-full";
const labelClass = "flex flex-col gap-1.5";
const labelTextClass = "text-slate-500 text-[10px] uppercase tracking-widest";

export const TaskForm = ({ task, onDone }: TaskFormProps) => {
  const qc = useQueryClient();
  const isEdit = Boolean(task);

  const [title, setTitle] = useState(task?.title ?? "");
  const [description, setDescription] = useState(task?.description ?? "");
  const [priority, setPriority] = useState<Priority>(
    task?.priority ?? "Medium",
  );
  const [dueDate, setDueDate] = useState(task?.dueDate ?? "");
  const [error, setError] = useState<string | null>(null);

  const mutation = useMutation({
    mutationFn: () =>
      isEdit && task
        ? taskApi.update(task.id, {
            title,
            description: description || undefined,
            priority,
            dueDate: dueDate || undefined,
          })
        : taskApi.create({
            title,
            description: description || undefined,
            priority,
            dueDate: dueDate || undefined,
          }),
    onSuccess: () => {
      qc.invalidateQueries({ queryKey: ["tasks"] });
      onDone();
    },
    onError: (err: unknown) => {
      setError(
        err instanceof Error
          ? err.message
          : "Something went wrong. Please try again.",
      );
    },
  });

  const handleSubmit = (e: React.FormEvent) => {
    e.preventDefault();
    setError(null);
    mutation.mutate();
  };

  return (
    <div className="font-mono">
      <div className="flex items-center justify-between px-6 pt-5 pb-0">
        <h2 className="text-slate-100 text-xs font-bold uppercase tracking-widest">
          {isEdit ? "Edit task" : "New task"}
        </h2>
        <button
          onClick={onDone}
          aria-label="Close"
          className="text-slate-500 hover:text-slate-200 transition-colors text-sm px-1"
        >
          ✕
        </button>
      </div>

      <form onSubmit={handleSubmit} className="flex flex-col gap-4 p-6">
        {error && (
          <div className="px-4 py-3 bg-red-950/50 border border-red-800/50 rounded text-red-300 text-xs leading-relaxed">
            {error}
          </div>
        )}

        <label className={labelClass}>
          <span className={labelTextClass}>Title *</span>
          <input
            type="text"
            value={title}
            onChange={(e) => setTitle(e.target.value)}
            placeholder="What needs doing?"
            required
            autoFocus
            className={inputClass}
          />
        </label>

        <label className={labelClass}>
          <span className={labelTextClass}>Description</span>
          <textarea
            value={description}
            onChange={(e) => setDescription(e.target.value)}
            placeholder="Optional details…"
            rows={3}
            className={`${inputClass} resize-none`}
          />
        </label>

        <div className="grid grid-cols-2 gap-3">
          <label className={labelClass}>
            <span className={labelTextClass}>Priority</span>
            <select
              value={priority}
              onChange={(e) => setPriority(e.target.value as Priority)}
              className={inputClass}
            >
              {PRIORITIES.map((p) => (
                <option key={p} value={p}>
                  {p}
                </option>
              ))}
            </select>
          </label>

          <label className={labelClass}>
            <span className={labelTextClass}>Due date</span>
            <input
              type="date"
              value={dueDate}
              onChange={(e) => setDueDate(e.target.value)}
              className={inputClass}
            />
          </label>
        </div>

        <div className="flex justify-end gap-2 mt-1">
          <button
            type="button"
            onClick={onDone}
            className="px-4 py-2 border border-slate-700 hover:border-slate-500 text-slate-500 hover:text-slate-300 rounded text-xs uppercase tracking-wider transition-colors"
          >
            Cancel
          </button>
          <button
            type="submit"
            disabled={mutation.isPending || !title.trim()}
            className="px-5 py-2 bg-amber-400 hover:bg-amber-300 disabled:opacity-40 disabled:cursor-not-allowed text-slate-900 rounded text-xs font-bold uppercase tracking-wider transition-colors flex items-center justify-center min-w-[110px]"
          >
            {mutation.isPending ? (
              <span className="w-3.5 h-3.5 border-2 border-slate-900/20 border-t-slate-900 rounded-full animate-spin" />
            ) : isEdit ? (
              "Save changes"
            ) : (
              "Create task"
            )}
          </button>
        </div>
      </form>
    </div>
  );
};
