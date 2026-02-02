"use client";

import { InlineSpinner } from "@/components/feedback/InlineSpinner";
import { RedisKeyType } from "@/lib/types";

type RedisKeysFiltersProps = {
  pattern: string;
  filterType: "all" | RedisKeyType;
  isLoading: boolean;
  typeFilters: Array<"all" | RedisKeyType>;
  onPatternChange: (value: string) => void;
  onFilterChange: (value: "all" | RedisKeyType) => void;
  onSearch: () => void;
  onAddKey: () => void;
};

export function RedisKeysFilters({
  pattern,
  filterType,
  isLoading,
  typeFilters,
  onPatternChange,
  onFilterChange,
  onSearch,
  onAddKey,
}: RedisKeysFiltersProps) {
  return (
    <div className="border-b border-border-dark p-4">
      <div className="flex flex-col gap-3">
        <label className="flex flex-col gap-2">
          <span className="text-xs font-semibold uppercase text-slate-500">
            Regex Search
          </span>
          <div className="flex h-10 items-center rounded-lg border border-border-dark bg-surface-dark px-3 text-sm text-slate-200 focus-within:border-primary">
            <span className="material-symbols-outlined text-[18px] text-slate-500">
              search
            </span>
            <input
              className="flex-1 bg-transparent px-3 text-sm font-mono text-slate-200 placeholder:text-slate-500 focus:ring-0"
              placeholder="Search keys..."
              value={pattern}
              onChange={(event) => onPatternChange(event.target.value)}
            />
            <span className="rounded bg-background px-1.5 py-0.5 text-[10px] font-bold text-slate-400">
              REGEX
            </span>
          </div>
        </label>
        <div className="flex flex-wrap items-center justify-between gap-3">
          <div className="flex flex-wrap gap-2">
            {typeFilters.map((type) => (
              <button
                key={type}
                className={`h-7 rounded px-3 text-[11px] font-bold uppercase transition-all ${
                  filterType === type
                    ? "bg-primary text-white"
                    : "bg-background text-slate-400 hover:text-white"
                }`}
                type="button"
                onClick={() => onFilterChange(type)}
              >
                {type}
              </button>
            ))}
          </div>
          <div className="ml-auto flex items-center gap-2">
            <button
              className="flex items-center gap-1 rounded bg-primary px-3 py-1 text-[11px] font-bold uppercase text-white"
              onClick={onSearch}
              type="button"
            >
              <span className="material-symbols-outlined text-[14px]">
                sync
              </span>
              Scan
            </button>
            <button
              className="flex items-center gap-1 rounded border border-border-dark bg-background px-3 py-1 text-[11px] font-bold uppercase text-slate-200 hover:border-primary"
              type="button"
              onClick={onAddKey}
            >
              <span className="material-symbols-outlined text-[14px]">
                add
              </span>
              Add Key
            </button>
          </div>
        </div>
      </div>
      {isLoading && (
        <div className="mt-3 flex items-center gap-2 text-xs text-slate-400">
          <InlineSpinner className="size-3 border-slate-300" />
          Loading keys...
        </div>
      )}
    </div>
  );
}
