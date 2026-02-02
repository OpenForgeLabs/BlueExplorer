"use client";

import { useCallback, useEffect, useMemo, useState } from "react";
import { InlineSpinner } from "@/components/feedback/InlineSpinner";
import { AsyncGate } from "@/components/feedback/AsyncGate";
import { ResourceGrid } from "@/features/dashboard/components/ResourceGrid";
import { fetchResourceHealth } from "@/features/dashboard/services/healthService";
import { useResources } from "@/features/dashboard/hooks/useResources";
import { deleteResource } from "@/features/dashboard/services/resourcesService";
import { ResourceStatus, ResourceSummary } from "@/lib/types";
import { useSearchParams } from "next/navigation";

export function DashboardScreen() {
  const searchParams = useSearchParams();
  const selectedType = useMemo(() => {
    const value = searchParams?.get("resource");
    if (value === "redis" || value === "service-bus") {
      return value;
    }
    return "all";
  }, [searchParams]);
  const [viewMode, setViewMode] = useState<"grid" | "list">("grid");
  const [healthAutoRefresh, setHealthAutoRefresh] = useState(false);
  const [healthInterval, setHealthInterval] = useState(30);
  const [healthStatuses, setHealthStatuses] = useState<Record<string, boolean>>(
    {},
  );
  const { data, allResources, isLoading, error, refresh } =
    useResources(selectedType);

  const normalizedResources = useMemo(() => {
    return data.map((resource) => {
      const key = `${resource.type}:${resource.name}`;
      const health = healthStatuses[key];
      if (health === undefined) {
        return resource;
      }
      return {
        ...resource,
        status: (health ? "connected" : "offline") as ResourceStatus,
      };
    });
  }, [data, healthStatuses]);

  const refreshHealth = useCallback(async () => {
    if (!allResources.length) {
      return;
    }
    const response = await fetchResourceHealth(
      allResources.map((resource) => ({
        type: resource.type,
        name: resource.name,
      })),
    );
    if (!response.isSuccess) {
      return;
    }
    setHealthStatuses(response.data.statuses ?? {});
  }, [allResources]);

  const handleDeleteResource = useCallback(
    async (resource: ResourceSummary) => {
      const response = await deleteResource(resource.type, resource.name);
      if (!response.isSuccess) {
        throw new Error(response.message || "Failed to delete resource.");
      }
      await refresh();
    },
    [refresh],
  );

  useEffect(() => {
    if (!healthAutoRefresh) {
      return;
    }

    const timeout = setTimeout(() => {
      void refreshHealth();
    }, 0);

    const interval = setInterval(() => {
      void refreshHealth();
    }, Math.max(5, healthInterval) * 1000);

    return () => {
      clearTimeout(timeout);
      clearInterval(interval);
    };
  }, [healthAutoRefresh, healthInterval, refreshHealth]);

  useEffect(() => {
    if (!allResources.length) {
      return;
    }
    void refreshHealth();
  }, [allResources, refreshHealth]);

  const statusSummary = useMemo(() => {
    return normalizedResources.reduce(
      (summary, resource) => {
        if (resource.status === "connected") {
          summary.connected += 1;
        } else if (resource.status === "warning") {
          summary.warning += 1;
        } else {
          summary.other += 1;
        }
        return summary;
      },
      { connected: 0, warning: 0, other: 0 },
    );
  }, [normalizedResources]);

  return (
    <div className="flex-1 overflow-y-auto bg-background/50 px-4 pb-6 sm:px-6 lg:px-8 lg:pb-8">
          <div className="mb-2 flex flex-wrap items-center justify-end gap-3">
            <div className="flex items-center gap-4 text-xs font-medium uppercase tracking-wider">
              <span className="flex items-center gap-1.5 text-emerald-500">
                <span className="size-2 rounded-full bg-emerald-500"></span>
                {statusSummary.connected} Healthy
              </span>
              <span className="flex items-center gap-1.5 text-amber-500">
                <span className="size-2 rounded-full bg-amber-500"></span>
                {statusSummary.warning} Warning
              </span>
            </div>
          </div>

          <h2 className="mb-6 text-2xl font-bold text-slate-100">
            Active Connections
          </h2>

          <div className="mb-8 flex flex-wrap gap-3">
            <button
              className="flex items-center gap-2 rounded-lg border border-border-dark bg-surface-dark px-3 py-1.5 text-sm font-medium transition-colors hover:border-primary disabled:cursor-not-allowed disabled:opacity-60"
              onClick={refresh}
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
            <div className="flex flex-wrap items-center gap-3 rounded-lg border border-border-dark bg-surface-dark px-3 py-1.5 text-sm">
              <label className="flex items-center gap-2 text-xs text-slate-300">
                <input
                  type="checkbox"
                  className="size-4 rounded border-border-dark bg-background text-primary"
                  checked={healthAutoRefresh}
                  onChange={(event) => setHealthAutoRefresh(event.target.checked)}
                />
                Auto health
              </label>
              <div className="flex items-center gap-2 text-xs text-slate-400">
                <span>Every</span>
                <input
                  className="h-8 w-16 rounded-md border border-border-dark bg-background px-2 text-xs text-slate-200"
                  type="number"
                  min={5}
                  value={healthInterval}
                  onChange={(event) =>
                    setHealthInterval(
                      Math.max(5, Number(event.target.value) || 5),
                    )
                  }
                  disabled={!healthAutoRefresh}
                />
                <span>s</span>
              </div>
            </div>
            <button className="flex items-center gap-2 rounded-lg border border-border-dark bg-surface-dark px-3 py-1.5 text-sm font-medium transition-colors hover:border-primary">
              <span>Environment: All</span>
              <span className="material-symbols-outlined text-[18px]">
                expand_more
              </span>
            </button>
            <button className="flex items-center gap-2 rounded-lg border border-border-dark bg-surface-dark px-3 py-1.5 text-sm font-medium transition-colors hover:border-primary">
              <span>Status: Connected</span>
              <span className="material-symbols-outlined text-[18px]">
                expand_more
              </span>
            </button>
            <button className="flex items-center gap-2 rounded-lg border border-border-dark bg-surface-dark px-3 py-1.5 text-sm font-medium transition-colors hover:border-primary">
              <span>Resource Type</span>
              <span className="material-symbols-outlined text-[18px]">
                expand_more
              </span>
            </button>
            <div className="ml-auto flex items-center rounded-lg bg-surface-dark p-1">
              <button
                className={`rounded px-2 py-1 shadow-sm ${
                  viewMode === "grid"
                    ? "bg-background text-slate-100"
                    : "text-slate-500"
                }`}
                onClick={() => setViewMode("grid")}
                type="button"
              >
                <span className="material-symbols-outlined text-[20px]">
                  grid_view
                </span>
              </button>
              <button
                className={`rounded px-2 py-1 shadow-sm ${
                  viewMode === "list"
                    ? "bg-background text-slate-100"
                    : "text-slate-500"
                }`}
                onClick={() => setViewMode("list")}
                type="button"
              >
                <span className="material-symbols-outlined text-[20px]">
                  view_list
                </span>
              </button>
            </div>
          </div>

      <AsyncGate
        isLoading={isLoading}
        error={error}
        empty={!isLoading && data.length === 0}
      >
        <ResourceGrid
          resources={normalizedResources}
          isLoading={false}
          error={undefined}
          view={viewMode}
          onDeleteResource={handleDeleteResource}
        />
      </AsyncGate>
    </div>
  );
}
