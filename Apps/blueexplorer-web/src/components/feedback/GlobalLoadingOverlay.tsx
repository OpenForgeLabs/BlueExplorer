"use client";

import { LoadingOverlay } from "@/components/feedback/LoadingOverlay";
import { useAsyncTasks } from "@/lib/async/AsyncContext";

export function GlobalLoadingOverlay() {
  const asyncTasks = useAsyncTasks();

  if (!asyncTasks || asyncTasks.activeCount === 0) {
    return null;
  }

  return <LoadingOverlay label="Working..." />;
}
