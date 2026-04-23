import type { Priority } from "../../types";

export type StatusFilter = "all" | "active" | "complete";

interface FilterBarProps {
  status: StatusFilter;
  priority: Priority | "all";
  onStatusChange: (s: StatusFilter) => void;
  onPriorityChange: (p: Priority | "all") => void;
  total: number;
}

const STATUS_OPTIONS: { value: StatusFilter; label: string }[] = [
  { value: "all", label: "All" },
  { value: "active", label: "Active" },
  { value: "complete", label: "Done" },
];

const PRIORITY_OPTIONS: { value: Priority | "all"; label: string }[] = [
  { value: "all", label: "Any priority" },
  { value: "High", label: "High" },
  { value: "Medium", label: "Medium" },
  { value: "Low", label: "Low" },
];

export const FilterBar = ({
  status,
  priority,
  onStatusChange,
  onPriorityChange,
  total,
}: FilterBarProps) => (
  <div className="flex items-center justify-between gap-3 pb-4 mb-1 border-b border-slate-800 flex-wrap font-mono">
    <div className="flex gap-1">
      {STATUS_OPTIONS.map((opt) => (
        <button
          key={opt.value}
          onClick={() => onStatusChange(opt.value)}
          className={`px-3 py-1.5 rounded border text-[10px] font-bold uppercase tracking-wider transition-colors ${
            status === opt.value
              ? "bg-amber-400 border-amber-400 text-slate-900"
              : "border-slate-700 text-slate-500 hover:border-amber-400 hover:text-amber-400"
          }`}
        >
          {opt.label}
        </button>
      ))}
    </div>

    <div className="flex items-center gap-3">
      <select
        value={priority}
        onChange={(e) => onPriorityChange(e.target.value as Priority | "all")}
        aria-label="Filter by priority"
        className="bg-[#1a1d27] border border-slate-700 focus:border-amber-400 rounded px-2.5 py-1.5 text-slate-400 text-[10px] uppercase tracking-wider outline-none transition-colors cursor-pointer font-mono"
      >
        {PRIORITY_OPTIONS.map((opt) => (
          <option key={opt.value} value={opt.value}>
            {opt.label}
          </option>
        ))}
      </select>
      <span className="text-[10px] text-slate-600 uppercase tracking-wider whitespace-nowrap">
        {total} task{total !== 1 ? "s" : ""}
      </span>
    </div>
  </div>
);
