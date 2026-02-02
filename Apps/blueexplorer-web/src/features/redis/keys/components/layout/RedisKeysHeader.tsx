"use client";

import { InlineSpinner } from "@/components/feedback/InlineSpinner";

type RedisKeysHeaderProps = {
  connectionName: string;
  isLoading: boolean;
  onRefresh: () => void;
  onServerInfo: () => void;
};

export function RedisKeysHeader({
  connectionName,
  isLoading,
  onRefresh,
  onServerInfo,
}: RedisKeysHeaderProps) {
  return (
    <div className="mb-4 flex flex-wrap items-center justify-between gap-3">
      <div>
        <h2 className="text-2xl font-bold text-slate-100">Keys Browser</h2>
        <p className="text-sm text-slate-400">
          Explore keys for {connectionName} with advanced filters.
        </p>
      </div>
      <div className="flex items-center gap-3">
        <button
          className="flex items-center gap-2 rounded-lg border border-border-dark bg-surface-dark px-4 py-2 text-sm font-semibold text-slate-100 transition-colors hover:border-primary"
          type="button"
          onClick={onServerInfo}
        >
          <span className="material-symbols-outlined text-[18px]">info</span>
          Server Info
        </button>
        <button
          className="flex items-center gap-2 rounded-lg border border-border-dark bg-surface-dark px-3 py-2 text-sm font-medium transition-colors hover:border-primary disabled:cursor-not-allowed disabled:opacity-60"
          onClick={onRefresh}
          type="button"
          disabled={isLoading}
        >
          {isLoading ? (
            <InlineSpinner className="size-4 border-slate-300" />
          ) : (
            <span className="material-symbols-outlined text-[18px]">
              refresh
            </span>
          )}
          {isLoading ? "Refreshing" : "Refresh"}
        </button>
      </div>
    </div>
  );
}
