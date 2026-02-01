"use client";

import { Button } from "@/components/Button";
import { InlineSpinner } from "@/components/feedback/InlineSpinner";
import { Input } from "@/components/Input";
import { useAsyncTasks } from "@/lib/async/AsyncContext";

export function Topbar() {
  const asyncTasks = useAsyncTasks();

  return (
    <header className="flex h-16 items-center justify-between border-b border-border-dark bg-background px-6 lg:px-8">
      <div className="flex flex-1 items-center gap-6">
        <div className="relative w-full max-w-md">
          <span className="material-symbols-outlined absolute left-3 top-1/2 -translate-y-1/2 text-[20px] text-slate-400">
            search
          </span>
          <Input
            hasIcon
            placeholder="Search connections, namespaces, or tags..."
            type="text"
          />
        </div>
      </div>
      <div className="flex items-center gap-3">
        {asyncTasks && asyncTasks.activeCount > 0 && (
          <div className="flex items-center gap-2 rounded-full border border-border-dark bg-surface-dark px-3 py-1 text-xs text-slate-300">
            <InlineSpinner className="size-3 border-slate-300" />
            Syncing
          </div>
        )}
        <Button size="sm" variant="ghost" className="p-2">
          <span className="material-symbols-outlined">help_outline</span>
        </Button>
      </div>
    </header>
  );
}
