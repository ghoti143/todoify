import { useState } from "react";
import { useQuery } from "@tanstack/react-query";
import { taskApi } from "../../api/tasks";
import type { Task, Priority } from "../../types";
import { TaskCard } from "./TaskCard";
import { TaskForm } from "./TaskForm";
import { FilterBar } from "./FilterBar";
import type { StatusFilter } from "./FilterBar";

export const TaskList = () => {
  const [status, setStatus] = useState<StatusFilter>("all");
  const [priority, setPriority] = useState<Priority | "all">("all");
  const [editTask, setEditTask] = useState<Task | null>(null);
  const [showCreate, setShowCreate] = useState(false);

  const params: Record<string, string> = {};
  if (status !== "all") params.status = status;
  if (priority !== "all") params.priority = priority;

  const { data, isLoading, isError, refetch } = useQuery({
    queryKey: ["tasks", params],
    queryFn: () => taskApi.getAll(params),
  });

  const filtered = (data ?? [])
    .filter(
      (t) =>
        status === "all" ||
        (status === "active" ? !t.isComplete : t.isComplete),
    )
    .filter((t) => priority === "all" || t.priority === priority);

  const closeModal = () => {
    setShowCreate(false);
    setEditTask(null);
  };

  return (
    <div className="max-w-2xl mx-auto font-mono">
      {/* Top bar */}
      <div className="flex items-center justify-between mb-5">
        <span className="text-slate-400 text-xs uppercase tracking-widest">
          {new Date().toLocaleDateString("en-US", {
            weekday: "long",
            month: "long",
            day: "numeric",
          })}
        </span>
        <button
          onClick={() => setShowCreate(true)}
          className="px-4 py-2 bg-amber-400 hover:bg-amber-300 text-slate-900 text-xs font-bold uppercase tracking-wider rounded transition-colors"
        >
          + New task
        </button>
      </div>

      <FilterBar
        status={status}
        priority={priority}
        onStatusChange={setStatus}
        onPriorityChange={setPriority}
        total={filtered.length}
      />

      {/* Task list */}
      <div className="flex flex-col gap-2 mt-4">
        {isLoading &&
          [...Array(4)].map((_, i) => (
            <div
              key={i}
              className="h-16 rounded bg-slate-800/50 animate-pulse"
              style={{ animationDelay: `${i * 80}ms` }}
            />
          ))}

        {isError && (
          <div className="flex flex-col items-center gap-3 py-16 text-center">
            <span className="text-2xl text-slate-700">⚠</span>
            <p className="text-slate-500 text-xs uppercase tracking-wider">
              Failed to load tasks
            </p>
            <button
              onClick={() => refetch()}
              className="px-4 py-2 border border-red-800 hover:border-red-600 text-red-400 rounded text-xs uppercase tracking-wider transition-colors"
            >
              Retry
            </button>
          </div>
        )}

        {!isLoading && !isError && filtered.length === 0 && (
          <div className="flex flex-col items-center gap-3 py-16 text-center">
            <span className="text-3xl text-slate-800">◻</span>
            <p className="text-slate-500 text-xs uppercase tracking-wider">
              {status !== "all" || priority !== "all"
                ? "No tasks match your filters"
                : "Nothing here yet"}
            </p>
            {status === "all" && priority === "all" && (
              <button
                onClick={() => setShowCreate(true)}
                className="mt-1 px-5 py-2 bg-amber-400 hover:bg-amber-300 text-slate-900 rounded text-xs font-bold uppercase tracking-wider transition-colors"
              >
                + Create your first task
              </button>
            )}
          </div>
        )}

        {!isLoading &&
          !isError &&
          filtered.map((task) => (
            <TaskCard
              key={task.id}
              task={task}
              onEdit={(t) => setEditTask(t)}
            />
          ))}
      </div>

      {/* Modal */}
      {(showCreate || editTask) && (
        <div
          className="fixed inset-0 bg-black/70 flex items-center justify-center z-50 p-4"
          onClick={closeModal}
        >
          <div
            className="w-full max-w-md bg-[#0f1117] border border-slate-800 rounded"
            onClick={(e) => e.stopPropagation()}
          >
            <TaskForm task={editTask ?? undefined} onDone={closeModal} />
          </div>
        </div>
      )}
    </div>
  );
};
